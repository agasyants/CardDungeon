using System.Formats.Asn1;
using static System.Console;
namespace CardDungeon;

public class Level{
    public  Room[,] rooms;
    public int x = 0;
    public int y = 0;
    public int level_number;
    readonly Random rnd = new();
    public int level_size = 4;
    public Level(Player player, int level_number){
        level_size = level_number + 3;
        int n = rnd.Next(3, level_size);
        int m = level_size - n + 2;
        rooms = new Room[n, m];
        GenerateLevel(n,m);
        this.level_number = level_number;
        WriteLine(level_number + " level");
        EnterRoom(player);
    }
    public void GenerateLevel(int n, int m){
        // generate rooms
        for (int i = 0; i < n; i++){
            for (int j = 0; j < m; j++){
                rooms[i, j] = new EmptyRoom();
            }
        }
        // start room
        rooms[0, 0] = new StartRoom();
        // end room
        int x = rnd.Next(0, n-1) + 1;
        int y = rnd.Next(m-x, m);
        rooms[x, y] = new EndRoom();
        // altar room
        while (true){
            x = rnd.Next(0, n);
            y = rnd.Next(0, m);
            if (rooms[x, y] is EmptyRoom){
                rooms[x, y] = new AltarRoom(this);
                break;
            }
        }
        // enemy room
        int count = rnd.Next(2, level_size);
        for (int i = 0; i < count; i++){
            while (true){
                x = rnd.Next(0, n);
                y = rnd.Next(0, m);
                if (rooms[x, y] is EmptyRoom){
                    int num = rnd.Next(0, 3);
                    if (num == 0)
                        rooms[x, y] = new EnemyRoom(new TO());
                    else
                        rooms[x, y] = new EnemyRoom(new Fool());
                    break;
                }
            }
        }
        // chest room
        count = rnd.Next(2, level_size);
        for (int i = 0; i < count; i++){
            while (true){
                x = rnd.Next(0, n);
                y = rnd.Next(0, m);
                if (rooms[x, y] is EmptyRoom){
                    rooms[x, y] = new ChestRoom();
                    break;
                }
            }
        }
        // map room
        count = rnd.Next(0, 2);
        for (int i = 0; i < count; i++){
            while (true){
                x = rnd.Next(0, n);
                y = rnd.Next(0, m);
                if (rooms[x, y] is EmptyRoom){
                    rooms[x, y] = new MapRoom(this);
                    break;
                }
            }
        }
    }
    public void ShowMap(int x, int y){
        for (int i = rooms.GetLength(1)-1; i >= 0; i--){
            string output = "";
            for (int j = 0; j < rooms.GetLength(0); j++){
                if (j == x && i == y){
                    output += "I ";
                } else if (rooms[j, i].visited || Global.testing){
                    output += rooms[j, i].map_tag + " ";
                } else {
                    output += "? ";
                }
            } 
            WriteLine(output);
        }
    }
    public bool EnterRoom(Player player){
        return rooms[x,y].EnterRoom(player);
    }
    public void Clear(){
        for (int i = 0; i < rooms.GetLength(0); i++){
            for (int j = 0; j < rooms.GetLength(1); j++){
                if (rooms[i, j] is ChestRoom){
                    ChestRoom room = (ChestRoom)rooms[i, j];
                    Chest? chest = room.chest;
                    Loot? loot = room.loot;
                    if (chest != null){
                        chest.ResetChest();
                        loot.ClearLoot();
                    }
                }
            }
        }
    }
}

public abstract class Room {
    public readonly Random rnd = new();
    public string map_tag = " ";
    public bool visited = false;
    public bool EnterRoom(Player player){
        visited = true;
        return _EnterRoom(player);
    }
    public abstract bool _EnterRoom(Player player);
}

public class EnemyRoom : Room {
    private readonly MiniGames game;
    public List<Actor> enemies = [];
    public Loot loot = new();
    public EnemyRoom(MiniGames game){
        this.game = game;
        int num = rnd.Next(0, 10);
        Actor enemy = new("Skeleton", 80, 0);
        enemy.current_armor = 0;
        enemy.hp = 80;
        enemies.Add(enemy);
        map_tag = "1";
        if (num==4) {
            map_tag = "2";
            Actor enemy2 = new("Zombie", 50, 0);
            enemy2.current_armor = 0;
            enemy2.hp = 50;
            enemies.Add(enemy2);
        }
    }
    public override bool _EnterRoom(Player player){
        if (enemies.Count != 0){
            WriteLine("");
            WriteLine("There is enemy in this room");
            game.StartGame(player, enemies);
            if (player.hp <= 0){
                return true;
            } else {
                Input.ProgramSays([], player);
                WriteLine("You win!");
                loot.GenerateLoot();
                loot.UseLoot(player);
                enemies.Clear();
            }
        } else {
            WriteLine("");
            if (loot.IsEmpty)
                WriteLine("Only traces of a heated battle remain here.");
            else
                loot.UseLoot(player);
        } return false;
    }
}

public class AltarRoom : Room {
    public Loot loot = new();
    private readonly Room[,] rooms;
    public AltarRoom(Level level){
        map_tag = "A";
        rooms = level.rooms;
    }
    public override bool _EnterRoom(Player player){
        WriteLine("");
        WriteLine("You are in the altar room");
        Write("Do you want to sacrifice all your cards to the altar? ");
        if (Input.BoolInput("yes", "no", player)){
            WriteLine("You sacrifice all your cards to the altar");
            int num = player.cards.Count;
            Deck.GetInstance().ReturnCards(player.RemoveCards([]));
            player.cards.AddRange(Deck.GetInstance().GetCards(num-1));
            for (int i = 0; i < rooms.GetLength(0); i++){
                for (int j = 0; j < rooms.GetLength(1); j++){
                    if (rooms[i, j] is EndRoom){
                        WriteLine("The goddess showed your way");
                        rooms[i,j].visited = true;
                        return false;
                    }
                }
            }
        } 
        return false;
    }
}

public class ChestRoom : Room {
    public Chest? chest;
    public Loot loot = new();
    public ChestRoom(){
        map_tag = "C";
        chest = new Chest();
    }
    public override bool _EnterRoom(Player player){
        if (chest != null){
            WriteLine("");
            if (chest.open == false){
                WriteLine("There is the closed chest in this room");
                if (chest.lock_cards.Count == 0){
                    int num2 = rnd.Next(0, 6);
                    if (num2 < 3)
                        num2 = 1;
                    else if (num2>4)
                        num2 = 3;
                    else
                        num2 = 2;
                    chest.lock_cards.AddRange(Deck.GetInstance().GetCards(num2));
                }
                Input.ShowCards("It closed with this cards:", chest.lock_cards);
                Write("Do you want to try to open it? ");
                if (Input.BoolInput("yes", "no", player)){
                    WriteLine();
                    WriteLine("If you want to open it you have to beat every card");
                    player.ShowCards();
                    var card_input = Input.MultipleInput(player.cards.Count, player);
                    if (Input.IsBeat(chest.lock_cards, player.GetCards(card_input))){
                        WriteLine();
                        WriteLine("You open the chest!!!");
                        loot.GenerateLoot();
                        chest.open = true;
                        Deck.GetInstance().ReturnCards(chest.lock_cards);
                        chest.lock_cards.Clear();
                        Deck.GetInstance().ReturnCards(player.RemoveCards(card_input));
                        loot.UseLoot(player);
                    } else {
                        WriteLine("You don't beat");
                        WriteLine("");
                    }
                }
            } else {
                if (loot.IsEmpty){
                    WriteLine("There is the empty chest in this room.");
                } else {
                    WriteLine("There is the open chest in this room.");
                    loot.UseLoot(player);
                }
            } 
        } return false;
    }
}

public class StartRoom : Room {
    public StartRoom(){
        map_tag = "S";
    }
    public override bool _EnterRoom(Player player){
        WriteLine("");
        WriteLine("You are in the start room");
        return false;
    }
}

public class EndRoom : Room {
    public EndRoom(){
        map_tag = "x";
    }
    public override bool _EnterRoom(Player player){
        WriteLine("");
        WriteLine("In this room there is a descent down to the next level!");
        Write("Do you want to continue? ");
        if (Input.BoolInput("yes","no", player)){
            //exit level
            return true;
        } return false;
    }
}

public class EmptyRoom : Room {
    public EmptyRoom(){
        map_tag = " ";
    }
    public override bool _EnterRoom(Player player){
        WriteLine("");
        WriteLine("You are in the empty room");
        return false;
    }
}

public class MapRoom : Room {
    private readonly Room[,] rooms;
    public MapRoom(Level level){
        map_tag = "M";
        rooms = level.rooms;
    }
    public override bool _EnterRoom(Player player){
        WriteLine("");
        WriteLine("You are in the map room");
        Write("Do you want to fill the gaps in the map? ");
        if (Input.BoolInput("yes","no", player)){
            for (int i = 0; i < rooms.GetLength(0); i++){
                for (int j = 0; j < rooms.GetLength(1); j++){
                    rooms[i,j].visited = true;
                }
            }
        }
        return false;
    }
}
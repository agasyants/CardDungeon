using System.ComponentModel;
using System.Dynamic;
using static System.Console;

namespace CardDungeon;

public class Card(Rank rank, Suit suit) {
    public Rank rank = rank;
    public Suit suit = suit;
    public string Print(){
        string result = "<" + rank.ToString() +" of ";
        if (suit == Deck.GetInstance().trump)
            return result + suit.ToString().ToUpper() + ">";
        else
            return result + suit.ToString() + ">";
    }
}
public sealed class Deck{ //singleton
    public static Deck instance = new();
    public List<Card> cards = [];
    public Card? trump_card;
    public Suit trump;
    public void MakeTrump(){ 
        trump_card = GetCards(1)[0];
        trump = trump_card.suit;
    }

    public static Deck GetInstance(){
        instance ??= new Deck();
        return instance;
    }
    private Deck(){
        foreach (Suit suit in Enum.GetValues(typeof(Suit))){
            foreach (Rank rank in Enum.GetValues(typeof(Rank))){
                cards.Add(new Card(rank, suit));
            }
        }
        MakeTrump();
    }
    public List<Card> GetCards(int count){
        Random rnd = new();
        List<Card> result = [];
        if (count > cards.Count){
            count = cards.Count;
        } for (int i = 0; i < count; i++){
            int num = rnd.Next(0, cards.Count);
            Card card = cards[num];
            cards.RemoveAt(num);
            result.Add(card);
        } return result;
    }
    public void ReturnCards(List<Card> cards){
        foreach (Card card in cards){
            this.cards.Add(card);
        }
    }
}

public class Chest{
    public bool open = false;
    public List<Card> lock_cards = [];
    public List<Card> content_cards = [];
    public void RemoveContentCards(List<Card> cards){
        foreach (Card card in cards){
            content_cards.Remove(card);
        }
    }
    public void GetCards(Actor player){
        WriteLine("Cards in chest:");
        for (int i = 0; i < content_cards.Count; i++){
            WriteLine("  " + content_cards[i].Print());
        }
        Write("What card do you want to get? ");
        var input = Input.MultipleInput(content_cards.Count);
        if (input.Count == 0){
            player.cards.AddRange(content_cards);
            content_cards.Clear();
        } else {
            List<Card> cards = [];
            foreach (int i in input)
                cards.Add(content_cards[i]);
            RemoveContentCards(cards);
            player.cards.AddRange(cards);
        }
    }
    public void ResetChest(){
        Deck.GetInstance().ReturnCards(lock_cards);
        lock_cards = [];
        Deck.GetInstance().ReturnCards(content_cards);
        content_cards = [];
    }
}

public class Room{
    public RoomType roomType = RoomType.Empty;
    public List<Actor> enemies = [];
    public Chest? chest;
    public string map_tag = " ";
    private MiniGames game;
    public Room(MiniGames game){
        this.game = game;
        Random rnd = new();
        int num = rnd.Next(0, 10);
        if (num == 0){
            map_tag = "A";
            roomType = RoomType.Altar;
        } else if (num >= 1 && num < 5){
            map_tag = "E";
            roomType = RoomType.Enemy;
            Actor enemy = new("Skeleton");
            enemy.armor = 0;
            enemy.hp = 80;
            enemies.Add(enemy);
            if (num==4) {
                Actor enemy2 = new("Zombie");
                enemy2.armor = 0;
                enemy2.hp = 50;
                enemies.Add(enemy2);
            }
        } else if (num >= 7){
            map_tag = "C";
            chest = new Chest();
            roomType = RoomType.Chest;
        }
    }
  
    public bool EnterRoom(Actor player){
        if (roomType == RoomType.Enemy){
            if (enemies.Count != 0){
                WriteLine("");
                WriteLine("There is enemy in this room");
                game.StartGame(player, enemies);
                if (player.hp <= 0){
                    return true;
                } else {
                    WriteLine("You win!");
                    player.cards.AddRange(Deck.GetInstance().GetCards(2));
                    enemies.Clear();
                }
            } else {
                WriteLine("");
                WriteLine("Only traces of a heated battle remain here.");
            }
        } else if (roomType == RoomType.Chest && chest != null){
            WriteLine("");
            if (chest.open == false){
                WriteLine("There is the closed chest in this room");
                Random rnd = new();
                int num2 = rnd.Next(0, 6);
                if (num2 < 3)
                    num2 = 1;
                else if (num2>4)
                    num2 = 3;
                else
                    num2 = 2;
                chest.lock_cards.AddRange(Deck.GetInstance().GetCards(num2));
                Input.ShowCards("It closed with this cards:", chest.lock_cards);
                Write("Do you want to try to open it? ");
                if (Input.BoolInput("yes","no")){
                    WriteLine();
                    WriteLine("If you want to open it you have to beat every card");
                    player.ShowCards();
                    var card_input = Input.MultipleInput(player.cards.Count);
                    if (Input.IsBeat(chest.lock_cards, player.GetCards(card_input))){
                        WriteLine("You open the chest!!!");
                        int num = rnd.Next(0, 3);
                        chest.content_cards.AddRange(Deck.GetInstance().GetCards(num+1));
                        chest.open = true;
                        Deck.GetInstance().ReturnCards(chest.lock_cards);
                        chest.lock_cards.Clear();
                        Deck.GetInstance().ReturnCards(player.RemoveCards(card_input));
                        chest.GetCards(player);
                    } else {
                        WriteLine("lose");
                        WriteLine("");
                    }
                }
            } else {
                if (chest.content_cards.Count == 0){
                    WriteLine("There is the empty chest in this room.");
                } else {
                    WriteLine("There is the open chest in this room.");
                    chest.GetCards(player);
                }
            }
        } else if (roomType == RoomType.End){
            WriteLine("");
            WriteLine("In this room there is a descent down to the next level!");
            Write("Do you want to continue? ");
            if (Input.BoolInput("yes","no")){
                //exit level
                return true;
            }
        } else if (roomType == RoomType.Start){
            WriteLine("");
            WriteLine("You are in the start room");
        } else if (roomType == RoomType.Empty){
            WriteLine("");
            WriteLine("You are in the empty room");
        } else if (roomType == RoomType.Boss){
            WriteLine("");
            WriteLine("Boss");
        } else if (roomType == RoomType.Shop){
            WriteLine("");
            WriteLine("Shop");
        } else if (roomType == RoomType.Trap){
            WriteLine("");
            WriteLine("Trap");
        } else if (roomType == RoomType.Altar){
            WriteLine("");
            WriteLine("You are in the altar room");
            Write("Do you want to sacrifice all your cards to the altar? ");
            if (Input.BoolInput("yes", "no")){
                WriteLine("You sacrifice all your cards to the altar");
                int num = player.cards.Count;
                Deck.GetInstance().ReturnCards(player.RemoveCards([]));
                player.cards.AddRange(Deck.GetInstance().GetCards(num-1));
            }
        } else {
            WriteLine("ERROR");
            WriteLine("");
        } return false;
    }
}
public class Loot{
    List<Card> cards = [];
    public bool health_potion = false;
    public bool armor_potion = false;
    public bool phenix_stone = false;
    public Loot(){
        Random rnd = new();
        int num = rnd.Next(0, 12);
        if (num == 0 || num == 1){
            health_potion = true;
        } else if (num == 2 || num == 3){
            armor_potion = true;
        } else if (num == 4){
            phenix_stone = true;
        } else {

            cards = Deck.GetInstance().GetCards(1);
        }
    }
}
public class Hand{
    List<Card> cards = [];
    public List<Card> GetCards(List<int> card_index){
        List<Card> result = [];
        foreach (int i in card_index){
            result.Add(cards[i]);
        } return result;
    }
    public List<Card> RemoveCards(List<int> card_index){
        List<Card> result = [];
        if (card_index.Count == 0){
            result = cards;
            cards.Clear();
        } else {
            card_index.Sort();
            card_index.Reverse();
            foreach (int i in card_index){
                result.Add(cards[i]);
                cards.RemoveAt(i);
            } 
        } return result;
    }
    public void ShowCards(){
        for (int i = 0; i < cards.Count; i++){
            Console.WriteLine(i+1 + ". " + cards[i].rank + " of " + cards[i].suit);
        }
    }
}
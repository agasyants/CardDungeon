using System.ComponentModel.Design;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using Microsoft.VisualBasic;
using static System.Console;
namespace CardDungeon;

public class Input{
    public static bool BoolInput(string word_true, string word_false){
        bool flag = true;
        bool result = false;
        while (flag) {
            var input = ReadLine();
            if (input == word_true){
                result = true;
                flag = false;
            } else if (input == word_false){
                result = false;
                flag = false;
            } else {
                WriteLine("Wrong input");
            }
        } return result;
    }
    public static void ProgramSays(List<string> strings){
        foreach (string str in strings) {
            WriteLine(str);
        }
        var input = ReadLine();
    } 
    public static string HighlightTrump(Suit suit){
        string result = "";
        if (suit == Deck.GetInstance().trump){
            result = suit.ToString().ToUpper();
        } else {
            result = suit.ToString();
        } return result;
    }
    public static void ShowCards(string frase, List<Card> cards){
        if (cards.Count == 0){
            return;
        }
        WriteLine(frase);
        for (int i = 0; i < cards.Count; i++){
            WriteLine(i+1 + ". " + cards[i].rank + " of " + HighlightTrump(cards[i].suit));
        }
    }
    public static bool IsBeat(List<Card> first, List<Card> second){
        if (first.Count != second.Count){
            return false;
        }
        for (int i = 0; i < first.Count; i++){
            if (!CompareCards(first[i], second[i])){
                return false;
            }
        } return true;
    }
    public static bool CompareCards(Card first, Card second) {
        Suit trump = Deck.GetInstance().trump;
        if (second.suit == trump && first.suit != trump){
            return true;
        } else if (first.suit == second.suit && (first.rank < second.rank || (first.rank == Rank.Ace && second.rank == Rank.Two))){
            return true;
        } else {
            return false;
        }
    }
    public static List<int> MultipleInput(int max_count) {
        List<int> result = new List<int>();
        bool flag = true;
        while (flag) {
            Console.Write("Input cards: ");
            result = new List<int>();
            flag = false;
            var R = Console.ReadLine();
            if (R is null){
                continue;
            }
            string[] input = R.Split(' ');
            if (input.Length <= max_count) {
                foreach (string i in input) {
                    if (int.TryParse(i, out int num)) {
                        if (num <= max_count && num > 0 && !result.Contains(num-1)) {
                            result.Add(num - 1);
                        } else {
                            flag = true;
                            break;
                        }
                    } else {
                        flag = true;
                        break;
                    }
                }
            } 
            else {
                flag = true;
            }
        } return result;
    }
    public static void ClearLevel(Room[,] rooms){
        for (int i = 0; i < rooms.GetLength(0); i++){
            for (int j = 0; j < rooms.GetLength(1); j++){
                Chest? chest = rooms[i, j].chest;
                if (chest != null){
                    chest.ResetChest();
                }
            }
        }
    }
    public static void ShowLevel(Room[,] rooms, int x, int y){
        for (int i = rooms.GetLength(1)-1; i >= 0; i--){
            string output = "";
            for (int j = 0; j < rooms.GetLength(0); j++){
                if (j == x && i == y){
                    output += "I ";
                } else {
                    output += rooms[j, i].map_tag + " ";
                }
            } 
            WriteLine(output);
        }
    }
}
class Program{
    static void Main(string[] args){
        // starting game
        bool testing = true;
        Actor player = new("Player");
        player.cards = Deck.GetInstance().GetCards(6);
        Random rnd = new();
        int n = 3;
        int m = 3;
        WriteLine("Trump: " + Deck.GetInstance().trump.ToString().ToUpper());
        WriteLine();
        for (int level_number = 1; level_number < 15; level_number++)
        {
            player.armor = 80;
            // genegating level
            Room[,] rooms = new Room[n, m];
            for (int i = 0; i < n; i++){
                for (int j = 0; j < m; j++){
                    rooms[i, j] = new Room(new Fool());
                }
            }
            rooms[0, 0].roomType = RoomType.Start;
            rooms[0, 0].map_tag = "S";
            int end_x = rnd.Next(0, n-1) + 1;
            int end_y = rnd.Next(m-end_x, m);
            rooms[end_x, end_y].roomType = RoomType.End;
            rooms[end_x, end_y].enemies.Clear();
            rooms[end_x, end_y].chest = null;
            rooms[end_x, end_y].map_tag = "x";
            WriteLine(level_number + " level");
            WriteLine("You are in the start room");
            int x = 0;
            int y = 0;
            while (player.hp > 0){
                // input
                //WriteLine("x "+(x+1) + " y " + (y+1));
                //WriteLine(Deck.GetInstance().cards.Count);
                var input = ReadLine();
                if (input == "show"){
                    player.ShowCards();
                    WriteLine("");
                    continue;
                } else if (input == "test") {
                    testing = true;
                    continue;
                } else if (input == "map" && testing) {
                    Input.ShowLevel(rooms, x, y);
                    continue;
                } else if (input == "a" && x > 0) {
                    x--;
                } else if (input == "d" && x < n - 1) {
                    x++;
                } else if (input == "w" && y < m - 1) {
                    y++;
                } else if (input == "s" && y > 0) {
                    y--;
                } else if (input != "a" && input != "w" && input != "s" && input != "d"){
                    WriteLine("");
                    WriteLine("...");
                    continue;
                } else {
                    WriteLine("");
                    WriteLine("Wall");
                    continue;
                }
                // cheking room
                if (rooms[x, y].EnterRoom(player)) {
                    break;
                }
            }
            // update level
            if (player.hp <= 0){
                break;
            }
            Input.ClearLevel(rooms);
            if (n==m){
                n++;
            } else {
                m++;
            }
        } WriteLine("Game over");
    }
}
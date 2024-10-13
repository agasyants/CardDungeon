using System.ComponentModel.Design;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using Microsoft.VisualBasic;
using static System.Console;
namespace CardDungeon;

public static class Global{
    public static bool testing = false;
}
public class Input{
    public static bool BoolInput(string word_true, string word_false, Player player){
        bool flag = true;
        bool result = false;
        while (flag) {
            var input = ReadLine();
            if (input is not null){
                input = input.ToString().ToLower();
                if (input.Contains(word_true)){
                    result = true;
                    flag = false;
                } else if (input.Contains(word_false)){
                    result = false;
                    flag = false;
                } else {
                    WriteLine("Wrong input");
                }
            }
            
        } return result;
    }
    public static void ProgramSays(List<string> strings, Player player){
        foreach (string str in strings) {
            WriteLine(str);
        }
        var input = In(player);
    } 
    public static void ShowCards(string frase, List<Card> cards){
        if (cards.Count == 0){
            return;
        }
        WriteLine(frase);
        for (int i = 0; i < cards.Count; i++){
            WriteLine(i+1 + ". " + cards[i].Print());
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
    public static List<int> MultipleInput(int max_count, Player player) {
        List<int> result = new List<int>();
        bool flag = true;
        while (flag) {
            Write("Input cards: ");
            result = new List<int>();
            flag = false;
            var R = In(player);
            if (R is null){
                continue;
            }
            string[] input = R.Split(' ');
            if (input[0]=="") {
                return result;
            }
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
    public static bool MovementInput(Player player, Level level){
        WriteLine("Where we go?");
        // input
        if (Global.testing){
            WriteLine($"x {level.x+1} y {level.y+1}");
            WriteLine(Deck.GetInstance().cards.Count);
        }
        var input = In(player);
        if (input == "e") {
            return false;
        } else if (input == "exit") {
            player.hp = 0;
            return true;
        } else if (input == "map") {
            level.ShowMap(level.x,level.y);
            return true;
        } else if (input == "a" && level.x > 0) {
            level.x--;
        } else if (input == "d" && level.x < level.rooms.GetLength(0) - 1) {
            level.x++;
        } else if (input == "w" && level.y < level.rooms.GetLength(1) - 1) {
            level.y++;
        } else if (input == "s" && level.y > 0) {
            level.y--;
        } else if (input != "a" && input != "w" && input != "s" && input != "d"){
            WriteLine("");
            WriteLine("Wrong input");
            return true;
        } else {
            WriteLine("");
            WriteLine("Wall");
            return true;
        } 
        return false;
    }
    public static string In(Player player){
        string input = "";
        while (true){
            input = ReadLine() ?? "";
            if (input == "show"){
                player.ShowInventory();
                player.PrintHP();
                WriteLine("");
            } else if (input == "heal") {
                player.UseHeal();
            } else if (input == "test") {
                Global.testing = true;
            } else {
                break;
            }
        } return input;
    }
    // keyInput
    // Console.Key
    public static void StartFrase(){
        WriteLine("show - show inventory");
        WriteLine("test - test mode");
        WriteLine("map - show map");
        WriteLine("heal - use health potion");
        WriteLine("exit - exit game");
        WriteLine("a, w, s, d - move");
        WriteLine("e - interaction");
    }
    public static string InputName(){
        string name = ReadLine() ?? "Player";
        if (name == ""){
            name = "Player";
        } return name;
    }
}
class Program{
    static void Main(string[] args){
        // starting game
        WriteLine("Welcome to CardDungeon");
        Write("What's your name? ");
        Player player = new(Input.InputName(), 50, 80);
        while (true){
            Game(player);
            if (player.hp <= 0){
                WriteLine("GAME OVER");
                WriteLine("Do you want to restart?");
                if (Input.BoolInput("yes", "no", player)){
                    player = new("Player", 50, 80);
                } else { 
                    break;
                }
            } else {
                WriteLine("You've completed the game! Congratulations!");
            }
        }
        
    }
    static void Game(Player player){
        // game
        Input.StartFrase();
        for (int level_number = 1; level_number <= 10; level_number++)
        {
            player.current_armor = player.max_armor;
            if (level_number % 5 == 0)
                Deck.GetInstance().MakeTrump();
            // genegating level
            Level level = new(player, level_number);
            while (player.hp > 0){
                if (Input.MovementInput(player, level))
                    continue;
                level.EnterRoom(player);
            }
            if (player.hp <= 0)
                return;
            // update level
            level.Clear();
            level.level_size++;
        }
    }
}
﻿using System.ComponentModel.Design;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using Microsoft.VisualBasic;
using static System.Console;
namespace CardDungeon;

public static class Global{
    public static bool testing = false;
}

public class Input{
    public static bool BoolInput(string word_true, string word_false){
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
    public static void ProgramSays(List<string> strings){
        foreach (string str in strings) {
            WriteLine(str);
        }
        var input = ReadLine();
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
    public static List<int> MultipleInput(int max_count) {
        List<int> result = new List<int>();
        bool flag = true;
        while (flag) {
            Write("Input cards: ");
            result = new List<int>();
            flag = false;
            var R = ReadLine();
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
        Actor player = new("Player", 50, 80);
        player.cards = Deck.GetInstance().GetCards(6);
        Random rnd = new();
        int s = 4;
        WriteLine("show - show inventory");
        WriteLine("test - test mode");
        WriteLine("map - show map");
        WriteLine("heal - use health potion");
        WriteLine("a, w, s, d - move");
        WriteLine("e - interaction");
        WriteLine("Trump: " + Deck.GetInstance().trump.ToString().ToUpper());
        WriteLine();
        for (int level_number = 1; level_number <= 10; level_number++)
        {
            player.current_armor = player.max_armor;
            // genegating level
            int n = rnd.Next(3, s);
            int m = s - n + 2;
            Room[,] rooms = new Room[n, m];
            for (int i = 0; i < n; i++){
                for (int j = 0; j < m; j++){
                    if (Global.testing)
                        rooms[i, j] = new Room(new FoolImp());
                    else
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
                WriteLine("Where we go?");
                // input
                if (Global.testing){
                    WriteLine("x "+(x+1) + " y " + (y+1));
                    WriteLine(Deck.GetInstance().cards.Count);
                }
                var input = ReadLine();
                if (input == "show"){
                    player.ShowCards();
                    player.PrintHP();
                    WriteLine("");
                    continue;
                } else if (input == "e") {
                    if (rooms[x, y].EnterRoom(player))
                        break;
                    else
                        continue;
                } else if (input == "heal") {
                    player.UseHeal();
                } else if (input == "test") {
                    Global.testing = true;
                    continue;
                } else if (input == "map") {
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
            s++;
        } WriteLine("GAME OVER");
    }
}
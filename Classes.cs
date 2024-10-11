using System.CodeDom.Compiler;
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
    public Suit trump = Suit.Clubs;
    public void MakeTrump(){ 
        Card trump_card = GetCards(1)[0];
        if (trump_card != null)
            ReturnCards([trump_card]);
        trump_card = GetCards(1)[0];
        trump = trump_card.suit;
        WriteLine("New trump is " + Deck.GetInstance().trump.ToString().ToUpper());
        ReturnCards([trump_card]);
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
    public void ResetChest(){
        Deck.GetInstance().ReturnCards(lock_cards);
        lock_cards = [];
    }
}

public class Loot{
    public bool IsEmpty = false;
    List<Card> cards = [];
    public bool health_potion = false;
    public int armor = 0;
    public bool phenix_stone = false;
    public string[] possible_armor = ["Helmet", "Lats", "Cuirass", "Boots", "Leggings", "Bracers"];
    public void GenerateLoot(){
        Random rnd = new();
        int num = rnd.Next(0, 12);
        if (num == 0 || num == 1){
            health_potion = true;
        } else if (num == 2 || num == 3){
            armor = 10 + rnd.Next(0,20);
        } else if (num == 4){
            phenix_stone = true;
        } else {
            int num2 = rnd.Next(0, 3);
            if (num2 == 0)
                cards = Deck.GetInstance().GetCards(1);
            else if (num2 == 1)
                cards = Deck.GetInstance().GetCards(2);
            else if (num2 == 2)
                cards = Deck.GetInstance().GetCards(3);
        }
    }
    public void UseLoot(Player player){
        WriteLine("Your loot is...");
        if (health_potion){
            WriteLine("Health potion!");
            Write("Do you want to get it? ");
            if (Input.BoolInput("yes", "no", player)){
                player.health_potions++;
            }
        } else if (armor != 0){
            Random rnd = new();
            int num = rnd.Next(0, possible_armor.Length);
            WriteLine(possible_armor[num] + "! (" + armor + " arm)");
            Write("Do you want to get it? ");
            if (Input.BoolInput("yes", "no", player)){
                player.GetArmor(armor);
            }
        } else if (phenix_stone){
            WriteLine("Phenix stone!");
            if (player.phenix_stone){
                WriteLine("But you already heave one, so you can't take it(");
            } else{
                Write("Do you want to get it? ");
                if (Input.BoolInput("yes", "no", player)){
                    player.phenix_stone = true;
                }
            }
        } else if (cards.Count()>0){
            WriteLine("Cards!");
            for (int i = 0; i < cards.Count; i++){
                WriteLine("  " + cards[i].Print());
            }
            Write("What card do you want to get? ");
            var input = Input.MultipleInput(cards.Count, player);
            if (input.Count == 0){
                player.cards.AddRange(cards);
                cards.Clear();
            } else {
                List<Card> cards = [];
                foreach (int i in input)
                    cards.Add(cards[i]);
                cards.Clear();
                player.cards.AddRange(cards);
            }
        } 
        if (!health_potion && armor == 0 && !phenix_stone && cards.Count == 0)
            IsEmpty = true;
    }
    public void ClearLoot(){
        health_potion = false;
        armor = 0;
        phenix_stone = false;
        Deck.GetInstance().ReturnCards(cards);
        cards.Clear();
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


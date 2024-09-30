namespace CardDungeon;
using static System.Console;

abstract public class MiniGames {
    public abstract void StartGame(Actor player, List<Actor> enemies);
    public static bool CanPut(List<Card> cards){
        if (cards.Count == 1){
            return true;
        }
        int num = (int)cards[0].rank;
        for (int i=1; i<cards.Count; i++){
            if ((int)cards[i].rank != num){
                return false;
            }
        } return true;
    }
    public static bool CanAdd(List<Card> cards, int n){
        for (int i=0; i<cards.Count; i++){
            if ((int)cards[i].rank != n){
                return false;
            }
        } return true;
    }
    public static bool SumEnemiessHP(List<Actor> actors){
        int sum = 0;
        foreach (Actor actor in actors){
            sum += actor.hp;
        } return sum > 0;
    }
    public static int UpdateIndex(int index, int max){
        if (index == max){
            return 0;
        } else {
            return index+1;
        }
    }
    public static void PrintHP(List<Actor> actors){
        foreach (Actor actor in actors){
            actor.PrintHP();
        }
    }
    public static void AddCardTo(Actor player, List<Actor> enemies, int to){
        foreach (Actor enemy in enemies){
            enemy.cards.AddRange(Deck.GetInstance().GetCards(to-enemy.cards.Count));
        }
        List<Card> add_cards = Deck.GetInstance().GetCards(to-player.cards.Count);
        if (add_cards.Count != 0){
            if (add_cards.Count==1)
                WriteLine("You get 1 card:");
            else 
                WriteLine("You get "+ add_cards.Count +" cards:");
            foreach (Card card in add_cards){
                WriteLine(card.rank +" of "+ card.suit);
            }
            player.cards.AddRange(add_cards);
        }
    }
    public static void ShowBeatCards(List<Card> table, List<Card> beat){
        for (int i=0; i<table.Count; i++){
            WriteLine(table[i].rank+" of "+table[i].suit+" <== "+beat[i].rank+" of "+beat[i].suit);
        }
    } 
    public static void MassiveDamage(List<Card> cards, Actor player, List<Actor> enemies, Actor safe){
        if (safe == player){
            foreach (Actor enemy in enemies){
                enemy.GetDamage(cards,1);
            }
        } else {
            player.GetDamage(cards,1);
            foreach (Actor enemy in enemies){
                if (enemy != safe)
                    enemy.GetDamage(cards,1);
            }
        }
    }
}

public class Fool: MiniGames {
    public override void StartGame(Actor player, List<Actor> enemies) {
        // who first
        foreach (Actor enemy in enemies){
            enemy.cards = Deck.GetInstance().GetCards(6);
        }
        int[] actors_min = new int[enemies.Count];
        foreach (Actor actor in enemies){
            int[] ranks = new int[actor.cards.Count];
            for (int i = 0; i < actor.cards.Count; i++){
                ranks[i] = (int)actor.cards[i].rank;
            } actors_min[enemies.IndexOf(actor)] = ranks.Min();
        }
        int index = Array.IndexOf(actors_min, actors_min.Max());
        if (index != 0)
            PrintHP(enemies);
        // fight starts
        List<Card> table = [];
        while (player.hp > 0 && SumEnemiessHP(enemies)){
            // player turn
            if (index == 0){
                // player attack
                Input.ShowCards("Cards on the table:", table);
                if (table.Count == 0){
                    PrintHP(enemies);
                    player.PrintHP();
                    Input.ProgramSays([]);
                    WriteLine("Your turn");
                    WriteLine("Your cards: ");
                    player.ShowCards();
                    while (table.Count==0){
                        var input = Input.MultipleInput(player.cards.Count);
                        if (CanPut(player.GetCards(input))){
                            table.AddRange(player.RemoveCards(input));
                            index = UpdateIndex(index, enemies.Count);
                        } else {
                            WriteLine("Wrong input");
                        }
                    } WriteLine();
                } else {
                    // player defend
                    WriteLine("Your cards: ");
                    player.ShowCards();
                    int n = table.Count;
                    while (table.Count==n){
                        var input = Input.MultipleInput(player.cards.Count);
                        bool flag1 = Input.IsBeat(table, player.GetCards(input));
                        bool flag2 = CanAdd(player.GetCards(input),(int)table[0].rank);
                        if (flag1 && flag2) {
                            Write("Beat cards or add cards? ");
                            if (Input.BoolInput("beat","add")){
                                flag2 = false;
                            } else {
                                flag1 = false;
                            }
                        } 
                        if ((!flag1)&&(!flag2)) {
                            WriteLine("You don't beat");
                            player.GetDamage(table,2);
                            table.Clear();
                        }
                        if (flag1){
                            WriteLine("You beat cards");
                            table.AddRange(player.RemoveCards(input));
                            // choose enemy
                            MassiveDamage(table, player, enemies, player);
                            table.Clear();
                            AddCardTo(player, enemies, 6);
                        } 
                        if (flag2) {
                            // we add
                            WriteLine("You add");
                            table.AddRange(player.RemoveCards(input));
                            index = UpdateIndex(index, enemies.Count);
                        } 
                    }
                } 
            // enemy turn
            } else {
                Actor enemy = enemies[index-1];
                if (Global.testing){
                    enemy.ShowCards();
                }
                if (table.Count == 0){
                    // enemy attack
                    WriteLine(enemy.name + " turn");
                    // check if had same cards
                    // dictionary
                    // choose random card
                    var rnd = new Random();
                    int num = rnd.Next(0, enemy.cards.Count);
                    WriteLine(enemy.name+" put "+enemy.cards[num].rank+" of "+enemy.cards[num].suit +" on the table");
                    table.Add(enemy.cards[num]);
                    enemy.cards.RemoveAt(num);
                    index = UpdateIndex(index, enemies.Count);
                } else {
                    // enemy defend
                    int num = (int)table[0].rank;
                    bool flag = true;
                    // add cards
                    for (int i = 0; i < enemy.cards.Count; i++){
                        if ((int)enemy.cards[i].rank == num){
                            table.Add(enemy.cards[i]);
                            flag = false;
                            WriteLine(enemy.name+" add "+enemy.cards[i].rank+" of "+enemy.cards[i].suit);
                            enemy.cards.RemoveAt(i);
                            index = UpdateIndex(index, enemies.Count);
                        }
                    } 
                    // beat cards
                    if (flag){
                        List<Card> beat_cards = [];
                        // looking for the best cards
                        foreach (Card card in table){
                            List<Card> true_card = [];
                            List<int> true_card_index = [];
                            foreach (Card enemy_card in enemy.cards){
                                if (card.suit==enemy_card.suit && card.rank<enemy_card.rank){
                                    true_card.Add(enemy_card);
                                    true_card_index.Add((int)enemy_card.rank);
                                }
                            }
                            if (true_card.Count == 0 && card.suit != Deck.GetInstance().trump){
                                foreach (Card enemy_card in enemy.cards){
                                    if (enemy_card.suit == Deck.GetInstance().trump){
                                        true_card.Add(enemy_card);
                                        true_card_index.Add((int)enemy_card.rank);
                                    }
                                } 
                            } 
                            if (true_card.Count == 0) {
                                flag = false;
                            } else {
                                Card card_to_beat = true_card[true_card_index.IndexOf(true_card_index.Min())];
                                beat_cards.Add(card_to_beat);
                                enemy.cards.Remove(card_to_beat);
                            }
                        } 
                        if (flag) {
                            // if enemy can beat the cards
                            WriteLine(enemy.name + " beat your cards");
                            ShowBeatCards(table,beat_cards);
                            table.AddRange(beat_cards);
                            MassiveDamage(table, player, enemies, enemy);
                            Deck.GetInstance().RerurnCards(table);
                            table.Clear();
                        } else {
                            // if enemy can't beat the cards
                            WriteLine(enemy.name+" don't beat cards");
                            enemy.cards.AddRange(beat_cards);
                            enemy.GetDamage(table,2);
                            Deck.GetInstance().RerurnCards(table);
                            table.Clear();
                        } AddCardTo(player, enemies, 6);
                    }
                } Input.ProgramSays([]);
            } 
        if (player.hp <= 0)
            WriteLine("You died");
        else {
            foreach (Actor enemy in enemies){
                if (enemy.hp <= 0){
                    WriteLine(enemy.name + " died");
                    Deck.GetInstance().RerurnCards(enemy.cards);
                    enemies.Remove(enemy);
                    enemy.cards = [];
                }
            } 
        }
        player.cards.AddRange(Deck.GetInstance().GetCards(2));
        WriteLine("You win the fight!!!");
        }
    }
}
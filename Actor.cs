namespace CardDungeon;
public class Actor {
    public int hp;
    public int max_hp;
    public int max_armor;
    public int current_armor;
    public string name;
    public List<Card> cards = [];
    public Actor(string name, int hp, int armor){
        this.name = name;
        this.hp = hp;
        this.max_hp = hp;
        this.max_armor = armor;
        this.current_armor = armor;
    }
    public void PrintHP(){
        if (current_armor<=0)
            Console.WriteLine($"{name} hp: {hp}");
        else
            Console.WriteLine($"{name} hp: {hp}, armor {current_armor}");
    }
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
        } 
        return result;
    }
    public void ShowCards(){
        for (int i = 0; i < cards.Count; i++){
            Console.WriteLine(i+1 + ". " + cards[i].Print());
        }
    }
    public virtual void Death(){
        Deck.GetInstance().ReturnCards(RemoveCards([]));
        Console.WriteLine(name + " died");
    }
    public bool GetDamage(List<Card> cards, int multiplier){
        int damage = 0;
        foreach (Card card in cards){
            damage += ((int)card.rank + 2)*multiplier;
        }
        if (multiplier==1)
            Console.WriteLine(name + " get " + damage + " damage");
        else
            Console.WriteLine($"{name} get {damage} damage (x{multiplier})");
        if (current_armor > 0){
            current_armor -= damage;
            if (current_armor < 0){
                hp += current_armor;
                current_armor = 0;
            }
        } else {
            hp -= damage;
        }
        if (hp <= 0){
            Death();
            return false;
        } return true;
    }
}

public class Player : Actor {
    public bool phenix_stone = false;
    public int health_potions = 0;
    public Player(string name, int hp, int armor) : base(name, hp, armor){
        cards = Deck.GetInstance().GetCards(6);
    }
    public void AddCards(List<Card> add_cards){
        if (add_cards.Count != 0){
            if (add_cards.Count==1)
                Console.WriteLine("You get 1 card:");
            else 
                Console.WriteLine($"You get {add_cards.Count} cards:");
            foreach (Card card in add_cards){
                Console.WriteLine(card.Print());
            }
            cards.AddRange(add_cards);
        }
    }
    public override void Death(){
        Console.WriteLine("You died");
        if (phenix_stone){
                hp = 1;
            phenix_stone = false;
            Console.WriteLine("Phenix stone saved you");
            Console.WriteLine("Your hp = 1");
        } else{
            Deck.GetInstance().ReturnCards(RemoveCards([]));
        }
    }
    public void ShowInventory(){
        ShowCards();
        if (phenix_stone)
            Console.WriteLine("You have phenix stone");
        if (health_potions > 0){
            if (health_potions == 1)
                Console.WriteLine("You have 1 health potion");
            else
                Console.WriteLine($"You have {health_potions} health potions");
        }
    }
    public void UseHeal(){
        if (health_potions > 0){
            health_potions--;
            int heal = 20;
            if (hp+heal > max_hp)
                hp = max_hp;
            else
                hp += heal;
            Console.WriteLine($"{name} use health potion and heal {heal} hp");
        } else {
            Console.WriteLine("You have no health potions");
        }
    }
    public void GetArmor(int armor){
        current_armor += armor;
        max_armor += armor;
    }
    
}

public class Enemy : Actor {
    public Enemy(string name, int hp, int armor) : base(name, hp, armor){}
}
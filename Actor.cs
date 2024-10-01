namespace CardDungeon;
public class Actor(string name)
{
    public int hp = 50;
    public int armor = 100;
    public string name = name;
    public bool phenix_stone = false;
    public List<Card> cards = [];
    public void PrintHP(){
        if (armor<=0)
            Console.WriteLine(name + " hp: " + hp);
        else
            Console.WriteLine(name + " hp: " + hp + ", armor: " + armor);
    }
    public void AddCards(int n){
        this.cards.AddRange(Deck.GetInstance().GetCards(n));
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
        } return result;
    }
    public void ShowCards(){
        for (int i = 0; i < cards.Count; i++){
            Console.WriteLine(i+1 + ". " + cards[i].Print());
        }
    }
    public void Death(){
        Deck.GetInstance().ReturnCards(RemoveCards([]));
        Console.WriteLine(name + " died");
    }
    public bool GetDamage(List<Card> cards, int multiplier){
        int damage = 0;
        foreach (Card card in cards){
            damage = damage + ((int)card.rank + 2)*multiplier;
        }
        if (multiplier==1)
            Console.WriteLine(name + " get " + damage + " damage");
        else
            Console.WriteLine(name + " get " + damage + " damage (x" + multiplier + ")");
        if (armor > 0){
            armor -= damage;
            if (armor < 0){
                hp += armor;
                armor = 0;
            }
        } else {
            hp -= damage;
        }
        if (hp <= 0){
            if (phenix_stone){
                hp = 1;
                phenix_stone = false;
                Console.WriteLine("Phenix stone saved you");
                Console.WriteLine("Your hp = 1");
                return true;
            }
            Death();
            return false;
        } return true;
    }
}

public class CardInstance {
    public CardDefinition definition;
    public int currentCost;
    public CardInstance(CardDefinition def) {
        definition = def;
        currentCost = def.cost;
    }
}

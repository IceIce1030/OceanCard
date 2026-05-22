namespace OceanCard.Models;

public class Deck
{
    public int Id { get; set; }
    public string Name { get; set; } = "";

    // 這個牌組收錄的卡牌(查詢時由 Repository 組裝進來)
    public List<Card> Cards { get; set; } = new();
}
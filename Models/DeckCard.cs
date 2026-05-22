namespace OceanCard.Models;

// 牌組與卡牌的橋樑:一列代表「某牌組收錄某卡」
public class DeckCard
{
    public int Id { get; set; }
    public int DeckId { get; set; }
    public int CardId { get; set; }
}
namespace OceanCard.Models;

public enum OceanElement
{
    Shallow,   // 淺灘
    Reef,      // 珊瑚礁
    Current,   // 洋流
    DeepSea,   // 深海
    Abyss      // 深淵
}

public enum Rarity
{
    Common,    // 普通
    Rare,      // 稀有
    Epic,      // 史詩
    Legendary  // 傳說
}

public class Card
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public OceanElement Element { get; set; }
    public Rarity Rarity { get; set; }
    public int Cost { get; set; }
    public int Attack { get; set; }
    public int Health { get; set; }
    public string ImagePath { get; set; } = "";

    // 查詢時由 Repository 組裝進來
    public List<Skill> Skills { get; set; } = new();
}
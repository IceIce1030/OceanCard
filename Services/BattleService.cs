using OceanCard.Models;

namespace OceanCard.Services;

// 單次攻擊的計算結果
public class AttackResult
{
    public Card Attacker { get; set; } = null!;
    public Card Defender { get; set; } = null!;
    public double Multiplier { get; set; }    // 屬性倍率
    public int Damage { get; set; }           // 實際傷害
    public int DefenderHealthAfter { get; set; } // 防守方剩餘生命
    public bool IsLethal { get; set; }        // 是否一擊斃命
    public string Relation { get; set; } = "";  // 屬性關係文字
}

public class BattleService
{
    // 五角循環相克表:key 克 value
    private static readonly Dictionary<OceanElement, OceanElement> Counters = new()
    {
        { OceanElement.Shallow, OceanElement.Reef },     // 淺灘 克 珊瑚礁
        { OceanElement.Reef,    OceanElement.Current },  // 珊瑚礁 克 洋流
        { OceanElement.Current, OceanElement.DeepSea },  // 洋流 克 深海
        { OceanElement.DeepSea, OceanElement.Abyss },    // 深海 克 深淵
        { OceanElement.Abyss,   OceanElement.Shallow },  // 深淵 克 淺灘
    };

    // 倍率設定
    private const double Advantage = 1.5;   // 克制
    private const double Disadvantage = 0.75; // 被克
    private const double Neutral = 1.0;     // 無關

    // 計算 attacker 攻擊 defender 的結果
    public AttackResult CalculateAttack(Card attacker, Card defender)
    {
        double multiplier;
        string relation;

        if (Counters[attacker.Element] == defender.Element)
        {
            multiplier = Advantage;
            relation = "屬性克制 ▲";
        }
        else if (Counters[defender.Element] == attacker.Element)
        {
            multiplier = Disadvantage;
            relation = "屬性被克 ▼";
        }
        else
        {
            multiplier = Neutral;
            relation = "屬性無關 ―";
        }

        // 傷害 = 攻擊力 × 屬性倍率(無條件捨去取整數)
        int damage = (int)Math.Floor(attacker.Attack * multiplier);
        int healthAfter = defender.Health - damage;

        return new AttackResult
        {
            Attacker = attacker,
            Defender = defender,
            Multiplier = multiplier,
            Damage = damage,
            DefenderHealthAfter = healthAfter,
            IsLethal = healthAfter <= 0,
            Relation = relation
        };
    }
}
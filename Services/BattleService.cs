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

// 多回合對戰裡,單一回合的紀錄
public class RoundLog
{
    public int Round { get; set; }
    public string AttackerName { get; set; } = "";
    public string DefenderName { get; set; } = "";
    public int Damage { get; set; }
    public string Relation { get; set; } = "";
    public int DefenderHealthAfter { get; set; }
}

// 整場多回合對戰的結果
public class DuelResult
{
    public List<RoundLog> Rounds { get; set; } = new();
    public string WinnerName { get; set; } = "";   // 空字串 = 平手
    public bool ReachedLimit { get; set; }          // 是否打到回合上限
    public int CardAHealthFinal { get; set; }
    public int CardBHealthFinal { get; set; }
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

    private const int MaxRounds = 20;   // 回合上限

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

    // 多回合對戰:兩張卡輪流攻擊,打到一方血量歸零或達回合上限
    public DuelResult Duel(Card cardA, Card cardB)
    {
        var result = new DuelResult();

        // 各自的當前生命(對戰過程中會扣)
        int hpA = cardA.Health;
        int hpB = cardB.Health;

        // 先攻判定:攻擊力高者先,相同則 A 先
        bool aTurn = cardA.Attack >= cardB.Attack;

        for (int round = 1; round <= MaxRounds; round++)
        {
            // 決定這回合的攻方與守方
            var attacker = aTurn ? cardA : cardB;
            var defender = aTurn ? cardB : cardA;

            // 用現成的單次攻擊計算(含屬性倍率)
            var atk = CalculateAttack(attacker, defender);

            // 扣防守方的血
            if (aTurn) hpB -= atk.Damage;
            else       hpA -= atk.Damage;

            result.Rounds.Add(new RoundLog
            {
                Round = round,
                AttackerName = attacker.Name,
                DefenderName = defender.Name,
                Damage = atk.Damage,
                Relation = atk.Relation,
                DefenderHealthAfter = aTurn ? hpB : hpA
            });

            // 檢查是否分出勝負
            if (hpB <= 0)
            {
                result.WinnerName = cardA.Name;
                break;
            }
            if (hpA <= 0)
            {
                result.WinnerName = cardB.Name;
                break;
            }

            aTurn = !aTurn;   // 換邊
        }

        result.CardAHealthFinal = hpA;
        result.CardBHealthFinal = hpB;

        // 若迴圈跑完都沒人倒(打到上限)
        if (string.IsNullOrEmpty(result.WinnerName))
        {
            result.ReachedLimit = true;
            if (hpA > hpB) result.WinnerName = cardA.Name;
            else if (hpB > hpA) result.WinnerName = cardB.Name;
            // hpA == hpB → WinnerName 維持空字串 = 平手
        }

        return result;
    }
}
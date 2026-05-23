using OceanCard.Models;

namespace OceanCard.Services;

// 一場小對戰(單卡 vs 單卡)的結果
public class MatchResult
{
    public int Round { get; set; }            // 第幾場
    public Card? CardA { get; set; }           // A 隊出戰卡(可能為 null = 不戰而勝)
    public Card? CardB { get; set; }
    public string Outcome { get; set; } = "";  // 結果描述文字
    public int Winner { get; set; }            // 1=A贏, 2=B贏, 0=平手
}

// 整場牌組對戰的結果
public class DeckBattleResult
{
    public List<MatchResult> Matches { get; set; } = new();
    public int AWins { get; set; }
    public int BWins { get; set; }
    public int Draws { get; set; }
    public int FinalWinner { get; set; }   // 1=A隊, 2=B隊, 0=平手
}

public class DeckBattleService
{
    private readonly BattleService _battle;

    public DeckBattleService(BattleService battle)
    {
        _battle = battle;
    }

    // 計算兩個牌組的完整對戰
    public DeckBattleResult Fight(Deck deckA, Deck deckB)
    {
        var result = new DeckBattleResult();

        var cardsA = deckA.Cards;
        var cardsB = deckB.Cards;

        // 以較短的牌組為準,只打這麼多場
        int rounds = Math.Min(cardsA.Count, cardsB.Count);

        for (int i = 0; i < rounds; i++)
        {
            var ca = cardsA[i];
            var cb = cardsB[i];

            var match = JudgeOneMatch(i + 1, ca, cb);
            result.Matches.Add(match);

            if (match.Winner == 1) result.AWins++;
            else if (match.Winner == 2) result.BWins++;
            else result.Draws++;
        }

        // 整場勝負:贏比較多場的隊伍獲勝
        if (result.AWins > result.BWins) result.FinalWinner = 1;
        else if (result.BWins > result.AWins) result.FinalWinner = 2;
        else result.FinalWinner = 0;

        return result;
    }

    // 判定一場小對戰:雙方互打一次,定勝負
    private MatchResult JudgeOneMatch(int round, Card ca, Card cb)
    {
        // A 打 B、B 打 A 各一次
        var aHitsB = _battle.CalculateAttack(ca, cb);
        var bHitsA = _battle.CalculateAttack(cb, ca);

        var match = new MatchResult
        {
            Round = round,
            CardA = ca,
            CardB = cb
        };

        bool aDead = bHitsA.IsLethal;   // A 被打到血量歸零
        bool bDead = aHitsB.IsLethal;   // B 被打到血量歸零

        if (bDead && !aDead)
        {
            match.Winner = 1;
            match.Outcome = $"{ca.Name} 擊倒 {cb.Name}";
        }
        else if (aDead && !bDead)
        {
            match.Winner = 2;
            match.Outcome = $"{cb.Name} 擊倒 {ca.Name}";
        }
        else if (aDead && bDead)
        {
            // 同歸於盡 → 比誰造成的傷害高
            if (aHitsB.Damage > bHitsA.Damage)
            {
                match.Winner = 1;
                match.Outcome = $"同歸於盡,{ca.Name} 傷害較高險勝";
            }
            else if (bHitsA.Damage > aHitsB.Damage)
            {
                match.Winner = 2;
                match.Outcome = $"同歸於盡,{cb.Name} 傷害較高險勝";
            }
            else
            {
                match.Winner = 0;
                match.Outcome = "兩敗俱傷,平手";
            }
        }
        else
        {
            // 都沒倒 → 比剩餘血量
            if (aHitsB.DefenderHealthAfter < bHitsA.DefenderHealthAfter)
            {
                match.Winner = 1;
                match.Outcome = $"{ca.Name} 把對手打得更慘,勝";
            }
            else if (bHitsA.DefenderHealthAfter < aHitsB.DefenderHealthAfter)
            {
                match.Winner = 2;
                match.Outcome = $"{cb.Name} 把對手打得更慘,勝";
            }
            else
            {
                match.Winner = 0;
                match.Outcome = "勢均力敵,平手";
            }
        }

        return match;
    }
}
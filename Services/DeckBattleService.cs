using OceanCard.Models;

namespace OceanCard.Services;

// 一場小對戰(單卡 vs 單卡,多回合)的結果
public class MatchResult
{
    public int Round { get; set; }            // 第幾場
    public Card? CardA { get; set; }
    public Card? CardB { get; set; }
    public string Outcome { get; set; } = "";  // 結果描述
    public int Winner { get; set; }            // 1=A贏, 2=B贏, 0=平手
    public int RoundsPlayed { get; set; }      // 這場打了幾回合
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

        if (result.AWins > result.BWins) result.FinalWinner = 1;
        else if (result.BWins > result.AWins) result.FinalWinner = 2;
        else result.FinalWinner = 0;

        return result;
    }

    // 判定一場小對戰:用多回合 Duel 打到分勝負
    private MatchResult JudgeOneMatch(int round, Card ca, Card cb)
    {
        var duel = _battle.Duel(ca, cb);

        var match = new MatchResult
        {
            Round = round,
            CardA = ca,
            CardB = cb,
            RoundsPlayed = duel.Rounds.Count
        };

        if (duel.WinnerName == ca.Name)
        {
            match.Winner = 1;
            match.Outcome = $"{ca.Name} 於第 {duel.Rounds.Count} 回合擊敗 {cb.Name}";
        }
        else if (duel.WinnerName == cb.Name)
        {
            match.Winner = 2;
            match.Outcome = $"{cb.Name} 於第 {duel.Rounds.Count} 回合擊敗 {ca.Name}";
        }
        else
        {
            match.Winner = 0;
            match.Outcome = $"打滿 {duel.Rounds.Count} 回合平手";
        }

        return match;
    }
}
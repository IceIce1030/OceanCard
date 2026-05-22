using Dapper;
using Microsoft.Data.Sqlite;
using OceanCard.Models;

namespace OceanCard.Repositories;

public class DeckRepository
{
    private readonly string _connStr;

    public DeckRepository(IConfiguration config)
    {
        _connStr = config.GetConnectionString("Default")!;
    }

    private SqliteConnection Connection() => new(_connStr);

    public async Task<List<Deck>> GetAllAsync()
    {
        using var conn = Connection();

        var decks = (await conn.QueryAsync<Deck>(
            "SELECT * FROM Decks ORDER BY Id")).ToList();

        var links = await conn.QueryAsync<DeckCard>("SELECT * FROM DeckCards");

        foreach (var deck in decks)
        {
            var count = links.Count(l => l.DeckId == deck.Id);
            deck.Cards = Enumerable.Range(0, count).Select(_ => new Card()).ToList();
        }

        return decks;
    }

    public async Task<Deck?> GetByIdAsync(int id)
    {
        using var conn = Connection();

        var deck = await conn.QuerySingleOrDefaultAsync<Deck>(
            "SELECT * FROM Decks WHERE Id = @id", new { id });

        if (deck is null)
            return null;

        var cards = await conn.QueryAsync<Card>(@"
            SELECT c.* FROM Cards c
            JOIN DeckCards dc ON dc.CardId = c.Id
            WHERE dc.DeckId = @id
            ORDER BY c.Cost, c.Id;", new { id });

        deck.Cards = cards.ToList();
        return deck;
    }

    public async Task<int> CreateAsync(string name)
    {
        using var conn = Connection();
        return await conn.ExecuteScalarAsync<int>(@"
            INSERT INTO Decks (Name) VALUES (@name);
            SELECT last_insert_rowid();", new { name });
    }

    public async Task<bool> RenameAsync(int id, string name)
    {
        using var conn = Connection();
        var rows = await conn.ExecuteAsync(
            "UPDATE Decks SET Name = @name WHERE Id = @id", new { id, name });
        return rows > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var conn = Connection();
        var rows = await conn.ExecuteAsync(
            "DELETE FROM Decks WHERE Id = @id", new { id });
        return rows > 0;
    }

    public async Task AddCardAsync(int deckId, int cardId)
    {
        using var conn = Connection();

        var exists = await conn.ExecuteScalarAsync<int>(@"
            SELECT COUNT(*) FROM DeckCards
            WHERE DeckId = @deckId AND CardId = @cardId;",
            new { deckId, cardId });

        if (exists > 0)
            return;

        await conn.ExecuteAsync(@"
            INSERT INTO DeckCards (DeckId, CardId)
            VALUES (@deckId, @cardId);", new { deckId, cardId });
    }

    public async Task RemoveCardAsync(int deckId, int cardId)
    {
        using var conn = Connection();
        await conn.ExecuteAsync(@"
            DELETE FROM DeckCards
            WHERE DeckId = @deckId AND CardId = @cardId;",
            new { deckId, cardId });
    }
    // 隨機生成一個牌組:從卡庫隨機挑 count 張不重複的卡
    public async Task<int> CreateRandomDeckAsync(string name, int count)
    {
        using var conn = Connection();
        conn.Open();
        using var tx = conn.BeginTransaction();

        // 建牌組
        var deckId = await conn.ExecuteScalarAsync<int>(@"
            INSERT INTO Decks (Name) VALUES (@name);
            SELECT last_insert_rowid();", new { name }, tx);

        // 撈出全部卡牌 Id,洗牌後取前 count 張
        var allIds = (await conn.QueryAsync<int>(
            "SELECT Id FROM Cards", transaction: tx)).ToList();

        var rng = new Random();
        var picked = allIds.OrderBy(_ => rng.Next()).Take(count).ToList();

        // 塞進 DeckCards
        foreach (var cardId in picked)
        {
            await conn.ExecuteAsync(@"
                INSERT INTO DeckCards (DeckId, CardId)
                VALUES (@deckId, @cardId);",
                new { deckId, cardId }, tx);
        }

        tx.Commit();
        return deckId;
    }
}
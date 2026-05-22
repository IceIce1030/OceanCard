using Dapper;
using Microsoft.Data.Sqlite;
using OceanCard.Models;

namespace OceanCard.Repositories;

public class CardRepository
{
    private readonly string _connStr;

    public CardRepository(IConfiguration config)
    {
        _connStr = config.GetConnectionString("Default")!;
    }

    private SqliteConnection Connection() => new(_connStr);

    public async Task<List<Card>> GetAllAsync()
    {
        using var conn = Connection();
        var cards = (await conn.QueryAsync<Card>(
            "SELECT * FROM Cards ORDER BY Cost, Id")).ToList();
        var skills = await conn.QueryAsync<Skill>("SELECT * FROM Skills");
        foreach (var card in cards)
            card.Skills = skills.Where(s => s.CardId == card.Id).ToList();
        return cards;
    }

    public async Task<Card?> GetByIdAsync(int id)
    {
        using var conn = Connection();
        var card = await conn.QuerySingleOrDefaultAsync<Card>(
            "SELECT * FROM Cards WHERE Id = @id", new { id });
        if (card is null) return null;
        card.Skills = (await conn.QueryAsync<Skill>(
            "SELECT * FROM Skills WHERE CardId = @id", new { id })).ToList();
        return card;
    }

    public async Task<List<Card>> SearchAsync(
        string? keyword, OceanElement? element, Rarity? rarity, string? sort)
    {
        using var conn = Connection();
        var where = new List<string>();
        var param = new DynamicParameters();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            where.Add("Name LIKE @keyword");
            param.Add("keyword", $"%{keyword.Trim()}%");
        }
        if (element is not null)
        {
            where.Add("Element = @element");
            param.Add("element", (int)element.Value);
        }
        if (rarity is not null)
        {
            where.Add("Rarity = @rarity");
            param.Add("rarity", (int)rarity.Value);
        }

        var whereSql = where.Count > 0 ? "WHERE " + string.Join(" AND ", where) : "";
        var orderSql = sort switch
        {
            "cost"   => "ORDER BY Cost, Id",
            "attack" => "ORDER BY Attack DESC, Id",
            "health" => "ORDER BY Health DESC, Id",
            "name"   => "ORDER BY Name, Id",
            _        => "ORDER BY Id"
        };

        var sql = $"SELECT * FROM Cards {whereSql} {orderSql}";
        var cards = (await conn.QueryAsync<Card>(sql, param)).ToList();
        var skills = await conn.QueryAsync<Skill>("SELECT * FROM Skills");
        foreach (var card in cards)
            card.Skills = skills.Where(s => s.CardId == card.Id).ToList();
        return cards;
    }

    public async Task<int> CreateAsync(Card card)
    {
        using var conn = Connection();
        conn.Open();
        using var tx = conn.BeginTransaction();
        var newId = await conn.ExecuteScalarAsync<int>(@"
            INSERT INTO Cards (Name, Description, Element, Rarity, Cost, Attack, Health, ImagePath)
            VALUES (@Name, @Description, @Element, @Rarity, @Cost, @Attack, @Health, @ImagePath);
            SELECT last_insert_rowid();", card, tx);
        foreach (var s in card.Skills)
        {
            await conn.ExecuteAsync(@"
                INSERT INTO Skills (Name, Effect, CardId)
                VALUES (@Name, @Effect, @CardId);",
                new { s.Name, s.Effect, CardId = newId }, tx);
        }
        tx.Commit();
        return newId;
    }

    public async Task<bool> UpdateAsync(Card card)
    {
        using var conn = Connection();
        conn.Open();
        using var tx = conn.BeginTransaction();
        var rows = await conn.ExecuteAsync(@"
            UPDATE Cards SET
                Name = @Name, Description = @Description,
                Element = @Element, Rarity = @Rarity,
                Cost = @Cost, Attack = @Attack, Health = @Health,
                ImagePath = @ImagePath
            WHERE Id = @Id;", card, tx);
        if (rows == 0)
        {
            tx.Rollback();
            return false;
        }
        await conn.ExecuteAsync(
            "DELETE FROM Skills WHERE CardId = @id", new { id = card.Id }, tx);
        foreach (var s in card.Skills)
        {
            await conn.ExecuteAsync(@"
                INSERT INTO Skills (Name, Effect, CardId)
                VALUES (@Name, @Effect, @CardId);",
                new { s.Name, s.Effect, CardId = card.Id }, tx);
        }
        tx.Commit();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var conn = Connection();
        var rows = await conn.ExecuteAsync(
            "DELETE FROM Cards WHERE Id = @id", new { id });
        return rows > 0;
    }
}
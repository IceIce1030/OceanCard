using Dapper;
using Microsoft.Data.Sqlite;

namespace OceanCard.Data;

public static class DatabaseInitializer
{
    public static void Initialize(string connectionString)
    {
        using var conn = new SqliteConnection(connectionString);
        conn.Open();

        // 建立資料表(已存在就略過)
        conn.Execute(@"
            CREATE TABLE IF NOT EXISTS Cards (
                Id          INTEGER PRIMARY KEY AUTOINCREMENT,
                Name        TEXT    NOT NULL,
                Description TEXT    NOT NULL DEFAULT '',
                Element     INTEGER NOT NULL,
                Rarity      INTEGER NOT NULL,
                Cost        INTEGER NOT NULL,
                Attack      INTEGER NOT NULL,
                Health      INTEGER NOT NULL,
                ImagePath   TEXT    NOT NULL DEFAULT ''
            );

            CREATE TABLE IF NOT EXISTS Skills (
                Id      INTEGER PRIMARY KEY AUTOINCREMENT,
                Name    TEXT    NOT NULL,
                Effect  TEXT    NOT NULL DEFAULT '',
                CardId  INTEGER NOT NULL,
                FOREIGN KEY (CardId) REFERENCES Cards(Id) ON DELETE CASCADE
            );

            CREATE TABLE IF NOT EXISTS Decks (
                Id   INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL
            );

            CREATE TABLE IF NOT EXISTS DeckCards (
                Id     INTEGER PRIMARY KEY AUTOINCREMENT,
                DeckId INTEGER NOT NULL,
                CardId INTEGER NOT NULL,
                FOREIGN KEY (DeckId) REFERENCES Decks(Id) ON DELETE CASCADE,
                FOREIGN KEY (CardId) REFERENCES Cards(Id) ON DELETE CASCADE
            );
        ");

        // 種子資料:只在完全沒有卡牌時才塞
        var cardCount = conn.ExecuteScalar<int>("SELECT COUNT(*) FROM Cards");
        if (cardCount == 0)
            SeedData(conn);
    }

    private static void SeedData(SqliteConnection conn)
    {
        // 第一張:深海巨鯊
        var sharkId = conn.ExecuteScalar<int>(@"
            INSERT INTO Cards (Name, Description, Element, Rarity, Cost, Attack, Health, ImagePath)
            VALUES (@Name, @Description, @Element, @Rarity, @Cost, @Attack, @Health, @ImagePath);
            SELECT last_insert_rowid();",
            new
            {
                Name = "深海巨鯊",
                Description = "海裡最不需要預約的牙醫。牠很忙,你別煩牠。",
                Element = 3,   // DeepSea
                Rarity = 3,    // Legendary
                Cost = 7, Attack = 8, Health = 6,
                ImagePath = ""
            });

        conn.Execute(@"
            INSERT INTO Skills (Name, Effect, CardId)
            VALUES (@Name, @Effect, @CardId);",
            new[]
            {
                new { Name = "獵殺", Effect = "攻擊淺灘屬性單位時造成雙倍傷害", CardId = sharkId },
                new { Name = "深淵恐懼", Effect = "登場時使敵方一隻單位攻擊力 -1", CardId = sharkId }
            });

        // 第二張:珊瑚守衛
        var guardId = conn.ExecuteScalar<int>(@"
            INSERT INTO Cards (Name, Description, Element, Rarity, Cost, Attack, Health, ImagePath)
            VALUES (@Name, @Description, @Element, @Rarity, @Cost, @Attack, @Health, @ImagePath);
            SELECT last_insert_rowid();",
            new
            {
                Name = "珊瑚守衛",
                Description = "站了三千年沒換班,工會已介入調查。",
                Element = 1,   // Reef
                Rarity = 2,    // Epic
                Cost = 4, Attack = 3, Health = 7,
                ImagePath = ""
            });

        conn.Execute(@"
            INSERT INTO Skills (Name, Effect, CardId)
            VALUES (@Name, @Effect, @CardId);",
            new { Name = "守護", Effect = "為相鄰單位提供 1 點護甲", CardId = guardId });

        // 第三張:洋流游俠
        var rangerId = conn.ExecuteScalar<int>(@"
            INSERT INTO Cards (Name, Description, Element, Rarity, Cost, Attack, Health, ImagePath)
            VALUES (@Name, @Description, @Element, @Rarity, @Cost, @Attack, @Health, @ImagePath);
            SELECT last_insert_rowid();",
            new
            {
                Name = "洋流游俠",
                Description = "跑很快,但常常忘記自己要去哪。",
                Element = 2,   // Current
                Rarity = 1,    // Rare
                Cost = 3, Attack = 4, Health = 2,
                ImagePath = ""
            });

        conn.Execute(@"
            INSERT INTO Skills (Name, Effect, CardId)
            VALUES (@Name, @Effect, @CardId);",
            new { Name = "疾襲", Effect = "登場當回合即可攻擊", CardId = rangerId });
    }
}
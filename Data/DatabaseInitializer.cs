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
        // 卡牌種子資料:名稱, 描述, 屬性(0淺灘1珊瑚礁2洋流3深海4深淵), 稀有度(0普通1稀有2史詩3傳說), 費, 攻, 血
        var cards = new (string Name, string Desc, int El, int Rar, int Cost, int Atk, int Hp, string Icon)[]
        {
            // --- 淺灘 Shallow ---
            ("淺灘小丑魚", "靠裝可愛維生,意外地有效。", 0, 0, 1, 1, 2, "🤡"),
            ("陽光水母", "漂亮,但碰了會後悔。", 0, 0, 2, 2, 1, "🎐"),
            ("沙灘寄居蟹", "換殼換到有選擇障礙。", 0, 0, 2, 1, 4, "🐚"),
            ("淺水海龜", "活了一百年,還是游不快。", 0, 1, 3, 2, 6, "🐢"),
            ("珍珠貝衛兵", "話很少,因為嘴閉著。", 0, 1, 4, 3, 5, "🦪"),
            ("潮間帶霸主", "退潮時牠最大,漲潮時牠裝死。", 0, 2, 5, 5, 4, "🏖️"),
            ("黎明海神", "掌管日出,經常睡過頭。", 0, 3, 7, 7, 7, "🌅"),

            // --- 珊瑚礁 Reef ---
            ("珊瑚幼蟲", "現在很小,夢想很大。", 1, 0, 1, 1, 1, "🪸"),
            ("清潔蝦", "幫大魚做 SPA,小費收得不錯。", 1, 0, 2, 1, 3, "🦐"),
            ("石斑魚", "躲在洞裡,假裝自己是石頭。", 1, 0, 3, 3, 3, "🐟"),
            ("海葵刺客", "看起來是花,其實是陷阱。", 1, 1, 3, 4, 2, "🌸"),
            ("鸚鵡螺長老", "腦袋構造很古老,記性卻很好。", 1, 1, 4, 2, 7, "🐚"),
            ("珊瑚守衛", "站了三千年沒換班,工會已介入調查。", 1, 2, 4, 3, 7, "🛡️"),
            ("礁岩龍王", "珊瑚礁的房東,租金很貴。", 1, 3, 7, 6, 8, "🐉"),

            // --- 洋流 Current ---
            ("迷路沙丁魚", "跟著魚群游,從來不知道要去哪。", 2, 0, 1, 2, 1, "🐠"),
            ("飛魚信使", "送信很快,但常常飛過頭。", 2, 0, 2, 2, 2, "🦋"),
            ("洋流游俠", "跑很快,但常常忘記自己要去哪。", 2, 1, 3, 4, 2, "🏊"),
            ("旗魚衝鋒兵", "時速很快,煞車是弱點。", 2, 1, 4, 5, 3, "🗡️"),
            ("迴游鮪魚", "一輩子都在游,沒空停下來。", 2, 2, 5, 5, 5, "🐟"),
            ("急流操縱者", "能控制洋流,但控制不了脾氣。", 2, 2, 5, 4, 6, "🌀"),
            ("潮汐之王", "說漲潮就漲潮,非常任性。", 2, 3, 7, 7, 6, "👑"),

            // --- 深海 DeepSea ---
            ("深海燈籠魚", "自帶照明,電費自付。", 3, 0, 2, 2, 2, "🏮"),
            ("盲眼洞穴魚", "看不見,但聽力一流。", 3, 0, 2, 1, 4, "🐡"),
            ("巨口鯊", "嘴巴比腦袋大,個性也是。", 3, 1, 4, 5, 3, "🦈"),
            ("深海管蟲", "靠熱泉維生,從不曬太陽。", 3, 1, 3, 2, 6, "🪱"),
            ("吞噬鰻", "肚子是無底洞,字面意義上。", 3, 2, 5, 6, 4, "🐍"),
            ("幽光巨章魚", "八隻手,九個心眼。", 3, 2, 6, 5, 6, "🐙"),
            ("深海巨鯊", "海裡最不需要預約的牙醫。牠很忙,你別煩牠。", 3, 3, 7, 8, 6, "🦷"),
            ("無光帝王", "從沒見過光,也不打算見。", 3, 3, 8, 8, 8, "🌑"),

            // --- 深淵 Abyss ---
            ("深淵浮游生物", "渺小,但數量是它的武器。", 4, 0, 1, 1, 1, "🦠"),
            ("骸骨海馬", "優雅地飄著,有點陰森。", 4, 0, 2, 2, 2, "🐴"),
            ("幽魂水母", "半透明,連自己都快看不到。", 4, 1, 3, 3, 3, "👻"),
            ("深淵獵手", "在黑暗裡狩獵,連自己都嚇到。", 4, 1, 4, 4, 4, "🏹"),
            ("古老海妖", "歌聲動人,內容是抱怨。", 4, 2, 5, 5, 5, "🧜"),
            ("虛空巨獸", "大到沒有天敵,除了無聊。", 4, 2, 6, 6, 5, "🐋"),
            ("深淵領主", "深淵的管理者,從不開會。", 4, 3, 8, 9, 7, "💀"),
        };
        // 批次插入卡牌,並順手記下每張的 Id(給技能用)
        var cardIds = new List<int>();
        foreach (var c in cards)
        {
            var id = conn.ExecuteScalar<int>(@"
                INSERT INTO Cards (Name, Description, Element, Rarity, Cost, Attack, Health, ImagePath)
                VALUES (@Name, @Desc, @El, @Rar, @Cost, @Atk, @Hp, @Icon);
                SELECT last_insert_rowid();",
                new { c.Name, c.Desc, c.El, c.Rar, c.Cost, c.Atk, c.Hp, c.Icon });
            cardIds.Add(id);
        }

        // 給少數幾張卡加技能當示範(其餘卡先沒有技能)
        conn.Execute(@"INSERT INTO Skills (Name, Effect, CardId) VALUES (@Name, @Effect, @CardId);",
            new[]
            {
                new { Name = "獵殺", Effect = "攻擊淺灘屬性單位時造成雙倍傷害", CardId = cardIds[27] },
                new { Name = "深淵恐懼", Effect = "登場時使敵方一隻單位攻擊力 -1", CardId = cardIds[27] },
                new { Name = "守護", Effect = "為相鄰單位提供 1 點護甲", CardId = cardIds[12] },
                new { Name = "疾襲", Effect = "登場當回合即可攻擊", CardId = cardIds[16] },
            });
    }
}
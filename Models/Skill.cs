namespace OceanCard.Models;

public class Skill
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Effect { get; set; } = "";
    public int CardId { get; set; }   // 外鍵,指向 Card.Id
}
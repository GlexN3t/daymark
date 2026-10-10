namespace Daymark.Api.Domain;

public class Habit
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }

    public string Name { get; set; } = string.Empty;
    public string? Note { get; set; }
    public string Color { get; set; } = "#1f6f5c";
    public DateOnly CreatedOn { get; set; }
    public bool Archived { get; set; }

    public List<CheckIn> CheckIns { get; set; } = [];
}

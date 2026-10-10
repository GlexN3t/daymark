namespace Daymark.Api.Domain;

public class CheckIn
{
    public int Id { get; set; }
    public int HabitId { get; set; }
    public Habit? Habit { get; set; }

    public DateOnly Date { get; set; }
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; }
}

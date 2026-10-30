namespace ChumakovD_LR5.Models;

public class TodoItem
{
    public long Id { get; set; }
    public string? Name { get; set; }
    public bool IsComplete { get; set; }
    // Завдання 2. Дата виконання та пріоритет зберігаються в базі.
    public DateOnly DueDate { get; set; }
    public int Priority { get; set; }
    public string? Secret { get; set; }
}

using System.ComponentModel.DataAnnotations;

namespace ChumakovD_LR5.Configuration;

// Завдання 3. Ім'я бази даних у конфігурації з DataAnnotations.
public sealed class DatabaseOptions
{
    public const string SectionName = "Database";

    [Required(ErrorMessage = "Database:Name не може бути null, порожнім або складатися з пробілів.")]
    [MinLength(4, ErrorMessage = "Database:Name має містити щонайменше 4 символи.")]
    public string Name { get; set; } = null!;
}

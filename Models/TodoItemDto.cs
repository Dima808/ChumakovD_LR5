using System.ComponentModel.DataAnnotations;

namespace ChumakovD_LR5.Models;

// Завдання 1 і 2. DTO захищає Secret і передає нові поля у всіх операціях.
public sealed class TodoItemDto : IValidatableObject
{
    public long Id { get; set; }
    [Required(ErrorMessage = "Назва завдання обов'язкова.")]
    [StringLength(120, ErrorMessage = "Назва має містити не більше 120 символів.")]
    public string Name { get; set; } = string.Empty;
    public bool IsComplete { get; set; }
    public DateOnly DueDate { get; set; }
    [Range(1, 5, ErrorMessage = "Пріоритет має бути від 1 до 5.")]
    public int Priority { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (DueDate == default)
            yield return new ValidationResult("Укажіть дату виконання.", [nameof(DueDate)]);
    }
}

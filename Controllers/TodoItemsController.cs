using ChumakovD_LR5.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChumakovD_LR5.Controllers;

// Завдання 1. Контролер згенеровано scaffolder та розширено DTO й новими діями.
[Route("api/[controller]")]
[ApiController]
public class TodoItemsController(TodoContext context) : ControllerBase
{
    // Завдання 1 і 2. Усі записи з DueDate і Priority, без Secret.
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TodoItemDto>>> GetTodoItems()
    {
        return await context.TodoItems.OrderBy(x => x.Id).Select(x => ToDto(x)).ToListAsync();
    }

    // Завдання 4. Пошук за двома або більше різними Id: ?ids=1&ids=3.
    [HttpGet("by-ids")]
    [ActionName("GetTodoItemsByIds")]
    public async Task<ActionResult<IEnumerable<TodoItemDto>>> GetTodoItems([FromQuery] long[] ids)
    {
        var requested = ids.Distinct().ToArray();
        if (requested.Length < 2 || requested.Any(id => id <= 0))
            return BadRequest(new { error = "Передайте щонайменше два різні додатні Id." });

        return await context.TodoItems.Where(x => requested.Contains(x.Id))
            .OrderBy(x => x.Id).Select(x => ToDto(x)).ToListAsync();
    }

    // Завдання 1 і 2. Отримання одного запису, 404 для відсутнього Id.
    [HttpGet("{id:long}")]
    public async Task<ActionResult<TodoItemDto>> GetTodoItem(long id)
    {
        var item = await context.TodoItems.FindAsync(id);
        return item is null ? NotFound() : ToDto(item);
    }

    // Завдання 1 і 2. Оновлення всіх відкритих полів без зміни Secret.
    [HttpPut("{id:long}")]
    public async Task<IActionResult> PutTodoItem(long id, TodoItemDto dto)
    {
        if (id != dto.Id) return BadRequest(new { error = "Id у маршруті та JSON мають збігатися." });
        var item = await context.TodoItems.FindAsync(id);
        if (item is null) return NotFound();
        item.Name = dto.Name;
        item.IsComplete = dto.IsComplete;
        item.DueDate = dto.DueDate;
        item.Priority = dto.Priority;
        await context.SaveChangesAsync();
        return NoContent();
    }

    // Завдання 1 і 2. Створення запису з серверним Id і відповіддю 201.
    [HttpPost]
    public async Task<ActionResult<TodoItemDto>> PostTodoItem(TodoItemDto dto)
    {
        if (dto.Id != 0) return BadRequest(new { error = "Не задавайте Id під час створення." });
        var item = FromDto(dto);
        context.TodoItems.Add(item);
        await context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetTodoItem), new { id = item.Id }, ToDto(item));
    }

    // Завдання 5. Один SaveChangesAsync для масиву; відповідь містить усі створені Id.
    [HttpPost("batch")]
    public async Task<ActionResult<IEnumerable<TodoItemDto>>> PostTodoItems(TodoItemDto[] items)
    {
        if (items.Length == 0 || items.Length > 200)
            return BadRequest(new { error = "Масив має містити від 1 до 200 елементів." });
        if (items.Any(item => item is null || item.Id != 0))
            return BadRequest(new { error = "Кожен елемент має бути об'єктом без заданого Id." });
        var created = items.Select(FromDto).ToArray();
        context.TodoItems.AddRange(created);
        await context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetTodoItems), created.Select(ToDto).ToArray());
    }

    // Завдання 1. Видалення запису, 204 або 404.
    [HttpDelete("{id:long}")]
    public async Task<IActionResult> DeleteTodoItem(long id)
    {
        var item = await context.TodoItems.FindAsync(id);
        if (item is null) return NotFound();
        context.TodoItems.Remove(item);
        await context.SaveChangesAsync();
        return NoContent();
    }

    private static TodoItemDto ToDto(TodoItem item) => new()
    {
        Id = item.Id, Name = item.Name ?? string.Empty, IsComplete = item.IsComplete,
        DueDate = item.DueDate, Priority = item.Priority
    };

    private static TodoItem FromDto(TodoItemDto dto) => new()
    {
        Name = dto.Name, IsComplete = dto.IsComplete, DueDate = dto.DueDate, Priority = dto.Priority
    };
}

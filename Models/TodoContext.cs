using Microsoft.EntityFrameworkCore;

namespace ChumakovD_LR5.Models;

public class TodoContext(DbContextOptions<TodoContext> options) : DbContext(options)
{
    public DbSet<TodoItem> TodoItems => Set<TodoItem>();
}

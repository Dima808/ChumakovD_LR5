using ChumakovD_LR5.Configuration;
using ChumakovD_LR5.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

// Завдання 1. API на контролерах, EF Core InMemory і Swagger.
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Завдання 3. Перевірка конфігурації під час запуску, до першого HTTP-запиту.
builder.Services.AddOptions<DatabaseOptions>()
    .Bind(builder.Configuration.GetSection(DatabaseOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();
builder.Services.AddDbContext<TodoContext>((services, options) =>
    options.UseInMemoryDatabase(services.GetRequiredService<IOptions<DatabaseOptions>>().Value.Name));

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Локальний HTTP-профіль призначений для демонстрації; HTTPS-профіль також доступний.
if (!app.Environment.IsDevelopment()) app.UseHttpsRedirection();
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseAuthorization();
app.MapControllers();
app.Run();

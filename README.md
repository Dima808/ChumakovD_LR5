# Лабораторна робота 5

Чумаков Дмитро, група 491.
Тема: створення веб-API в ASP.NET Core зі згенерованого контролера.

## Запуск у Visual Studio 2022

1. Встановіть робоче навантаження «ASP.NET і веброзробка» та .NET 8 SDK.
2. Відкрийте `ChumakovD_LR5.sln` і дочекайтеся відновлення NuGet-пакетів.
3. Виберіть профіль `http` і запустіть проєкт без налагодження.
4. Swagger: `http://localhost:5085/swagger`.
5. Браузерний клієнт: `http://localhost:5085/`.

## Запуск через CLI

```sh
dotnet restore
dotnet build --no-restore
dotnet run --launch-profile http
```

Проєкт призначений для .NET 8. `global.json` дозволяє встановлені feature bands SDK 8.0.
Профіль `https` доступний на `https://localhost:7085`; для нього налаштуйте локальний сертифікат за документацією Microsoft.
База EF Core InMemory не потребує SQL Server та втрачає дані після зупинки процесу.
Пакет SQL Server установлено для роботи генератора контролерів; під час виконання використовується InMemory.

## Виконані пункти

1. Проєкт створено шаблоном `dotnet new webapi --use-controllers --framework net8.0`.
   Контролер справді згенеровано `dotnet-aspnet-codegenerator` 8.0.7.
   Початковий результат і журнал збережено у `Documentation`.
   Додано DTO без службового поля Secret та браузерний JavaScript-клієнт.
2. `TodoItem` і `TodoItemDto` містять `DateOnly DueDate` та `int Priority`.
   GET, POST, PUT і batch передають обидва поля. Дата має формат `yyyy-MM-dd`, пріоритет — 1..5.
3. `Database:Name` винесено в `appsettings.json`. Застосовано `[Required]`, `[MinLength(4)]`,
   `ValidateDataAnnotations()` і `ValidateOnStart()`. Помилки видно в консолі запуску.
4. Після методу GET усієї колекції додано перевантаження `GetTodoItems(long[] ids)`.
   Шлях: `/api/TodoItems/by-ids?ids=1&ids=3`. Потрібні щонайменше два різні додатні Id.
   Повторення Id не дублює результати; невідомі Id не потрапляють у відповідь.
5. Після `PostTodoItem` додано `PostTodoItems` на `/api/TodoItems/batch`.
   Приймається від 1 до 200 об’єктів без заданого Id. Повертається 201 і весь масив із серверними Id.
   Некоректний масив відхиляється до `AddRange` і `SaveChangesAsync`.

## JSON і HTTP-приклади

- `ChumakovD_LR5.http` — запити для Visual Studio HTTP editor / REST Client.
- `src/data/todo-single.json` — один запис.
- `src/data/todo-batch.json` — масив із трьох записів.
- `src/data/todos.json` — 120 логічних завдань з тем API, без Id, для масового імпорту.
- `src/data/todo-update.json` — приклад PUT; замініть Id на фактично створений.
- `src/data/database-*.json` — правильна і неправильні конфігурації.
- `src/data/todo-invalid-*.json` — негативні приклади.

Імпорт 120 записів у PowerShell:

```powershell
$body = Get-Content -Raw -Encoding UTF8 ./src/data/todos.json
Invoke-RestMethod -Uri http://localhost:5085/api/TodoItems/batch -Method Post -ContentType 'application/json; charset=utf-8' -Body ([Text.Encoding]::UTF8.GetBytes($body))
```

Або через curl:

```sh
curl -i http://localhost:5085/api/TodoItems/batch -H 'Content-Type: application/json' --data-binary @src/data/todos.json
```

Виберіть один спосіб імпорту на чистому запуску. Повторний імпорт навмисно створює нові записи.

## Перевірки

За запущеного API виконайте:

```sh
python scripts/verify_api.py http://localhost:5085
```

Скрипт перевіряє 28 умов, створює власні записи й видаляє лише створені ним записи.
Докладні фактичні результати поточного виконання: `Documentation/api-tests.json` і `api-check.log`.
Перевірка неправильних налаштувань після збірки:

```sh
python scripts/verify_config.py
```

Вона запускає чотири окремі процеси з тимчасовими content root і перевіряє, що вони не починають слухати HTTP.
Усього під час підготовки пройдено 28 HTTP-перевірок та 4 перевірки запуску конфігурації.
Справжні скриншоти отримано з локально запущеного API у Linux, а не у Visual Studio.
Сторінки `wwwroot/evidence` — знімок фактичних вихідних файлів і журналів на час підготовки звіту;
поточні відповіді API дивіться у Swagger або на головній сторінці.

## Історія Git

У локальній історії базовий згенерований контролер закомічено в `main`, доробки — в окремій гілці `codex/chumakov-lr5`.
В архіві немає папок `.git`, `bin`, `obj`, `.vs`. Історію передано файлом `ChumakovD_LR5.bundle`.
Щоб відновити її поруч із розпакованим проєктом:

```sh
git clone -b codex/chumakov-lr5 ChumakovD_LR5.bundle ChumakovD_LR5-history
```

Публічний репозиторій: [Dima808/ChumakovD_LR5](https://github.com/Dima808/ChumakovD_LR5).
Гілка для перевірки: [codex/chumakov-lr5](https://github.com/Dima808/ChumakovD_LR5/tree/codex/chumakov-lr5).
На GitHub код і матеріали опубліковано окремими комітами у цій гілці; у віддаленій `main` залишається початковий README.
Локальну історію генерації та доробок збережено в bundle.

## Джерело

[Microsoft Learn — Create a controller-based web API with ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/tutorials/first-web-api?view=aspnetcore-8.0&tabs=visual-studio)

Використано вимоги лабораторної та правила оформлення, надані разом із завданням.

# QuizApp — образователна система тип викторина

Уеб система за създаване и провеждане на образователни викторини — с два режима: **Live сесия** (тип Kahoot, с PIN код и класация в реално време) и **самостоятелен режим** (решаване в собствено темпо).

## Технологии

- ASP.NET Core 9 (MVC)
- Entity Framework Core 9 + SQL Server
- ASP.NET Core Identity (роли: Admin / Teacher / Student)
- SignalR (реално време за Live сесиите)
- xUnit (unit тестове)

## Изисквания за стартиране

- Visual Studio 2022 (17.14+)
- .NET 9 SDK
- SQL Server Express (или друг SQL Server инстанс)
- SQL Server Management Studio (по избор, за преглед на базата)

## Стъпки за стартиране

1. **Clone на repository-то**

git clone https://github.com/bozhidarkirov/QuizApp.git

2. **Отвори `QuizApp.sln` във Visual Studio 2022**

3. **Провери connection string-а**

   Отвори `QuizApp.Web/appsettings.json` и провери дали `DefaultConnection` сочи към твоя SQL Server инстанс:
```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=.\\SQLEXPRESS;Database=QuizAppDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
   }
```
   Ако инстанцът ти се казва различно (не `SQLEXPRESS`), смени съответно.

4. **Приложи миграциите**

   Отвори Package Manager Console (Tools → NuGet Package Manager → Package Manager Console):
   - Default project: `QuizApp.Infrastructure`
   - Startup project: `QuizApp.Web`

   Изпълни:

5. **Стартирай проекта (F5)**

   При първо стартиране автоматично се seed-ват ролите (Admin/Teacher/Student), тестов admin акаунт и няколко примерни категории.

## Тестови акаунти

| Роля | Email | Парола |
|------|-------|--------|
| Admin | admin@quizapp.local | Admin123! |
| Teacher | bobi_stoqnow98@abv.bg | 0346075780Aa@ |
| Student | ilyayda155@gmail.com | 0346075780Aa@ |

Всеки новорегистриран потребител автоматично получава роля Student. Ролята може да се смени от Admin панела (`/Admin`).

## Основна функционалност

- **Admin**: управление на потребители/роли, категории
- **Teacher**: създаване/редакция/публикуване на викторини с въпроси (4 отговора, 1 верен); стартиране на Live сесия с PIN код
- **Student**: присъединяване към Live сесия чрез PIN, или самостоятелно решаване на публикувани викторини

## Unit тестове

Тестовете се намират в `QuizApp.Tests` и покриват точкуващата логика и валидацията при публикуване на викторина. Пускат се през Test Explorer (Test → Test Explorer → Run All Tests).
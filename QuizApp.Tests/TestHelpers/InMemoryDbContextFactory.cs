using Microsoft.EntityFrameworkCore;
using QuizApp.Infrastructure.Data;

namespace QuizApp.Tests.TestHelpers;

public static class InMemoryDbContextFactory
{
    public static ApplicationDbContext Create()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()) // уникално име за всеки тест = пълна изолация
            .Options;

        return new ApplicationDbContext(options);
    }
}
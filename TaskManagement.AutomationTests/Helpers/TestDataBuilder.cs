using Bogus;

namespace TaskManagement.AutomationTests.Helpers;

public static class TestDataBuilder
{
    public static (string Name, string Email, string Password) NewRegisterUser()
    {
        var faker = new Faker();
        var name = faker.Name.FullName();
        var email = $"autotest.{Guid.NewGuid():N}@example.com";
        var password = "Test@" + faker.Random.Number(10000, 99999);
        return (name, email, password);
    }

    public static (string Title, string Description, DateTime DueDate) NewTask()
    {
        var faker = new Faker();
        return (
            Title: faker.Lorem.Sentence(4),
            Description: faker.Lorem.Paragraph(1),
            DueDate: faker.Date.Future(1)
        );
    }
}
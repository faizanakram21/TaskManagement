using System.Net;
using FluentAssertions;
using TaskManagement.AutomationTests.API.Clients;
using TaskManagement.AutomationTests.Helpers;

namespace TaskManagement.AutomationTests.API.Tests;

[TestFixture]
[Category("API")]
public class AuthApiTests
{
    private AuthApiClient _authClient = null!;

    [SetUp]
    public void SetUp()
    {
        _authClient = new AuthApiClient();
    }

    [Test]
    public void Register_WithValidData_ReturnsCreatedUserAndToken()
    {
        var (name, email, password) = TestDataBuilder.NewRegisterUser();

        var response = _authClient.Register(new RegisterDto(name, email, password));

        response.IsSuccessful.Should().BeTrue(
            $"registration should succeed for valid data. Body: {response.Content}");
        response.Data.Should().NotBeNull();
        response.Data!.Email.Should().Be(email);
        response.Data.Token.Should().NotBeNullOrWhiteSpace();
    }

    [TestCase("", "notanemail", "Test@123")]
    [TestCase("Name", "valid@example.com", "")]
    [TestCase("Name", "not-an-email", "Test@123")]
    public void Register_WithInvalidData_ReturnsBadRequest(string name, string email, string password)
    {
        var response = _authClient.Register(new RegisterDto(name, email, password));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test]
    public void Login_WithValidCredentials_ReturnsTokenAndUserInfo()
    {
        var (name, email, password) = TestDataBuilder.NewRegisterUser();
        _authClient.Register(new RegisterDto(name, email, password));

        var response = _authClient.Login(new LoginDto(email, password));

        response.IsSuccessful.Should().BeTrue();
        response.Data.Should().NotBeNull();
        response.Data!.Token.Should().NotBeNullOrWhiteSpace();
        response.Data.Email.Should().Be(email);
    }

    [Test]
    public void Login_WithWrongPassword_ReturnsUnauthorized()
    {
        var (name, email, password) = TestDataBuilder.NewRegisterUser();
        _authClient.Register(new RegisterDto(name, email, password));

        var response = _authClient.Login_Raw(new LoginDto(email, "WrongPassword123"));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Test]
    public void Login_WithNonExistentUser_ReturnsUnauthorized()
    {
        var response = _authClient.Login_Raw(
            new LoginDto($"ghost.{Guid.NewGuid():N}@example.com", "Whatever@123"));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Test]
    public void Login_Response_MatchesExpectedContract()
    {
        var (name, email, password) = TestDataBuilder.NewRegisterUser();
        _authClient.Register(new RegisterDto(name, email, password));

        var response = _authClient.Login(new LoginDto(email, password));

        response.Data.Should().NotBeNull();
        response.Data!.Token.Should().NotBeNullOrEmpty();
        response.Data.Name.Should().NotBeNullOrEmpty();
        response.Data.Email.Should().NotBeNullOrEmpty();
        response.Data.Role.Should().NotBeNullOrEmpty();
    }
}
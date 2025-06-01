using System.Net.Http.Json;
using System.IdentityModel.Tokens.Jwt;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using Subman.Models;
using Xunit;

namespace Subman.Tests;

public class SubscriptionControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>, IAsyncLifetime
{
    private readonly HttpClient _client;
    private string _token = string.Empty;
    private string _userId = string.Empty;
    private readonly List<string> _createdSubscriptionIds = new();

    public SubscriptionControllerTests(CustomWebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient(); // Creates in-memory test server
    }

    // This method is called before each test
    public async Task InitializeAsync()
    {
        _token = await RegisterAndLoginAsync();
        _userId = ExtractUserIdFromToken(_token);
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_token}");
    }

    // This method is called after each test
    public async Task DisposeAsync()
    {
        await CleanupSubscriptionsAsync();
        await CleanupUserAsync(_userId);
    }

    // Method to register and login a new user, so the tests can use the token
    private async Task<string> RegisterAndLoginAsync()
    {
        // Register
        var register = new UserRegister
        {
            Username = "testuser",
            Email = "testuser@example.com",
            Password = "Test123!"
        };
        var regResponse = await _client.PostAsJsonAsync("api/auth/register", register);
        regResponse.EnsureSuccessStatusCode();

        // Login
        var login = new UserLogin
        {
            Email = "testuser@example.com",
            Password = "Test123!"
        };
        var loginResponse = await _client.PostAsJsonAsync("api/auth/login", login);
        loginResponse.EnsureSuccessStatusCode();

        return await loginResponse.Content.ReadAsStringAsync();
    }

    // Method to extract the user ID from the token
    private string ExtractUserIdFromToken(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadToken(token) as JwtSecurityToken;
        return jwt?.Claims.FirstOrDefault(c => c.Type == "userId")?.Value ?? throw new Exception("User ID missing in token");
    }

    // Method to clean up the user after the test (email is unique, so the user needs to be deleted each time)
    private async Task CleanupUserAsync(string userId)
    {
        var response = await _client.DeleteAsync($"/api/user/{userId}");
        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"Failed to delete user: {response.StatusCode}");
        }
    }

    // Method to clean up the subscriptions after the test
    private async Task CleanupSubscriptionsAsync()
    {
        foreach (var id in _createdSubscriptionIds)
        {
            await _client.DeleteAsync($"/api/subscription/{id}");
        }
    }

    // Method to generate a subscription
    private Subscription GenerateSubscription(string name = "Test Subscription") => new()
    {
        UserId = _userId,
        Name = name,
        Description = "Test description",
        Price = 9.99,
        Currency = "USD",
        StartDate = DateTime.UtcNow,
        Interval = 30
    };

    [Fact]
    public async Task CreateSubscription_ShouldReturnCreated()
    {
        // Arrange
        var sub = GenerateSubscription("Create Test");

        // Act
        var response = await _client.PostAsJsonAsync("/api/subscription", sub);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);

        var created = await response.Content.ReadFromJsonAsync<Subscription>();
        created.Should().NotBeNull("Expected the created subscription to be returned");

        created!.Name.Should().Be(sub.Name);
        created.UserId.Should().Be(sub.UserId);

        // Add the created subscription ID to the list so it can be cleaned up
        _createdSubscriptionIds.Add(created.Id!);
    }

    [Fact]
    public async Task CreateSubscription_WithInvalidData_ShouldReturnBadRequest()
    {
        // Arrange: create a subscription with missing required fields (e.g., no name)
        var invalidSub = new Subscription
        {
            UserId = _userId,
            Description = "Missing name field",
            Price = 9.99,
            Currency = "USD",
            StartDate = DateTime.UtcNow,
            Interval = 30
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/subscription", invalidSub);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateMultipleSubscriptions_AndGetAll_ShouldReturnAll()
    {
        // Arrange
        var sub1 = GenerateSubscription("Sub One");
        var sub2 = GenerateSubscription("Sub Two");

        var create1 = await _client.PostAsJsonAsync("/api/subscription", sub1);
        var created1 = await create1.Content.ReadFromJsonAsync<Subscription>();

        var create2 = await _client.PostAsJsonAsync("/api/subscription", sub2);
        var created2 = await create2.Content.ReadFromJsonAsync<Subscription>();

        _createdSubscriptionIds.Add(created1!.Id!);
        _createdSubscriptionIds.Add(created2!.Id!);

        // Act
        var response = await _client.GetAsync("/api/subscription");

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var list = await response.Content.ReadFromJsonAsync<List<Subscription>>();
        list.Should().Contain(x => x.Id == created1.Id);
        list.Should().Contain(x => x.Id == created2.Id);
    }

    [Fact]
    public async Task UpdateNonExistentSubscription_ShouldReturnNotFound()
    {
        // Arrange: Use a fake ID
        var fakeId = "nonexistent-id-123";
        var sub = GenerateSubscription("Fake Update");
        sub.Id = fakeId;

        // Act
        var response = await _client.PutAsJsonAsync($"/api/subscription/{fakeId}", sub);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetAllSubscriptions_ShouldReturnList()
    {
        // Arrange
        var sub = GenerateSubscription("GetAll Test");
        var create = await _client.PostAsJsonAsync("/api/subscription", sub);
        var created = await create.Content.ReadFromJsonAsync<Subscription>();

        // Act
        var response = await _client.GetAsync("/api/subscription");

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var list = await response.Content.ReadFromJsonAsync<List<Subscription>>();
        list.Should().NotBeNull();
        list.Should().NotBeEmpty();
        list.Should().Contain(x => x.Id == created!.Id);

        // Add the created subscription ID to the list - so it can be removed later
        _createdSubscriptionIds.Add(created.Id!);
    }

    [Fact]
    public async Task GetSubscriptionById_ShouldReturnCorrectSubscription()
    {
        // Arrange
        var sub = GenerateSubscription("GetById Test");
        var create = await _client.PostAsJsonAsync("/api/subscription", sub);
        var created = await create.Content.ReadFromJsonAsync<Subscription>();

        // Act
        var response = await _client.GetAsync($"/api/subscription/{created!.Id}");

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<Subscription>();
        result.Should().NotBeNull();

        // Add the created subscription ID to the list - so it can be removed later
        _createdSubscriptionIds.Add(created.Id!);
    }

    [Fact]
    public async Task GetSubscriptionByUserId_ShouldReturnList()
    {
        // Arrange
        var sub = GenerateSubscription("GetByUserId Test");
        var create = await _client.PostAsJsonAsync("/api/subscription", sub);
        var created = await create.Content.ReadFromJsonAsync<Subscription>();

        // Act
        var response = await _client.GetAsync($"/api/subscription/user/{created!.UserId}");

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var list = await response.Content.ReadFromJsonAsync<List<Subscription>>();
        list.Should().NotBeNull();
        list.Should().NotBeEmpty();
        list.Should().Contain(x => x.Id == created!.Id);
    }

    [Fact]
    public async Task UpdateSubscription_ShouldModifyFields()
    {
        // Arrange
        var sub = GenerateSubscription("Update Test");
        var create = await _client.PostAsJsonAsync("/api/subscription", sub);
        var created = await create.Content.ReadFromJsonAsync<Subscription>();

        // Act
        created!.Name = "Updated Name";
        created.Description = "Updated Description";

        var update = await _client.PutAsJsonAsync($"/api/subscription/{created.Id}", created);

        // Assert
        update.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var updated = await update.Content.ReadFromJsonAsync<Subscription>();
        updated!.Name.Should().Be("Updated Name");
        updated.Description.Should().Be("Updated Description");

        // Add the created subscription ID to the list - so it can be removed later
        _createdSubscriptionIds.Add(created.Id!);
    }

    [Fact]
    public async Task DeleteSubscription_ShouldReturnNoContent_AndRemoveIt()
    {
        // Arrange
        var sub = GenerateSubscription("Delete Test");
        var create = await _client.PostAsJsonAsync("/api/subscription", sub);
        var created = await create.Content.ReadFromJsonAsync<Subscription>();

        // Act
        var delete = await _client.DeleteAsync($"/api/subscription/{created!.Id}");

        // Assert
        delete.StatusCode.Should().Be(System.Net.HttpStatusCode.NoContent);

        var get = await _client.GetAsync($"/api/subscription/{created.Id}");
        get.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }
}
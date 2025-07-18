using Application.Interfaces.Data;
using Application.Operations.Queries.User;
using Domain.Entities;
using Domain.Enums;
using Moq;

namespace Application.UnitTests.Queries;

public class GetUserByEmailQueryHandlerTests
{
    private readonly Mock<IUserQueryRepository> _userQueryRepositoryMock;
    private readonly GetUserByEmailQueryHandler _queryHandler;

    public GetUserByEmailQueryHandlerTests()
    {
        _userQueryRepositoryMock = new Mock<IUserQueryRepository>();
        _queryHandler = new GetUserByEmailQueryHandler(_userQueryRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_UserExists_ReturnsUser()
    {
        // Arrange
        var email = "test@example.com";
        var user = new User("testuser", email, "Test User", new List<UserRole> { UserRole.Administrator });
        _userQueryRepositoryMock.Setup(repo => repo.GetByEmailAsync(email)).ReturnsAsync(user);
        var query = new GetUserByEmailQuery(email);

        // Act
        var result = await _queryHandler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(email, result.Email);
    }

    [Fact]
    public async Task Handle_UserDoesNotExist_ReturnsNull()
    {
        // Arrange
        var email = "nonexistent@example.com";
        _userQueryRepositoryMock.Setup(repo => repo.GetByEmailAsync(email)).ReturnsAsync((User?)null);
        var query = new GetUserByEmailQuery(email);

        // Act
        var result = await _queryHandler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Null(result);
    }
}

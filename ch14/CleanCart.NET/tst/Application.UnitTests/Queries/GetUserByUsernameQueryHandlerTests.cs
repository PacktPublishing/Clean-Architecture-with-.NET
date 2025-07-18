using Application.Interfaces.Data;
using Application.Operations.Queries.User;
using Domain.Entities;
using Domain.Enums;
using Moq;

namespace Application.UnitTests.Queries;

public class GetUserByUsernameQueryHandlerTests
{
    private readonly Mock<IUserQueryRepository> _userQueryRepositoryMock;
    private readonly GetUserByUsernameQueryHandler _queryHandler;

    public GetUserByUsernameQueryHandlerTests()
    {
        _userQueryRepositoryMock = new Mock<IUserQueryRepository>();
        _queryHandler = new GetUserByUsernameQueryHandler(_userQueryRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_UserExists_ReturnsUser()
    {
        // Arrange
        var username = "testuser";
        var user = new User(username, "testuser@example.com", "Test User", new List<UserRole> { UserRole.Administrator });
        _userQueryRepositoryMock.Setup(repo => repo.GetByUsernameAsync(username)).ReturnsAsync(user);
        var query = new GetUserByUsernameQuery(username);

        // Act
        var result = await _queryHandler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(username, result.Username);
    }

    [Fact]
    public async Task Handle_UserDoesNotExist_ReturnsNull()
    {
        // Arrange
        var username = "nonexistentuser";
        _userQueryRepositoryMock.Setup(repo => repo.GetByUsernameAsync(username)).ReturnsAsync((User?)null);
        var query = new GetUserByUsernameQuery(username);

        // Act
        var result = await _queryHandler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Null(result);
    }
}

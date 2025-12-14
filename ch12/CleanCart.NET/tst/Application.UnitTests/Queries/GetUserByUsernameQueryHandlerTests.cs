using Application.Interfaces.Data;
using Application.Operations.Queries.User;
using Domain.Entities;
using Domain.Enums;
using NSubstitute;

namespace Application.UnitTests.Queries;

public class GetUserByUsernameQueryHandlerTests
{
    private readonly IUserQueryRepository _userQueryRepositoryMock;
    private readonly GetUserByUsernameQueryHandler _queryHandler;

    public GetUserByUsernameQueryHandlerTests()
    {
        _userQueryRepositoryMock = Substitute.For<IUserQueryRepository>();
        _queryHandler = new GetUserByUsernameQueryHandler(_userQueryRepositoryMock);
    }

    [Fact]
    public async Task Handle_UserExists_ReturnsUser()
    {
        var username = "testuser";
        var user = new User(username, "testuser@example.com", "Test User", new List<UserRole> { UserRole.Administrator });
        _userQueryRepositoryMock.GetByUsernameAsync(username).Returns(user);
        var query = new GetUserByUsernameQuery(username);

        var result = await _queryHandler.Handle(query, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(username, result.Username);
    }

    [Fact]
    public async Task Handle_UserDoesNotExist_ReturnsNull()
    {
        var username = "nonexistentuser";
        _userQueryRepositoryMock.GetByUsernameAsync(username).Returns((User?)null);
        var query = new GetUserByUsernameQuery(username);

        var result = await _queryHandler.Handle(query, CancellationToken.None);

        Assert.Null(result);
    }
}

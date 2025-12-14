using Application.Interfaces.Data;
using Application.Operations.Queries.User;
using Domain.Entities;
using Domain.Enums;
using NSubstitute;

namespace Application.UnitTests.Queries;

public class GetUserByEmailQueryHandlerTests
{
    private readonly IUserQueryRepository _userQueryRepositoryMock;
    private readonly GetUserByEmailQueryHandler _queryHandler;

    public GetUserByEmailQueryHandlerTests()
    {
        _userQueryRepositoryMock = Substitute.For<IUserQueryRepository>();
        _queryHandler = new GetUserByEmailQueryHandler(_userQueryRepositoryMock);
    }

    [Fact]
    public async Task Handle_UserExists_ReturnsUser()
    {
        var email = "test@example.com";
        var user = new User("testuser", email, "Test User", new List<UserRole> { UserRole.Administrator });
        _userQueryRepositoryMock.GetByEmailAsync(email).Returns(user);
        var query = new GetUserByEmailQuery(email);

        var result = await _queryHandler.Handle(query, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(email, result.Email);
    }

    [Fact]
    public async Task Handle_UserDoesNotExist_ReturnsNull()
    {
        var email = "nonexistent@example.com";
        _userQueryRepositoryMock.GetByEmailAsync(email).Returns((User?)null);
        var query = new GetUserByEmailQuery(email);

        var result = await _queryHandler.Handle(query, CancellationToken.None);

        Assert.Null(result);
    }
}

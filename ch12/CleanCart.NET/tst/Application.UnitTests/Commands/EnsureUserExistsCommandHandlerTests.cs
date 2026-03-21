using Application.Interfaces.Data;
using Application.Operations.Commands.User;
using Domain.Entities;
using Domain.Enums;
using NSubstitute;

namespace Application.UnitTests.Commands;

public class EnsureUserExistsCommandHandlerTests
{
    private readonly IUserQueryRepository _mockUserQueryRepository;
    private readonly IUserCommandRepository _mockUserCommandRepository;
    private readonly EnsureUserExistsCommandHandler _commandHandler;

    public EnsureUserExistsCommandHandlerTests()
    {
        _mockUserQueryRepository = Substitute.For<IUserQueryRepository>();
        _mockUserCommandRepository = Substitute.For<IUserCommandRepository>();
        _commandHandler = new EnsureUserExistsCommandHandler(_mockUserQueryRepository, _mockUserCommandRepository);
    }

    [Fact]
    public async Task Handle_Should_Not_Create_User_When_User_Already_Exists()
    {
        var command = new EnsureUserExistsCommand
        {
            Username = "user@example.com",
            Email = "user@example.com",
            FullName = "Test User"
        };

        var existingUser = new User(
            command.Username,
            command.Email,
            command.FullName,
            [UserRole.Administrator]);

        _mockUserQueryRepository.GetByUsernameAsync(command.Username)
            .Returns(existingUser);

        await _commandHandler.Handle(command, CancellationToken.None);

        await _mockUserCommandRepository
            .DidNotReceive()
            .CreateAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_Create_User_When_User_Does_Not_Exist()
    {
        var command = new EnsureUserExistsCommand
        {
            Username = "user@example.com",
            Email = "user@example.com",
            FullName = "Test User"
        };

        _mockUserQueryRepository.GetByUsernameAsync(command.Username)
            .Returns((User?)null);

        await _commandHandler.Handle(command, CancellationToken.None);

        await _mockUserCommandRepository
            .Received(1)
            .CreateAsync(
                Arg.Is<User>(u =>
                    u.Username == command.Username &&
                    u.Email == command.Email &&
                    u.FullName == command.FullName &&
                    u.Roles.Contains(UserRole.Administrator) &&
                    u.Roles.Contains(UserRole.CustomerService)),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_Throw_When_Cancellation_Is_Requested()
    {
        var command = new EnsureUserExistsCommand
        {
            Username = "user@example.com",
            Email = "user@example.com",
            FullName = "Test User"
        };

        using var cancellationTokenSource = new CancellationTokenSource();
        await cancellationTokenSource.CancelAsync();

        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            _commandHandler.Handle(command, cancellationTokenSource.Token));

        await _mockUserQueryRepository
            .DidNotReceive()
            .GetByUsernameAsync(Arg.Any<string>());

        await _mockUserCommandRepository
            .DidNotReceive()
            .CreateAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
    }
}
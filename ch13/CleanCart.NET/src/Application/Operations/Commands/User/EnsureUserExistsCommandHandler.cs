using Application.Interfaces.Data;
using Domain.Enums;
using MediatR;

namespace Application.Operations.Commands.User;

public sealed class EnsureUserExistsCommandHandler(IUserQueryRepository userQueries, IUserCommandRepository userCommands)
    : IRequestHandler<EnsureUserExistsCommand>
{
    /// <inheritdoc />
    public async Task Handle(EnsureUserExistsCommand request, CancellationToken cancellationToken)
    {
        // Ideally, subsequent operations should accept the cancellation token and handle it appropriately.
        // Our interfaces intentionally do not include cancellation tokens to simplify the API,
        // but in a real-world application, you should consider how to propagate cancellation through your layers.
        cancellationToken.ThrowIfCancellationRequested();

        Domain.Entities.User? existingUser = await userQueries.GetByUsernameAsync(request.Username);

        if (existingUser is not null)
        {
            return;
        }

        // For testing purposes, grant all roles.
        // In a real-world scenario, assign roles based on permissions and business rules.
        // This would likely happen through a registration flow or admin onboarding process
        // rather than automatically in a command handler.
        List<UserRole> defaultRoles =
        [
            UserRole.Administrator,
            UserRole.CustomerService
        ];

        Domain.Entities.User newUser = new(request.Username, request.Email, request.FullName, defaultRoles);

        await userCommands.CreateAsync(newUser, cancellationToken);
    }
}
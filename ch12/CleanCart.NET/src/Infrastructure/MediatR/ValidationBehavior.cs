using FluentValidation;
using MediatR;

namespace Infrastructure.MediatR;

public class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var context = new ValidationContext<TRequest>(request);

        var failures = await Task.WhenAll(validators.Select(v => v.ValidateAsync(context, cancellationToken)));
        var errors = failures.SelectMany(f => f.Errors).Where(f => f != null).ToList();

        if (errors.Count != 0)
            throw new ValidationException(errors);

        return await next(cancellationToken);
    }
}

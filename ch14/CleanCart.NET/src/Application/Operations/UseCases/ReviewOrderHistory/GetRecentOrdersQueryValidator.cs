using FluentValidation;

namespace Application.Operations.UseCases.ReviewOrderHistory;

public class GetRecentOrdersQueryValidator : AbstractValidator<GetRecentOrdersQuery>
{
    public GetRecentOrdersQueryValidator()
    {
        RuleFor(q => q.WithinLast)
            .GreaterThan(TimeSpan.Zero)
            .WithMessage("Time window must be greater than zero.");
    }
}
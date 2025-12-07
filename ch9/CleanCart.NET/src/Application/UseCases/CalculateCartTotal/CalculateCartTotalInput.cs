namespace Application.UseCases.CalculateCartTotal;

public class CalculateCartTotalInput(Guid userId)
{
    public Guid UserId { get; } = userId;
}
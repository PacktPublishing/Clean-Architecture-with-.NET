using FluentValidation;

namespace Application.Operations.UseCases.ManageProductInventory;

public class UpdateProductInventoryCommandValidator : AbstractValidator<UpdateProductInventoryCommand>
{
    public UpdateProductInventoryCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.StockLevel).GreaterThanOrEqualTo(0);
    }
}

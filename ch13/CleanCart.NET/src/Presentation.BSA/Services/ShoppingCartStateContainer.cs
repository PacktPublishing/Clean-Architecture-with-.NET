namespace Presentation.BSA.Services;

public class ShoppingCartStateContainer
{
    public event Action? OnChange;

    public void NotifyCartChanged() => OnChange?.Invoke();
}

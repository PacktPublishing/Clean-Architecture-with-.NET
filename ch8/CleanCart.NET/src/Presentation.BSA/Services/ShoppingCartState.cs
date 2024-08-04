namespace Presentation.BSA.Services;

public class ShoppingCartState
{
    public event Action? OnChange;

    public void NotifyCartChanged() => OnChange?.Invoke();
}

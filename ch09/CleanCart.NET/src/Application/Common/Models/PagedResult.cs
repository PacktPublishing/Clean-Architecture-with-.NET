namespace Application.Common.Models;

public sealed class PagedResult<T>(
    IReadOnlyList<T> items,
    int totalCount,
    int page,
    int pageSize)
{
    public IReadOnlyList<T> Items { get; } = items ?? throw new ArgumentNullException(nameof(items));
    public int TotalCount { get; } = totalCount;
    public int Page { get; } = page;
    public int PageSize { get; } = pageSize;
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}
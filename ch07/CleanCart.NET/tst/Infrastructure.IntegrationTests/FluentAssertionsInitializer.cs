using FluentAssertions;
using System.Runtime.CompilerServices;
using TestCommon;

namespace Infrastructure.IntegrationTests;

internal static class FluentAssertionsInitializer
{
    [ModuleInitializer]
    internal static void Initialize()
    {
        AssertionOptions.AssertEquivalencyUsing(options =>
        {
            // Exclude properties starting with "Nav" within the Items collection
            // Example Path: orders[0].Items[0].NavProduct
            options.Excluding(x => x.Path.Contains("Nav"));
            return options.Using(new RelaxedEnumEquivalencyStep());
        });
    }
}
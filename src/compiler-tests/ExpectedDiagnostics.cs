using System.Text;
using Noa.Compiler.Diagnostics;

namespace Noa.Compiler.Tests;

internal static class ExpectedDiagnostics
{
    /// <summary>
    /// Asserts that a collection of diagnostics is an expected set of diagnostics.
    /// </summary>
    /// <param name="diagnostics">The diagnostics to assert.</param>
    /// <param name="expected">The expected diagnostics.</param>
    /// <param name="ignoreAdditional">
    /// Whether to ignore additional diagnostics not in <paramref name="expected"/>.
    /// </param>
    /// <param name="ignoredIds">
    /// A set of diagnostic IDs which unexpected diagnostics with will be ignored.
    /// </param>
    /// <param name="ignoredCategories">
    /// A set of category names which unexpected diagnostics in will be ignored.
    /// </param>
    public static void DiagnosticsShouldBe(
        this IEnumerable<IDiagnostic> diagnostics,
        IEnumerable<(DiagnosticId, Location)> expected,
        bool ignoreAdditional = false,
        IEnumerable<DiagnosticId>? ignoredIds = null,
        IEnumerable<string>? ignoredCategories = null)
    {
        var actual = diagnostics
            .Select(d => (d.Id, d.Location))
            .ToHashSet();

        foreach (var (id, location) in expected)
        {
            var success = actual.Remove((id, location));
            Assert.True(success, $"Expected {id} at {location}");
        }

        if (actual.Count == 0 || ignoreAdditional) return;

        var ignoredIdsSet = ignoredIds?.ToHashSet() ?? [];
        var ignoredCategoriesSet = ignoredCategories?.ToHashSet() ?? [];
        var unexpected = new List<(DiagnosticId, Location)>();
        foreach (var (id, location) in actual)
        {
            if (ignoredIdsSet.Contains(id)) continue;
            if (ignoredCategoriesSet.Contains(id.Category)) continue;
            unexpected.Add((id, location));
        }
        
        if (unexpected.Count == 0) return;
        
        var builder = new StringBuilder();
        builder.AppendLine("Unexpected diagnostics:");

        foreach (var (id, location) in unexpected)
        {
            builder.AppendLine($"    {id} at {location}");
        }
            
        Assert.Fail(builder.ToString());
    }
}

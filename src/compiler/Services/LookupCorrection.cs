// ReSharper disable IdentifierTypo
// ReSharper disable TooWideLocalVariableScope

using Noa.Compiler.Nodes;
using Noa.Compiler.Symbols;

namespace Noa.Compiler.Services;

internal static class LookupCorrection
{
    private const int MaxDistance = 3;
    
    /// <summary>
    /// Finds possible correction symbols for a typo in a symbol name.
    /// </summary>
    /// <param name="name">The name which might have a typo.</param>
    /// <param name="scope">The scope to find correction symbols in.</param>
    /// <param name="at">The node at which to look up the correction symbols.</param>
    public static IReadOnlyCollection<ISymbol> FindPossibleCorrections(string name, IScope scope, Node at) =>
        scope.AccessibleAt(at)
            .Where(symbol => Distance(name, symbol.Name, MaxDistance + 1) <= MaxDistance)
            .ToArray();

    // ------------------------------------------------------------------------------------------------------------
    // Adapted from
    // https://github.com/feature23/StringSimilarity.NET/blob/main/src/F23.StringSimilarity/Levenshtein.cs#L81-L164
    // ------------------------------------------------------------------------------------------------------------
    private static int Distance(ReadOnlySpan<char> s1, ReadOnlySpan<char> s2, int limit)
    {
        if (s1.SequenceEqual(s2))
        {
            return 0;
        }

        if (s1.Length == 0)
        {
            return s2.Length;
        }

        if (s2.Length == 0)
        {
            return s1.Length;
        }

        // create two work vectors of integer distances
        var v0 = new int[s2.Length + 1];
        var v1 = new int[s2.Length + 1];
        int[] vtemp;

        // initialize v0 (the previous row of distances)
        // this row is A[0][i]: edit distance for an empty s
        // the distance is just the number of characters to delete from t
        for (var i = 0; i < v0.Length; i++)
        {
            v0[i] = i;
        }

        for (var i = 0; i < s1.Length; i++)
        {
            // calculate v1 (current row distances) from the previous row v0
            // first element of v1 is A[i+1][0]
            //   edit distance is delete (i+1) chars from s to match empty t
            v1[0] = i + 1;

            var minv1 = v1[0];

            // use formula to fill in the rest of the row
            for (var j = 0; j < s2.Length; j++)
            {
                var cost = 1;
                if (s1[i].Equals(s2[j]))
                {
                    cost = 0;
                }

                v1[j + 1] = Math.Min(
                    v1[j] + 1, // Cost of insertion
                    Math.Min(
                        v0[j + 1] + 1, // Cost of remove
                        v0[j] + cost)); // Cost of substitution

                minv1 = Math.Min(minv1, v1[j + 1]);
            }

            if (minv1 >= limit)
            {
                return limit;
            }

            // copy v1 (current row) to v0 (previous row) for next iteration
            // System.arraycopy(v1, 0, v0, 0, v0.length);

            // Flip references to current and previous row
            vtemp = v0;
            v0 = v1;
            v1 = vtemp;
        }

        return v0[s2.Length];
    }
}

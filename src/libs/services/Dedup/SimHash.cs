using System.Numerics;
using System.Text;

namespace TNO.Services.Dedup;

/// <summary>
/// SimHash static class, provides a 64-bit locality sensitive fingerprint of text.
/// Texts that share most of their word sequences produce fingerprints that differ in few bits.
/// The hash is stable across processes and machines so fingerprints can be stored and compared later.
/// </summary>
public static class SimHash
{
    #region Variables
    private const ulong FnvOffsetBasis = 14695981039346656037UL;
    private const ulong FnvPrime = 1099511628211UL;
    #endregion

    #region Methods
    /// <summary>
    /// Compute the fingerprint of the specified words, using overlapping word sequences (shingles) as features.
    /// </summary>
    /// <param name="tokens">Normalized words.</param>
    /// <param name="shingleSize">Number of words in each feature.</param>
    /// <returns></returns>
    public static ulong Compute(IReadOnlyList<string> tokens, int shingleSize = 3)
    {
        return Compute(Shingles(tokens, shingleSize));
    }

    /// <summary>
    /// Compute the fingerprint of the specified features.
    /// </summary>
    /// <param name="features"></param>
    /// <returns></returns>
    public static ulong Compute(IEnumerable<string> features)
    {
        var weights = new int[64];
        foreach (var feature in features)
        {
            var hash = Hash(feature);
            for (var bit = 0; bit < 64; bit++)
            {
                weights[bit] += ((hash >> bit) & 1UL) == 1UL ? 1 : -1;
            }
        }

        var result = 0UL;
        for (var bit = 0; bit < 64; bit++)
        {
            if (weights[bit] > 0) result |= 1UL << bit;
        }
        return result;
    }

    /// <summary>
    /// Overlapping sequences of 'size' words.
    /// Fewer words than 'size' returns a single feature of all the words.
    /// </summary>
    /// <param name="tokens"></param>
    /// <param name="size"></param>
    /// <returns></returns>
    public static IEnumerable<string> Shingles(IReadOnlyList<string> tokens, int size = 3)
    {
        if (tokens.Count == 0) yield break;
        if (size < 1) size = 1;
        if (tokens.Count <= size)
        {
            yield return String.Join(' ', tokens);
            yield break;
        }
        for (var i = 0; i <= tokens.Count - size; i++)
        {
            yield return String.Join(' ', tokens.Skip(i).Take(size));
        }
    }

    /// <summary>
    /// Number of bits that differ between two fingerprints.
    /// </summary>
    /// <param name="first"></param>
    /// <param name="second"></param>
    /// <returns></returns>
    public static int Distance(ulong first, ulong second)
    {
        return BitOperations.PopCount(first ^ second);
    }

    /// <summary>
    /// Similarity of two fingerprints, 1 when identical and around 0.5 for unrelated text.
    /// </summary>
    /// <param name="first"></param>
    /// <param name="second"></param>
    /// <returns></returns>
    public static double Similarity(ulong first, ulong second)
    {
        return 1d - Distance(first, second) / 64d;
    }

    /// <summary>
    /// A stable 64-bit hash (FNV-1a with a MurmurHash3 finalizer so every bit depends on every input byte).
    /// Never use string.GetHashCode here, it is randomized per process.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static ulong Hash(string value)
    {
        var hash = FnvOffsetBasis;
        foreach (var b in Encoding.UTF8.GetBytes(value))
        {
            hash ^= b;
            hash *= FnvPrime;
        }

        hash ^= hash >> 33;
        hash *= 0xff51afd7ed558ccdUL;
        hash ^= hash >> 33;
        hash *= 0xc4ceb9fe1a85ec53UL;
        hash ^= hash >> 33;
        return hash;
    }
    #endregion
}

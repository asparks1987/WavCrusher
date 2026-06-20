using System.Text.RegularExpressions;

namespace WavCrusher.Domain;

public sealed partial record HashDigest(string Algorithm, string Hex)
{
    public static HashDigest Sha256(string hex)
    {
        if (!Sha256Regex().IsMatch(hex))
        {
            throw new ArgumentException("SHA-256 hashes must be 64 lowercase hexadecimal characters.", nameof(hex));
        }

        return new HashDigest("sha256", hex);
    }

    [GeneratedRegex("^[0-9a-f]{64}$", RegexOptions.CultureInvariant)]
    private static partial Regex Sha256Regex();
}

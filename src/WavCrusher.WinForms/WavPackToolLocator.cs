namespace WavCrusher.WinForms;

internal static class WavPackToolLocator
{
    public static (string WavPackPath, string WvUnpackPath) Locate()
    {
        var current = AppContext.BaseDirectory;
        while (!string.IsNullOrEmpty(current))
        {
            var candidate = Path.Combine(current, "third_party", "wavpack", "win-x64");
            var wavpack = Path.Combine(candidate, "wavpack.exe");
            var wvunpack = Path.Combine(candidate, "wvunpack.exe");
            if (File.Exists(wavpack) && File.Exists(wvunpack))
            {
                return (wavpack, wvunpack);
            }

            current = Directory.GetParent(current)?.FullName ?? string.Empty;
        }

        var baseDirectory = AppContext.BaseDirectory;
        return (
            Path.Combine(baseDirectory, "wavpack.exe"),
            Path.Combine(baseDirectory, "wvunpack.exe"));
    }
}

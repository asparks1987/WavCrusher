using WavCrusher.Domain;

namespace WavCrusher.Domain.Tests;

public sealed class ArchiveProfileTests
{
    [Fact]
    public void PureLosslessProfile_UsesOnlyApprovedVersionOneArguments()
    {
        Assert.Equal(
            new[] { "-hh", "-x6", "-m", "-v", "-t", "-z0", "--no-overwrite" },
            ArchiveProfile.PureLosslessHighCompressionV1.EncoderArguments);

        Assert.DoesNotContain("-d", ArchiveProfile.PureLosslessHighCompressionV1.EncoderArguments);
        Assert.DoesNotContain("-b", ArchiveProfile.PureLosslessHighCompressionV1.EncoderArguments);
        Assert.DoesNotContain("-c", ArchiveProfile.PureLosslessHighCompressionV1.EncoderArguments);
        Assert.DoesNotContain("-r", ArchiveProfile.PureLosslessHighCompressionV1.EncoderArguments);
        Assert.DoesNotContain("-i", ArchiveProfile.PureLosslessHighCompressionV1.EncoderArguments);
    }
}

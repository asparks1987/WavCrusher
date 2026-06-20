using WavCrusher.Domain;

namespace WavCrusher.Application.WavPack;

public interface IWavPackEncoder
{
    Task<WavPackEncodeResult> EncodeAsync(
        WavPackEncodeRequest request,
        IProgress<ToolProgress>? progress,
        CancellationToken cancellationToken);
}

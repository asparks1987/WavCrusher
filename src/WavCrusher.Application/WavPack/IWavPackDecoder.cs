using WavCrusher.Domain;

namespace WavCrusher.Application.WavPack;

public interface IWavPackDecoder
{
    Task<WavPackDecodeResult> DecodeAsync(
        WavPackDecodeRequest request,
        IProgress<ToolProgress>? progress,
        CancellationToken cancellationToken);
}

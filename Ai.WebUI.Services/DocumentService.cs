using Ai.WebUI.DataFormats;
using Microsoft.AspNetCore.Components.Forms;

namespace Ai.WebUI.Services;

public class DocumentService(IEnumerable<IContentDecoder> decoders)
{
    private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10 MB

    public async Task<string> ExtractTextAsync(
        IBrowserFile file,
        CancellationToken cancellationToken = default)
    {
        var decoder = decoders.FirstOrDefault(d => d.SupportsMimeType(file.ContentType));
        if (decoder is null)
            return string.Empty;

        await using var stream = file.OpenReadStream(MaxFileSizeBytes, cancellationToken);
        var content = await decoder.DecodeAsync(stream, cancellationToken);
        return string.Join("\n\n", content.Sections.Select(s => s.Content));
    }

    public async Task<string> ExtractTextAsync(
        string filePath,
        string mimeType,
        CancellationToken cancellationToken = default)
    {
        var decoder = decoders.FirstOrDefault(d => d.SupportsMimeType(mimeType));
        if (decoder is null)
            return string.Empty;

        var content = await decoder.DecodeAsync(filePath, cancellationToken);
        return string.Join("\n\n", content.Sections.Select(s => s.Content));
    }

    public bool IsSupported(string mimeType) =>
        decoders.Any(d => d.SupportsMimeType(mimeType));
}

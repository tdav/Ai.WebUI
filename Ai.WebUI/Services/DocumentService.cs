using Ai.WebUI.DataFormats;

namespace Ai.WebUI.Services;

public class DocumentService(IEnumerable<IContentDecoder> decoders)
{
    private readonly IReadOnlyList<IContentDecoder> contentDecoders = decoders.ToList();
}

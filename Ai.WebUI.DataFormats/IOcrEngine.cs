// Copyright (c) Microsoft. All rights reserved.

using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Ai.WebUI.DataFormats;

/// <summary>
/// An OCR engine that can read in text from image files.
/// </summary>
public interface IOcrEngine
{
    /// <summary>
    /// Reads all text from the image.
    /// </summary>
    /// <param name="imageContent">The image content stream.</param>
    /// <param name="cancellationToken">Task cancellation token</param>
    Task<string> ExtractTextFromImageAsync(Stream imageContent, CancellationToken cancellationToken = default);
}

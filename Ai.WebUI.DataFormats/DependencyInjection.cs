// Copyright (c) Microsoft. All rights reserved.

using Ai.WebUI.DataFormats;
using Ai.WebUI.DataFormats.Image;
using Ai.WebUI.DataFormats.Office;
using Ai.WebUI.DataFormats.Pdf;
using Ai.WebUI.DataFormats.Text;
using Ai.WebUI.DataFormats.WebPages;
using Microsoft.Extensions.DependencyInjection;

namespace Ai.WebUI.DataFormats;

/// <summary>
/// .NET IServiceCollection dependency injection extensions.
/// </summary>
public static partial class DependencyInjection
{

    public static IServiceCollection AddDefaultContentDecoders(this IServiceCollection services)
    {
        services.AddSingleton<IContentDecoder, TextDecoder>();
        services.AddSingleton<IContentDecoder, MarkDownDecoder>();
        services.AddSingleton<IContentDecoder, HtmlDecoder>();
        services.AddSingleton<IContentDecoder, PdfDecoder>();
        services.AddSingleton<IContentDecoder, ImageDecoder>();
        services.AddSingleton<IContentDecoder, MsExcelDecoder>();
        services.AddSingleton<IContentDecoder, MsPowerPointDecoder>();
        services.AddSingleton<IContentDecoder, MsWordDecoder>();
        services.AddSingleton<IWebScraper, WebScraper>();

        return services;
    }


}

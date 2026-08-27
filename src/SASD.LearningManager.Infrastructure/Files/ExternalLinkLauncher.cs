using System.Diagnostics;
using SASD.LearningManager.Application.Abstractions;

namespace SASD.LearningManager.Infrastructure.Files;

/// <summary>Uses the operating-system shell to open an already validated web address.</summary>
public sealed class ExternalLinkLauncher : IExternalLinkLauncher
{
    public void Open(Uri uri)
    {
        ArgumentNullException.ThrowIfNull(uri);
        if (!uri.IsAbsoluteUri || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            throw new InvalidOperationException("Only absolute HTTP and HTTPS URLs may be opened.");
        }

        Process.Start(new ProcessStartInfo(uri.AbsoluteUri)
        {
            UseShellExecute = true
        });
    }
}

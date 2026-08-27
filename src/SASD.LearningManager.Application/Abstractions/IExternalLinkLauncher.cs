namespace SASD.LearningManager.Application.Abstractions;

/// <summary>Opens trusted, already validated HTTP/HTTPS URLs outside the application.</summary>
public interface IExternalLinkLauncher
{
    /// <summary>Opens an absolute HTTP or HTTPS URI using the operating system's default browser.</summary>
    void Open(Uri uri);
}

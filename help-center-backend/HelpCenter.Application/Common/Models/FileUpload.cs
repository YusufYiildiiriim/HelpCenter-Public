namespace HelpCenter.Application.Common.Models;

/// <summary>
/// Framework-agnostic representation of an uploaded file. The Application layer
/// does not know about ASP.NET's <c>IFormFile</c>; the presentation layer converts
/// the incoming request to this type (see WebApi/Binders/FileUploadModelBinder).
/// This keeps commands and handlers HTTP-agnostic and lets tests feed them an
/// in-memory stream.
/// </summary>
public sealed class FileUpload
{
    private readonly Func<Stream> _openReadStream;

    public FileUpload(string fileName, long length, string? contentType, Func<Stream> openReadStream)
    {
        FileName = fileName;
        Length = length;
        ContentType = contentType;
        _openReadStream = openReadStream;
    }

    public string FileName { get; }

    public long Length { get; }

    public string? ContentType { get; }

    /// <summary>Opens a new stream for reading the file content. The caller must dispose it.</summary>
    public Stream OpenReadStream() => _openReadStream();

    /// <summary>Copies the file content to the target stream.</summary>
    public async Task CopyToAsync(Stream target, CancellationToken cancellationToken = default)
    {
        await using var source = OpenReadStream();
        await source.CopyToAsync(target, cancellationToken);
    }

    /// <summary>Shortcut for tests and in-memory scenarios.</summary>
    public static FileUpload FromBytes(string fileName, byte[] content, string? contentType = null) =>
        new(fileName, content.LongLength, contentType, () => new MemoryStream(content, writable: false));
}

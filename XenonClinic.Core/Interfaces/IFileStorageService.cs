namespace XenonClinic.Core.Interfaces;

/// <summary>
/// File storage service abstraction for cloud/local file storage.
/// </summary>
public interface IFileStorageService
{
    /// <summary>
    /// Upload a file.
    /// </summary>
    Task<FileUploadResult> UploadAsync(Stream stream, string fileName, string contentType, string? folder = null);

    /// <summary>
    /// Upload a file with metadata.
    /// </summary>
    Task<FileUploadResult> UploadAsync(FileUploadRequest request);

    /// <summary>
    /// Download a file.
    /// </summary>
    Task<FileDownloadResult?> DownloadAsync(string fileId);

    /// <summary>
    /// Delete a file.
    /// </summary>
    Task<bool> DeleteAsync(string fileId);

    /// <summary>
    /// Get file metadata.
    /// </summary>
    Task<FileMetadata?> GetMetadataAsync(string fileId);

    /// <summary>
    /// Generate a temporary download URL.
    /// </summary>
    Task<string?> GetTemporaryUrlAsync(string fileId, TimeSpan expiration);

    /// <summary>
    /// Check if a file exists.
    /// </summary>
    Task<bool> ExistsAsync(string fileId);

    /// <summary>
    /// List files in a folder.
    /// </summary>
    Task<IEnumerable<FileMetadata>> ListAsync(string? folder = null, int? limit = null);

    /// <summary>
    /// Copy a file.
    /// </summary>
    Task<FileUploadResult?> CopyAsync(string sourceFileId, string? destinationFolder = null);

    /// <summary>
    /// Move a file.
    /// </summary>
    Task<bool> MoveAsync(string fileId, string destinationFolder);
}

/// <summary>
/// File upload request.
/// </summary>
public class FileUploadRequest
{
    public required Stream Stream { get; set; }
    public required string FileName { get; set; }
    public required string ContentType { get; set; }
    public string? Folder { get; set; }
    public Dictionary<string, string> Metadata { get; set; } = new();
    public bool IsPublic { get; set; }
    public long? MaxSizeBytes { get; set; }
    public string[]? AllowedExtensions { get; set; }
}

/// <summary>
/// File upload result.
/// </summary>
public record FileUploadResult(
    string FileId,
    string FileName,
    string Url,
    long SizeBytes,
    string ContentType,
    DateTime UploadedAt
);

/// <summary>
/// File download result.
/// </summary>
public record FileDownloadResult(
    Stream Stream,
    string FileName,
    string ContentType,
    long SizeBytes
);

/// <summary>
/// File metadata.
/// </summary>
public record FileMetadata(
    string FileId,
    string FileName,
    string ContentType,
    long SizeBytes,
    string? Folder,
    DateTime CreatedAt,
    DateTime? ModifiedAt,
    Dictionary<string, string> CustomMetadata,
    bool IsPublic
);

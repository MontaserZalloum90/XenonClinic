using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using XenonClinic.Core.Interfaces;

namespace XenonClinic.Infrastructure.Services;

/// <summary>
/// File storage service with support for local and cloud storage.
/// </summary>
public class FileStorageService : IFileStorageService
{
    private readonly ILogger<FileStorageService> _logger;
    private readonly FileStorageSettings _settings;

    public FileStorageService(
        ILogger<FileStorageService> logger,
        IOptions<FileStorageSettings> settings)
    {
        _logger = logger;
        _settings = settings.Value;

        EnsureDirectoryExists(_settings.BasePath);
    }

    public async Task<FileUploadResult> UploadAsync(Stream stream, string fileName, string contentType, string? folder = null)
    {
        return await UploadAsync(new FileUploadRequest
        {
            Stream = stream,
            FileName = fileName,
            ContentType = contentType,
            Folder = folder
        });
    }

    public async Task<FileUploadResult> UploadAsync(FileUploadRequest request)
    {
        ValidateUpload(request);

        var fileId = GenerateFileId();
        var extension = Path.GetExtension(request.FileName);
        var storedFileName = $"{fileId}{extension}";
        var folderPath = GetFolderPath(request.Folder);
        var filePath = Path.Combine(folderPath, storedFileName);

        EnsureDirectoryExists(folderPath);

        await using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
        await request.Stream.CopyToAsync(fileStream);

        var fileInfo = new FileInfo(filePath);

        // Store metadata
        await SaveMetadataAsync(fileId, new FileMetadata(
            fileId,
            request.FileName,
            request.ContentType,
            fileInfo.Length,
            request.Folder,
            DateTime.UtcNow,
            null,
            request.Metadata,
            request.IsPublic
        ));

        var url = GetFileUrl(fileId, request.IsPublic);

        _logger.LogInformation("File uploaded: {FileId} ({FileName}, {Size} bytes)",
            fileId, request.FileName, fileInfo.Length);

        return new FileUploadResult(
            fileId,
            request.FileName,
            url,
            fileInfo.Length,
            request.ContentType,
            DateTime.UtcNow
        );
    }

    public async Task<FileDownloadResult?> DownloadAsync(string fileId)
    {
        var metadata = await GetMetadataAsync(fileId);
        if (metadata == null) return null;

        var filePath = GetFilePath(fileId, metadata);
        if (!File.Exists(filePath)) return null;

        var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        return new FileDownloadResult(stream, metadata.FileName, metadata.ContentType, metadata.SizeBytes);
    }

    public async Task<bool> DeleteAsync(string fileId)
    {
        var metadata = await GetMetadataAsync(fileId);
        if (metadata == null) return false;

        var filePath = GetFilePath(fileId, metadata);
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        await DeleteMetadataAsync(fileId);
        _logger.LogInformation("File deleted: {FileId}", fileId);
        return true;
    }

    public Task<FileMetadata?> GetMetadataAsync(string fileId)
    {
        var metadataPath = GetMetadataPath(fileId);
        if (!File.Exists(metadataPath)) return Task.FromResult<FileMetadata?>(null);

        var json = File.ReadAllText(metadataPath);
        var metadata = System.Text.Json.JsonSerializer.Deserialize<FileMetadata>(json);
        return Task.FromResult(metadata);
    }

    public Task<string?> GetTemporaryUrlAsync(string fileId, TimeSpan expiration)
    {
        // For local storage, generate a signed URL
        // In production with cloud storage, use pre-signed URLs
        var token = GenerateSignedToken(fileId, expiration);
        var url = $"{_settings.BaseUrl}/files/{fileId}?token={token}&expires={DateTimeOffset.UtcNow.Add(expiration).ToUnixTimeSeconds()}";
        return Task.FromResult<string?>(url);
    }

    public async Task<bool> ExistsAsync(string fileId)
    {
        var metadata = await GetMetadataAsync(fileId);
        return metadata != null;
    }

    public Task<IEnumerable<FileMetadata>> ListAsync(string? folder = null, int? limit = null)
    {
        var metadataDir = Path.Combine(_settings.BasePath, ".metadata");
        if (!Directory.Exists(metadataDir))
            return Task.FromResult<IEnumerable<FileMetadata>>(Array.Empty<FileMetadata>());

        var files = Directory.GetFiles(metadataDir, "*.json")
            .Select(f =>
            {
                var json = File.ReadAllText(f);
                return System.Text.Json.JsonSerializer.Deserialize<FileMetadata>(json);
            })
            .Where(m => m != null && (folder == null || m.Folder == folder))
            .Cast<FileMetadata>()
            .OrderByDescending(m => m.CreatedAt);

        var result = limit.HasValue ? files.Take(limit.Value) : files;
        return Task.FromResult(result.AsEnumerable());
    }

    public async Task<FileUploadResult?> CopyAsync(string sourceFileId, string? destinationFolder = null)
    {
        var source = await DownloadAsync(sourceFileId);
        if (source == null) return null;

        var metadata = await GetMetadataAsync(sourceFileId);
        if (metadata == null) return null;

        return await UploadAsync(source.Stream, metadata.FileName, metadata.ContentType, destinationFolder);
    }

    public async Task<bool> MoveAsync(string fileId, string destinationFolder)
    {
        var metadata = await GetMetadataAsync(fileId);
        if (metadata == null) return false;

        var currentPath = GetFilePath(fileId, metadata);
        var newFolderPath = GetFolderPath(destinationFolder);
        EnsureDirectoryExists(newFolderPath);

        var newPath = Path.Combine(newFolderPath, Path.GetFileName(currentPath));
        File.Move(currentPath, newPath);

        // Update metadata
        var updatedMetadata = metadata with { Folder = destinationFolder, ModifiedAt = DateTime.UtcNow };
        await SaveMetadataAsync(fileId, updatedMetadata);

        return true;
    }

    #region Private Methods

    private static string GenerateFileId() => Guid.NewGuid().ToString("N");

    private string GetFolderPath(string? folder)
    {
        if (string.IsNullOrEmpty(folder))
            return _settings.BasePath;

        // Validate folder path to prevent path traversal attacks
        var fullPath = Path.GetFullPath(Path.Combine(_settings.BasePath, folder));
        var basePath = Path.GetFullPath(_settings.BasePath);

        // Ensure the resolved path is within the base path
        if (!fullPath.StartsWith(basePath, StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("Path traversal attempt detected: {Folder}", folder);
            throw new InvalidOperationException("Invalid folder path - path traversal not allowed");
        }

        return fullPath;
    }

    private string GetFilePath(string fileId, FileMetadata metadata)
    {
        var extension = Path.GetExtension(metadata.FileName);
        var storedFileName = $"{fileId}{extension}";
        return Path.Combine(GetFolderPath(metadata.Folder), storedFileName);
    }

    private string GetMetadataPath(string fileId)
    {
        var metadataDir = Path.Combine(_settings.BasePath, ".metadata");
        EnsureDirectoryExists(metadataDir);
        return Path.Combine(metadataDir, $"{fileId}.json");
    }

    private async Task SaveMetadataAsync(string fileId, FileMetadata metadata)
    {
        var path = GetMetadataPath(fileId);
        var json = System.Text.Json.JsonSerializer.Serialize(metadata);
        await File.WriteAllTextAsync(path, json);
    }

    private Task DeleteMetadataAsync(string fileId)
    {
        var path = GetMetadataPath(fileId);
        if (File.Exists(path)) File.Delete(path);
        return Task.CompletedTask;
    }

    private string GetFileUrl(string fileId, bool isPublic)
    {
        return isPublic
            ? $"{_settings.BaseUrl}/public/{fileId}"
            : $"{_settings.BaseUrl}/files/{fileId}";
    }

    private static void EnsureDirectoryExists(string path)
    {
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);
    }

    private void ValidateUpload(FileUploadRequest request)
    {
        if (request.MaxSizeBytes.HasValue && request.Stream.Length > request.MaxSizeBytes)
        {
            throw new InvalidOperationException($"File exceeds maximum size of {request.MaxSizeBytes} bytes");
        }

        if (request.AllowedExtensions?.Length > 0)
        {
            var extension = Path.GetExtension(request.FileName).ToLowerInvariant();
            if (!request.AllowedExtensions.Contains(extension))
            {
                throw new InvalidOperationException($"File extension {extension} is not allowed");
            }
        }
    }

    private static string GenerateSignedToken(string fileId, TimeSpan expiration)
    {
        // In production, use HMAC with a secret key
        var data = $"{fileId}:{DateTimeOffset.UtcNow.Add(expiration).ToUnixTimeSeconds()}";
        var bytes = System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(data));
        return Convert.ToBase64String(bytes)[..16];
    }

    #endregion
}

/// <summary>
/// File storage settings.
/// </summary>
public class FileStorageSettings
{
    public string BasePath { get; set; } = "./uploads";
    public string BaseUrl { get; set; } = "http://localhost:5000";
    public long MaxFileSizeBytes { get; set; } = 50 * 1024 * 1024; // 50MB
    public string[] AllowedExtensions { get; set; } = { ".jpg", ".jpeg", ".png", ".gif", ".pdf", ".doc", ".docx", ".xls", ".xlsx" };
}

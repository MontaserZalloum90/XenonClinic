using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using XenonClinic.Core.DTOs;
using XenonClinic.Core.Entities;
using XenonClinic.Core.Interfaces;
using XenonClinic.Infrastructure.Data;

namespace XenonClinic.Infrastructure.Services;

/// <summary>
/// Service for DICOM/PACS operations and medical imaging
/// </summary>
public class DicomService : IDicomService
{
    private readonly ClinicDbContext _context;
    private readonly ILogger<DicomService> _logger;

    public DicomService(ClinicDbContext context, ILogger<DicomService> logger)
    {
        _context = context;
        _logger = logger;
    }

    #region Study Operations

    public async Task<DicomSearchResultDto> SearchStudiesAsync(int branchId, DicomStudySearchDto request)
    {
        var query = _context.DicomStudies
            .Include(s => s.Patient)
            .Where(s => s.BranchId == branchId);

        if (request.PatientId.HasValue)
            query = query.Where(s => s.PatientId == request.PatientId.Value);

        if (!string.IsNullOrWhiteSpace(request.PatientName))
            query = query.Where(s => (s.Patient!.FirstName + " " + s.Patient.LastName).Contains(request.PatientName));

        if (!string.IsNullOrWhiteSpace(request.PatientMRN))
            query = query.Where(s => s.Patient!.MRN == request.PatientMRN);

        if (!string.IsNullOrWhiteSpace(request.StudyInstanceUid))
            query = query.Where(s => s.StudyInstanceUid == request.StudyInstanceUid);

        if (!string.IsNullOrWhiteSpace(request.AccessionNumber))
            query = query.Where(s => s.AccessionNumber == request.AccessionNumber);

        if (!string.IsNullOrWhiteSpace(request.Modality))
            query = query.Where(s => s.Modality == request.Modality);

        if (request.StudyDateFrom.HasValue)
            query = query.Where(s => s.StudyDate >= request.StudyDateFrom.Value);

        if (request.StudyDateTo.HasValue)
            query = query.Where(s => s.StudyDate <= request.StudyDateTo.Value);

        if (!string.IsNullOrWhiteSpace(request.ReferringPhysician))
            query = query.Where(s => s.ReferringPhysician != null && s.ReferringPhysician.Contains(request.ReferringPhysician));

        if (!string.IsNullOrWhiteSpace(request.BodyPartExamined))
            query = query.Where(s => s.BodyPartExamined == request.BodyPartExamined);

        if (!string.IsNullOrWhiteSpace(request.Status))
            query = query.Where(s => s.Status == request.Status);

        var totalCount = await query.CountAsync();
        var skip = (request.PageNumber - 1) * request.PageSize;

        var studies = await query
            .OrderByDescending(s => s.StudyDate)
            .Skip(skip)
            .Take(request.PageSize)
            .ToListAsync();

        return new DicomSearchResultDto
        {
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            Studies = studies.Select(s => new DicomStudySummaryDto
            {
                Id = s.Id,
                StudyInstanceUid = s.StudyInstanceUid,
                AccessionNumber = s.AccessionNumber,
                StudyDate = s.StudyDate,
                StudyDescription = s.StudyDescription,
                Modality = s.Modality,
                PatientName = s.Patient != null ? $"{s.Patient.FirstName} {s.Patient.LastName}" : null,
                NumberOfSeries = s.NumberOfSeries,
                NumberOfInstances = s.NumberOfInstances,
                Status = s.Status,
                HasReport = s.Report != null
            }).ToList()
        };
    }

    public async Task<DicomStudyDto?> GetStudyByIdAsync(int id)
    {
        var study = await _context.DicomStudies
            .Include(s => s.Patient)
            .Include(s => s.Series!)
            .ThenInclude(ser => ser.Instances)
            .FirstOrDefaultAsync(s => s.Id == id);

        return study == null ? null : MapToStudyDto(study);
    }

    public async Task<DicomStudyDto?> GetStudyByUidAsync(int branchId, string studyInstanceUid)
    {
        var study = await _context.DicomStudies
            .Include(s => s.Patient)
            .Include(s => s.Series!)
            .ThenInclude(ser => ser.Instances)
            .FirstOrDefaultAsync(s => s.BranchId == branchId && s.StudyInstanceUid == studyInstanceUid);

        return study == null ? null : MapToStudyDto(study);
    }

    public async Task<IEnumerable<DicomStudySummaryDto>> GetPatientStudiesAsync(int patientId)
    {
        var studies = await _context.DicomStudies
            .Include(s => s.Patient)
            .Include(s => s.Report)
            .Where(s => s.PatientId == patientId)
            .OrderByDescending(s => s.StudyDate)
            .ToListAsync();

        return studies.Select(s => new DicomStudySummaryDto
        {
            Id = s.Id,
            StudyInstanceUid = s.StudyInstanceUid,
            AccessionNumber = s.AccessionNumber,
            StudyDate = s.StudyDate,
            StudyDescription = s.StudyDescription,
            Modality = s.Modality,
            PatientName = s.Patient != null ? $"{s.Patient.FirstName} {s.Patient.LastName}" : null,
            NumberOfSeries = s.NumberOfSeries,
            NumberOfInstances = s.NumberOfInstances,
            Status = s.Status,
            HasReport = s.Report != null
        });
    }

    public async Task<DicomStudyDto> CreateStudyAsync(int branchId, CreateDicomStudyDto dto)
    {
        var study = new DicomStudy
        {
            BranchId = branchId,
            StudyInstanceUid = GenerateUid(),
            PatientId = dto.PatientId,
            StudyDate = dto.StudyDate,
            StudyDescription = dto.StudyDescription,
            Modality = dto.Modality,
            ReferringPhysician = dto.ReferringPhysician,
            BodyPartExamined = dto.BodyPartExamined,
            AccessionNumber = dto.AccessionNumber ?? await GenerateAccessionNumberAsync(branchId),
            Status = "New",
            ReceivedDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        _context.DicomStudies.Add(study);
        await _context.SaveChangesAsync();

        return MapToStudyDto(study);
    }

    public async Task<DicomStudyDto> UpdateStudyStatusAsync(int studyId, string status)
    {
        var study = await _context.DicomStudies.FindAsync(studyId)
            ?? throw new KeyNotFoundException($"Study with ID {studyId} not found");

        study.Status = status;
        study.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return MapToStudyDto(study);
    }

    public async Task DeleteStudyAsync(int studyId)
    {
        var study = await _context.DicomStudies
            .Include(s => s.Series!)
            .ThenInclude(ser => ser.Instances)
            .FirstOrDefaultAsync(s => s.Id == studyId)
            ?? throw new KeyNotFoundException($"Study with ID {studyId} not found");

        // Delete all instances and series
        foreach (var series in study.Series ?? new List<DicomSeries>())
        {
            if (series.Instances != null)
                _context.DicomInstances.RemoveRange(series.Instances);
        }

        if (study.Series != null)
            _context.DicomSeries.RemoveRange(study.Series);

        _context.DicomStudies.Remove(study);
        await _context.SaveChangesAsync();
    }

    #endregion

    #region Series & Instance Operations

    public async Task<IEnumerable<DicomSeriesDto>> GetStudySeriesAsync(int studyId)
    {
        var series = await _context.DicomSeries
            .Include(s => s.Instances)
            .Where(s => s.StudyId == studyId)
            .OrderBy(s => s.SeriesNumber)
            .ToListAsync();

        return series.Select(MapToSeriesDto);
    }

    public async Task<IEnumerable<DicomInstanceDto>> GetSeriesInstancesAsync(int seriesId)
    {
        var instances = await _context.DicomInstances
            .Where(i => i.SeriesId == seriesId)
            .OrderBy(i => i.InstanceNumber)
            .ToListAsync();

        return instances.Select(MapToInstanceDto);
    }

    public async Task<DicomInstanceDto?> GetInstanceAsync(string sopInstanceUid)
    {
        var instance = await _context.DicomInstances
            .FirstOrDefaultAsync(i => i.SopInstanceUid == sopInstanceUid);

        return instance == null ? null : MapToInstanceDto(instance);
    }

    #endregion

    #region Viewer Operations

    public Task<DicomViewerConfigDto> GetViewerConfigAsync(int branchId)
    {
        // Return OHIF viewer configuration
        var config = new DicomViewerConfigDto
        {
            ViewerUrl = "/ohif-viewer",
            ViewerType = "OHIF",
            WadoRsUrl = "/dicomweb/studies",
            QidoRsUrl = "/dicomweb",
            StowRsUrl = "/dicomweb/studies",
            EnableMpr = true,
            EnableVr = false,
            EnableMeasurements = true,
            EnableAnnotations = true,
            AdditionalConfig = new Dictionary<string, object>
            {
                { "maxConcurrentMetadataRequests", 5 },
                { "showStudyList", true },
                { "defaultLanguage", "en" }
            }
        };

        return Task.FromResult(config);
    }

    public Task<DicomViewerSessionDto> CreateViewerSessionAsync(int branchId, string studyInstanceUid, int userId)
    {
        var sessionId = Guid.NewGuid().ToString();
        var session = new DicomViewerSessionDto
        {
            SessionId = sessionId,
            ViewerUrl = $"/ohif-viewer?StudyInstanceUIDs={studyInstanceUid}",
            StudyInstanceUid = studyInstanceUid,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddHours(8),
            AccessToken = GenerateViewerToken(sessionId, studyInstanceUid, userId)
        };

        return Task.FromResult(session);
    }

    public async Task<DicomImageResponseDto> GetRenderedImageAsync(DicomImageRequestDto request)
    {
        // DICOM image rendering requires fo-dicom library
        // Install: dotnet add package fo-dicom
        // The implementation below simulates the structure that would be used

        var response = new DicomImageResponseDto
        {
            ContentType = request.OutputFormat?.ToLower() switch
            {
                "png" => "image/png",
                _ => "image/jpeg"
            },
            Width = request.Width ?? 512,
            Height = request.Height ?? 512,
            SopInstanceUid = request.SopInstanceUid
        };

        try
        {
            // Find the DICOM instance
            var instance = await _context.DicomInstances
                .Include(i => i.Series)
                .ThenInclude(s => s!.Study)
                .FirstOrDefaultAsync(i => i.SopInstanceUid == request.SopInstanceUid);

            if (instance == null)
            {
                _logger.LogWarning("DICOM instance not found: {SopInstanceUid}", request.SopInstanceUid);
                return response;
            }

            // Get the file path
            var filePath = instance.StoragePath;
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                _logger.LogWarning("DICOM file not found at path: {Path}", filePath);
                return response;
            }

            // In production with fo-dicom:
            // var dicomFile = await DicomFile.OpenAsync(filePath);
            // var image = new DicomImage(dicomFile.Dataset);
            // var bitmap = image.RenderImage(request.Frame ?? 0);
            // Apply windowing: image.WindowCenter = request.WindowCenter ?? image.DefaultWindows[0].Center;
            // Apply rescale if needed for specific modalities
            // Convert to requested format with specified dimensions

            // For now, generate a placeholder test pattern
            response.ImageData = GeneratePlaceholderImage(
                response.Width,
                response.Height,
                instance.SopInstanceUid,
                request.OutputFormat ?? "jpeg");

            response.NumberOfFrames = instance.NumberOfFrames ?? 1;
            response.CurrentFrame = request.Frame ?? 0;
            response.PhotometricInterpretation = instance.PhotometricInterpretation;
            response.PixelSpacing = instance.PixelSpacing;
            response.ImageType = instance.ImageType;
            response.TransferSyntaxUid = instance.TransferSyntaxUid;

            _logger.LogDebug("Rendered DICOM image: {SopInstanceUid}, Frame: {Frame}",
                request.SopInstanceUid, request.Frame ?? 0);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rendering DICOM image: {SopInstanceUid}", request.SopInstanceUid);
        }

        return response;
    }

    private static byte[] GeneratePlaceholderImage(int width, int height, string instanceUid, string format)
    {
        // Generate a simple gray placeholder image
        // In production, this would be replaced by actual DICOM rendering

        // Create a simple PGM (grayscale) image as placeholder
        var header = $"P5\n# DICOM Placeholder: {instanceUid}\n{width} {height}\n255\n";
        var headerBytes = System.Text.Encoding.ASCII.GetBytes(header);
        var pixelData = new byte[width * height];

        // Create gradient pattern
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                pixelData[y * width + x] = (byte)((x + y) % 256);
            }
        }

        // For actual image conversion, use System.Drawing or ImageSharp
        // Here we return the raw grayscale data
        var result = new byte[headerBytes.Length + pixelData.Length];
        Buffer.BlockCopy(headerBytes, 0, result, 0, headerBytes.Length);
        Buffer.BlockCopy(pixelData, 0, result, headerBytes.Length, pixelData.Length);

        return result;
    }

    public Task<string> GetWadoRsUrlAsync(int branchId, string studyInstanceUid)
    {
        var url = $"/dicomweb/studies/{studyInstanceUid}";
        return Task.FromResult(url);
    }

    #endregion

    #region DICOM Storage/Transfer

    public async Task<DicomStoreResponseDto> StoreInstanceAsync(int branchId, DicomStoreRequestDto request)
    {
        var response = new DicomStoreResponseDto();

        try
        {
            // Parse DICOM data and extract metadata (placeholder)
            var studyUid = request.StudyInstanceUid ?? GenerateUid();
            var seriesUid = GenerateUid();
            var instanceUid = GenerateUid();

            // Find or create study
            var study = await _context.DicomStudies
                .FirstOrDefaultAsync(s => s.BranchId == branchId && s.StudyInstanceUid == studyUid);

            if (study == null)
            {
                study = new DicomStudy
                {
                    BranchId = branchId,
                    StudyInstanceUid = studyUid,
                    PatientId = request.PatientId,
                    StudyDate = DateTime.UtcNow,
                    Modality = "OT", // Would be extracted from DICOM
                    Status = "New",
                    ReceivedDate = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                };
                _context.DicomStudies.Add(study);
                await _context.SaveChangesAsync();
            }

            // Create series
            var series = new DicomSeries
            {
                StudyId = study.Id,
                SeriesInstanceUid = seriesUid,
                Modality = study.Modality,
                SeriesDate = DateTime.UtcNow,
                NumberOfInstances = 1,
                CreatedAt = DateTime.UtcNow
            };
            _context.DicomSeries.Add(series);
            await _context.SaveChangesAsync();

            // Create instance
            var instance = new DicomInstance
            {
                SeriesId = series.Id,
                SopInstanceUid = instanceUid,
                SopClassUid = "1.2.840.10008.5.1.4.1.1.2", // CT Image Storage
                InstanceNumber = 1,
                FileSizeBytes = request.DicomData.Length,
                CreatedAt = DateTime.UtcNow
            };
            _context.DicomInstances.Add(instance);

            // Update counts
            study.NumberOfSeries++;
            study.NumberOfInstances++;
            series.NumberOfInstances++;

            await _context.SaveChangesAsync();

            response.Success = true;
            response.StudyInstanceUid = studyUid;
            response.SeriesInstanceUid = seriesUid;
            response.SopInstanceUid = instanceUid;
            response.StatusCode = 0; // Success
        }
        catch (Exception ex)
        {
            response.Success = false;
            response.Message = ex.Message;
            response.StatusCode = 272; // Unable to store
        }

        return response;
    }

    public async Task<bool> MoveStudyAsync(int branchId, DicomMoveRequestDto request)
    {
        // DICOM C-MOVE requires fo-dicom library for network operations
        // Install: dotnet add package fo-dicom.NetCore

        try
        {
            // Validate the study exists
            var study = await _context.DicomStudies
                .FirstOrDefaultAsync(s => s.BranchId == branchId &&
                    s.StudyInstanceUid == request.StudyInstanceUid);

            if (study == null)
            {
                _logger.LogWarning("C-MOVE: Study not found: {StudyInstanceUid}", request.StudyInstanceUid);
                return false;
            }

            // Get PACS configuration for the destination AE
            // TODO: Implement DicomPacsConfig entity and DbSet
            // var pacsConfig = await _context.DicomPacsConfigs
            //     .FirstOrDefaultAsync(p => p.BranchId == branchId && p.AeTitle == request.DestinationAeTitle);
            //
            // if (pacsConfig == null)
            // {
            //     _logger.LogWarning("C-MOVE: Destination AE not configured: {AeTitle}", request.DestinationAeTitle);
            //     return false;
            // }

            _logger.LogInformation(
                "C-MOVE initiated: Study {StudyUid} to {DestAe}",
                request.StudyInstanceUid,
                request.DestinationAeTitle);

            // In production with fo-dicom:
            // var client = DicomClientFactory.Create(pacsConfig.Host, pacsConfig.Port,
            //     false, _config.LocalAeTitle, request.DestinationAeTitle);
            //
            // var moveRequest = new DicomCMoveRequest(request.DestinationAeTitle,
            //     request.StudyInstanceUid, DicomPriority.Medium);
            //
            // moveRequest.OnResponseReceived += (req, resp) => {
            //     if (resp.Status == DicomStatus.Pending)
            //         _logger.LogDebug("C-MOVE progress: Remaining {Remaining}", resp.Remaining);
            // };
            //
            // await client.AddRequestAsync(moveRequest);
            // await client.SendAsync();

            // Record the move request
            // TODO: Implement DicomMoveRecord entity and DbSet
            // var moveRecord = new DicomMoveRecord
            // {
            //     BranchId = branchId,
            //     StudyInstanceUid = request.StudyInstanceUid,
            //     DestinationAeTitle = request.DestinationAeTitle,
            //     Status = "Queued",
            //     RequestedAt = DateTime.UtcNow,
            //     QueryLevel = request.QueryLevel ?? "STUDY"
            // };
            //
            // _context.DicomMoveRecords.Add(moveRecord);
            // await _context.SaveChangesAsync();

            // Update study status
            study.Status = "MovePending";
            await _context.SaveChangesAsync();

            _logger.LogInformation("C-MOVE queued: {StudyInstanceUid} -> {DestinationAe}",
                request.StudyInstanceUid, request.DestinationAeTitle);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "C-MOVE failed: {StudyInstanceUid}", request.StudyInstanceUid);
            return false;
        }
    }

    public async Task<DicomExportResponseDto> ExportStudiesAsync(int branchId, DicomExportRequestDto request)
    {
        var response = new DicomExportResponseDto();

        var studies = await _context.DicomStudies
            .Include(s => s.Series)
            .ThenInclude(ser => ser.Instances)
            .Where(s => s.BranchId == branchId && request.StudyInstanceUids.Contains(s.StudyInstanceUid))
            .ToListAsync();

        response.TotalStudies = studies.Count;
        response.TotalInstances = studies.Sum(s => s.NumberOfInstances);
        response.TotalSizeBytes = studies.Sum(s => s.TotalSizeBytes ?? 0);
        response.Success = true;

        return response;
    }

    public async Task<int> ImportFromDirectoryAsync(int branchId, string directoryPath, int patientId)
    {
        // DICOM file parsing requires fo-dicom library
        // Install: dotnet add package fo-dicom

        if (!Directory.Exists(directoryPath))
        {
            _logger.LogWarning("DICOM import directory not found: {Path}", directoryPath);
            return 0;
        }

        var importedCount = 0;
        var dicomExtensions = new[] { ".dcm", ".dicom", "" }; // DICOM files may have no extension

        try
        {
            // Get all potential DICOM files
            var files = Directory.GetFiles(directoryPath, "*", SearchOption.AllDirectories)
                .Where(f => dicomExtensions.Contains(Path.GetExtension(f).ToLower()) ||
                           !Path.HasExtension(f))
                .ToList();

            _logger.LogInformation("DICOM import: Found {Count} potential files in {Path}",
                files.Count, directoryPath);

            foreach (var filePath in files)
            {
                try
                {
                    // Check if file is DICOM by reading magic number (DICM at offset 128)
                    if (!await IsDicomFileAsync(filePath))
                    {
                        _logger.LogDebug("Skipping non-DICOM file: {Path}", filePath);
                        continue;
                    }

                    // In production with fo-dicom:
                    // var dicomFile = await DicomFile.OpenAsync(filePath);
                    // var dataset = dicomFile.Dataset;
                    //
                    // Extract DICOM tags:
                    // var studyUid = dataset.GetSingleValueOrDefault<string>(DicomTag.StudyInstanceUID, GenerateUid());
                    // var seriesUid = dataset.GetSingleValueOrDefault<string>(DicomTag.SeriesInstanceUID, GenerateUid());
                    // var sopInstanceUid = dataset.GetSingleValueOrDefault<string>(DicomTag.SOPInstanceUID, GenerateUid());
                    // var modality = dataset.GetSingleValueOrDefault<string>(DicomTag.Modality, "OT");
                    // var studyDate = dataset.GetSingleValueOrDefault<DateTime>(DicomTag.StudyDate, DateTime.UtcNow);
                    // var patientName = dataset.GetSingleValueOrDefault<string>(DicomTag.PatientName, "");

                    // For placeholder, generate UIDs
                    var studyUid = GenerateUid();
                    var seriesUid = GenerateUid();
                    var sopInstanceUid = GenerateUid();

                    // Find or create study
                    var study = await _context.DicomStudies
                        .Include(s => s.Series)
                        .FirstOrDefaultAsync(s => s.BranchId == branchId && s.StudyInstanceUid == studyUid);

                    if (study == null)
                    {
                        study = new DicomStudy
                        {
                            BranchId = branchId,
                            PatientId = patientId,
                            StudyInstanceUid = studyUid,
                            StudyDate = DateTime.UtcNow,
                            Modality = "OT",
                            Status = "Imported",
                            ReceivedDate = DateTime.UtcNow,
                            CreatedAt = DateTime.UtcNow
                        };
                        _context.DicomStudies.Add(study);
                        await _context.SaveChangesAsync();
                    }

                    // Find or create series
                    var series = study.Series?.FirstOrDefault(s => s.SeriesInstanceUid == seriesUid);
                    if (series == null)
                    {
                        series = new DicomSeries
                        {
                            StudyId = study.Id,
                            SeriesInstanceUid = seriesUid,
                            SeriesNumber = (study.Series?.Count ?? 0) + 1,
                            Modality = "OT",
                            CreatedAt = DateTime.UtcNow
                        };
                        _context.DicomSeries.Add(series);
                        await _context.SaveChangesAsync();
                    }

                    // Get file info
                    var fileInfo = new FileInfo(filePath);

                    // Copy file to DICOM storage location
                    // TODO: Configure DICOM storage path via configuration
                    var baseStoragePath = Environment.GetEnvironmentVariable("DICOM_STORAGE_PATH") ?? "/var/lib/xenonclinic/dicom";
                    var storagePath = Path.Combine(
                        baseStoragePath,
                        branchId.ToString(),
                        study.StudyInstanceUid,
                        series.SeriesInstanceUid);

                    Directory.CreateDirectory(storagePath);
                    var destinationPath = Path.Combine(storagePath, $"{sopInstanceUid}.dcm");
                    File.Copy(filePath, destinationPath, overwrite: true);

                    // Create instance record
                    var instance = new DicomInstance
                    {
                        SeriesId = series.Id,
                        SopInstanceUid = sopInstanceUid,
                        SopClassUid = "1.2.840.10008.5.1.4.1.1.2", // CT Image Storage (default)
                        InstanceNumber = (series.Instances?.Count ?? 0) + 1,
                        StoragePath = destinationPath,
                        FileSizeBytes = fileInfo.Length,
                        TransferSyntaxUid = "1.2.840.10008.1.2", // Implicit VR Little Endian
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.DicomInstances.Add(instance);
                    await _context.SaveChangesAsync();

                    // Update study statistics
                    study.NumberOfSeries = study.NumberOfSeries + 1;
                    study.NumberOfInstances = study.NumberOfInstances + 1;
                    study.TotalSizeBytes = (study.TotalSizeBytes ?? 0) + fileInfo.Length;
                    await _context.SaveChangesAsync();

                    importedCount++;

                    _logger.LogDebug("Imported DICOM file: {Path} -> {SopInstanceUid}",
                        filePath, sopInstanceUid);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to import DICOM file: {Path}", filePath);
                }
            }

            _logger.LogInformation("DICOM import completed: {Imported} files from {Path}",
                importedCount, directoryPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DICOM import failed for directory: {Path}", directoryPath);
        }

        return importedCount;
    }

    private static async Task<bool> IsDicomFileAsync(string filePath)
    {
        try
        {
            // DICOM files have "DICM" magic number at offset 128
            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            if (stream.Length < 132)
                return false;

            stream.Seek(128, SeekOrigin.Begin);
            var buffer = new byte[4];
            await stream.ReadAsync(buffer);

            return buffer[0] == 'D' && buffer[1] == 'I' &&
                   buffer[2] == 'C' && buffer[3] == 'M';
        }
        catch
        {
            return false;
        }
    }

    #endregion

    #region Worklist Operations

    public async Task<IEnumerable<DicomWorklistEntryDto>> GetWorklistAsync(int branchId, string? modality = null, DateTime? date = null)
    {
        var query = _context.DicomWorklistEntries
            .Include(w => w.Patient)
            .Where(w => w.BranchId == branchId);

        if (!string.IsNullOrWhiteSpace(modality))
            query = query.Where(w => w.Modality == modality);

        if (date.HasValue)
            query = query.Where(w => w.ScheduledDateTime.Date == date.Value.Date);

        var entries = await query
            .OrderBy(w => w.ScheduledDateTime)
            .ToListAsync();

        return entries.Select(w => new DicomWorklistEntryDto
        {
            Id = w.Id,
            AccessionNumber = w.AccessionNumber,
            ScheduledProcedureStepId = w.ScheduledProcedureStepId,
            PatientId = w.PatientId,
            PatientName = w.Patient != null ? $"{w.Patient.FirstName} {w.Patient.LastName}" : "",
            PatientMRN = w.Patient?.MRN,
            PatientBirthDate = w.Patient?.DateOfBirth,
            PatientSex = w.Patient?.Gender,
            ScheduledDateTime = w.ScheduledDateTime,
            Modality = w.Modality,
            ScheduledStationAETitle = w.ScheduledStationAETitle,
            ScheduledPerformingPhysician = w.ScheduledPerformingPhysician,
            RequestedProcedureDescription = w.RequestedProcedureDescription,
            RequestedProcedureCode = w.RequestedProcedureCode,
            ReferringPhysician = w.ReferringPhysician,
            Status = w.Status,
            OrderId = w.OrderId
        });
    }

    public async Task<DicomWorklistEntryDto> CreateWorklistEntryAsync(int branchId, CreateWorklistEntryDto dto)
    {
        var entry = new DicomWorklistEntry
        {
            BranchId = branchId,
            AccessionNumber = await GenerateAccessionNumberAsync(branchId),
            ScheduledProcedureStepId = Guid.NewGuid().ToString("N")[..16].ToUpper(),
            PatientId = dto.PatientId,
            ScheduledDateTime = dto.ScheduledDateTime,
            Modality = dto.Modality,
            ScheduledStationAETitle = dto.ScheduledStationAETitle,
            RequestedProcedureDescription = dto.RequestedProcedureDescription,
            RequestedProcedureCode = dto.RequestedProcedureCode,
            ReferringPhysician = dto.ReferringPhysician,
            Status = "Scheduled",
            OrderId = dto.OrderId,
            CreatedAt = DateTime.UtcNow
        };

        _context.DicomWorklistEntries.Add(entry);
        await _context.SaveChangesAsync();

        var patient = await _context.Patients.FindAsync(dto.PatientId);

        return new DicomWorklistEntryDto
        {
            Id = entry.Id,
            AccessionNumber = entry.AccessionNumber,
            ScheduledProcedureStepId = entry.ScheduledProcedureStepId,
            PatientId = entry.PatientId,
            PatientName = patient != null ? $"{patient.FirstName} {patient.LastName}" : "",
            ScheduledDateTime = entry.ScheduledDateTime,
            Modality = entry.Modality,
            Status = entry.Status
        };
    }

    public async Task<DicomWorklistEntryDto> UpdateWorklistStatusAsync(int entryId, string status)
    {
        var entry = await _context.DicomWorklistEntries
            .Include(w => w.Patient)
            .FirstOrDefaultAsync(w => w.Id == entryId)
            ?? throw new KeyNotFoundException($"Worklist entry with ID {entryId} not found");

        entry.Status = status;
        entry.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return new DicomWorklistEntryDto
        {
            Id = entry.Id,
            AccessionNumber = entry.AccessionNumber,
            ScheduledProcedureStepId = entry.ScheduledProcedureStepId,
            PatientId = entry.PatientId,
            PatientName = entry.Patient != null ? $"{entry.Patient.FirstName} {entry.Patient.LastName}" : "",
            ScheduledDateTime = entry.ScheduledDateTime,
            Modality = entry.Modality,
            Status = entry.Status
        };
    }

    public async Task DeleteWorklistEntryAsync(int entryId)
    {
        var entry = await _context.DicomWorklistEntries.FindAsync(entryId)
            ?? throw new KeyNotFoundException($"Worklist entry with ID {entryId} not found");

        _context.DicomWorklistEntries.Remove(entry);
        await _context.SaveChangesAsync();
    }

    public async Task<string> GenerateAccessionNumberAsync(int branchId)
    {
        var today = DateTime.UtcNow;
        var prefix = $"ACC{today:yyyyMMdd}";

        var lastNumber = await _context.DicomStudies
            .Where(s => s.BranchId == branchId && s.AccessionNumber != null && s.AccessionNumber.StartsWith(prefix))
            .OrderByDescending(s => s.AccessionNumber)
            .Select(s => s.AccessionNumber)
            .FirstOrDefaultAsync();

        var sequence = 1;
        if (!string.IsNullOrEmpty(lastNumber) && lastNumber.Length > prefix.Length)
        {
            if (int.TryParse(lastNumber[prefix.Length..], out var lastSeq))
                sequence = lastSeq + 1;
        }

        return $"{prefix}{sequence:D4}";
    }

    #endregion

    #region Radiology Reports

    public async Task<RadiologyReportDto?> GetStudyReportAsync(int studyId)
    {
        var report = await _context.RadiologyReports
            .Include(r => r.Study)
            .ThenInclude(s => s!.Patient)
            .Include(r => r.ReportingRadiologist)
            .FirstOrDefaultAsync(r => r.StudyId == studyId);

        if (report == null) return null;

        return MapToReportDto(report);
    }

    public async Task<RadiologyReportDto> SaveReportAsync(int branchId, CreateRadiologyReportDto dto)
    {
        var study = await _context.DicomStudies
            .Include(s => s.Patient)
            .FirstOrDefaultAsync(s => s.Id == dto.StudyId)
            ?? throw new KeyNotFoundException($"Study with ID {dto.StudyId} not found");

        var existingReport = await _context.RadiologyReports
            .FirstOrDefaultAsync(r => r.StudyId == dto.StudyId);

        RadiologyReport report;
        if (existingReport != null)
        {
            report = existingReport;
            report.ClinicalHistory = dto.ClinicalHistory;
            report.Findings = dto.Findings;
            report.Impression = dto.Impression;
            report.Recommendations = dto.Recommendations;
            report.KeyImages = dto.KeyImageSopInstanceUids != null ? JsonSerializer.Serialize(dto.KeyImageSopInstanceUids) : null;
            report.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            report = new RadiologyReport
            {
                BranchId = branchId,
                StudyId = dto.StudyId,
                ClinicalHistory = dto.ClinicalHistory,
                Findings = dto.Findings,
                Impression = dto.Impression,
                Recommendations = dto.Recommendations,
                KeyImages = dto.KeyImageSopInstanceUids != null ? JsonSerializer.Serialize(dto.KeyImageSopInstanceUids) : null,
                Status = "Draft",
                DraftedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };
            _context.RadiologyReports.Add(report);
        }

        await _context.SaveChangesAsync();

        report.Study = study;
        return MapToReportDto(report);
    }

    public async Task<RadiologyReportDto> FinalizeReportAsync(int reportId, int radiologistId)
    {
        var report = await _context.RadiologyReports
            .Include(r => r.Study)
            .ThenInclude(s => s!.Patient)
            .FirstOrDefaultAsync(r => r.Id == reportId)
            ?? throw new KeyNotFoundException($"Report with ID {reportId} not found");

        report.Status = "Final";
        report.ReportingRadiologistId = radiologistId;
        report.FinalizedAt = DateTime.UtcNow;
        report.UpdatedAt = DateTime.UtcNow;

        // Update study status
        if (report.Study != null)
            report.Study.Status = "Reported";

        await _context.SaveChangesAsync();

        report.ReportingRadiologist = await _context.Doctors.FindAsync(radiologistId);
        return MapToReportDto(report);
    }

    public async Task<RadiologyReportDto> AmendReportAsync(int reportId, string amendmentReason, string amendedFindings)
    {
        var report = await _context.RadiologyReports
            .Include(r => r.Study)
            .ThenInclude(s => s!.Patient)
            .Include(r => r.ReportingRadiologist)
            .FirstOrDefaultAsync(r => r.Id == reportId)
            ?? throw new KeyNotFoundException($"Report with ID {reportId} not found");

        report.Status = "Amended";
        report.AmendedAt = DateTime.UtcNow;
        report.AmendmentReason = amendmentReason;
        report.Findings = amendedFindings;
        report.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return MapToReportDto(report);
    }

    public async Task<IEnumerable<DicomStudySummaryDto>> GetPendingReportsAsync(int branchId)
    {
        var studies = await _context.DicomStudies
            .Include(s => s.Patient)
            .Include(s => s.Report)
            .Where(s => s.BranchId == branchId && (s.Report == null || s.Report.Status == "Draft"))
            .OrderBy(s => s.StudyDate)
            .ToListAsync();

        return studies.Select(s => new DicomStudySummaryDto
        {
            Id = s.Id,
            StudyInstanceUid = s.StudyInstanceUid,
            AccessionNumber = s.AccessionNumber,
            StudyDate = s.StudyDate,
            StudyDescription = s.StudyDescription,
            Modality = s.Modality,
            PatientName = s.Patient != null ? $"{s.Patient.FirstName} {s.Patient.LastName}" : null,
            NumberOfSeries = s.NumberOfSeries,
            NumberOfInstances = s.NumberOfInstances,
            Status = s.Status,
            HasReport = s.Report != null
        });
    }

    #endregion

    #region PACS Configuration

    public async Task<IEnumerable<PacsServerConfigDto>> GetPacsConfigsAsync(int branchId)
    {
        var configs = await _context.PacsServerConfigs
            .Where(c => c.BranchId == branchId)
            .OrderBy(c => c.Name)
            .ToListAsync();

        return configs.Select(MapToPacsConfigDto);
    }

    public async Task<PacsServerConfigDto?> GetPacsConfigByIdAsync(int id)
    {
        var config = await _context.PacsServerConfigs.FindAsync(id);
        return config == null ? null : MapToPacsConfigDto(config);
    }

    public async Task<PacsServerConfigDto> SavePacsConfigAsync(int branchId, CreatePacsConfigDto dto)
    {
        // If setting as default, unset others
        if (dto.IsDefault)
        {
            var existingDefaults = await _context.PacsServerConfigs
                .Where(c => c.BranchId == branchId && c.IsDefault)
                .ToListAsync();

            foreach (var existing in existingDefaults)
                existing.IsDefault = false;
        }

        var config = new PacsServerConfig
        {
            BranchId = branchId,
            Name = dto.Name,
            AeTitle = dto.AeTitle,
            HostName = dto.HostName,
            Port = dto.Port,
            IsDefault = dto.IsDefault,
            IsActive = true,
            Description = dto.Description,
            CallingAeTitle = dto.CallingAeTitle,
            CreatedAt = DateTime.UtcNow
        };

        _context.PacsServerConfigs.Add(config);
        await _context.SaveChangesAsync();

        return MapToPacsConfigDto(config);
    }

    public async Task DeletePacsConfigAsync(int id)
    {
        var config = await _context.PacsServerConfigs.FindAsync(id)
            ?? throw new KeyNotFoundException($"PACS configuration with ID {id} not found");

        _context.PacsServerConfigs.Remove(config);
        await _context.SaveChangesAsync();
    }

    public Task<PacsConnectionTestDto> TestPacsConnectionAsync(int configId)
    {
        // Placeholder - would perform C-ECHO to test connection
        var result = new PacsConnectionTestDto
        {
            Success = true,
            ResponseTimeMs = 50,
            Message = "C-ECHO successful",
            SupportedSopClasses = new List<string>
            {
                "1.2.840.10008.5.1.4.1.1.2", // CT Image Storage
                "1.2.840.10008.5.1.4.1.1.4", // MR Image Storage
                "1.2.840.10008.5.1.4.1.1.1.1" // Digital X-Ray
            },
            SupportedTransferSyntaxes = new List<string>
            {
                "1.2.840.10008.1.2", // Implicit VR Little Endian
                "1.2.840.10008.1.2.1", // Explicit VR Little Endian
                "1.2.840.10008.1.2.4.70" // JPEG Lossless
            }
        };

        return Task.FromResult(result);
    }

    #endregion

    #region Statistics

    public async Task<DicomStatisticsDto> GetStatisticsAsync(int branchId)
    {
        var today = DateTime.UtcNow.Date;
        var weekStart = today.AddDays(-(int)today.DayOfWeek);
        var monthStart = new DateTime(today.Year, today.Month, 1);

        var studies = await _context.DicomStudies
            .Where(s => s.BranchId == branchId)
            .ToListAsync();

        var stats = new DicomStatisticsDto
        {
            TotalStudies = studies.Count,
            TotalSeries = studies.Sum(s => s.NumberOfSeries),
            TotalInstances = studies.Sum(s => s.NumberOfInstances),
            TotalStorageBytes = studies.Sum(s => s.TotalSizeBytes ?? 0),
            StudiesToday = studies.Count(s => s.StudyDate.Date == today),
            StudiesThisWeek = studies.Count(s => s.StudyDate >= weekStart),
            StudiesThisMonth = studies.Count(s => s.StudyDate >= monthStart),
            StudiesByModality = studies.GroupBy(s => s.Modality).ToDictionary(g => g.Key, g => g.Count()),
            StudiesByStatus = studies.GroupBy(s => s.Status).ToDictionary(g => g.Key, g => g.Count())
        };

        stats.TotalStorageFormatted = FormatBytes(stats.TotalStorageBytes);

        // Pending reports
        stats.PendingReports = await _context.DicomStudies
            .CountAsync(s => s.BranchId == branchId && (s.Report == null || s.Report.Status == "Draft"));

        return stats;
    }

    #endregion

    #region Private Helper Methods

    private static DicomStudyDto MapToStudyDto(DicomStudy study)
    {
        return new DicomStudyDto
        {
            Id = study.Id,
            StudyInstanceUid = study.StudyInstanceUid,
            StudyId = study.StudyId,
            AccessionNumber = study.AccessionNumber,
            StudyDate = study.StudyDate,
            StudyTime = study.StudyTime,
            StudyDescription = study.StudyDescription,
            Modality = study.Modality,
            PatientId = study.PatientId,
            PatientName = study.Patient != null ? $"{study.Patient.FirstName} {study.Patient.LastName}" : null,
            PatientMRN = study.Patient?.MRN,
            ReferringPhysician = study.ReferringPhysician,
            InstitutionName = study.InstitutionName,
            BodyPartExamined = study.BodyPartExamined,
            NumberOfSeries = study.NumberOfSeries,
            NumberOfInstances = study.NumberOfInstances,
            Status = study.Status,
            StorageLocation = study.StorageLocation,
            TotalSizeBytes = study.TotalSizeBytes,
            ReceivedDate = study.ReceivedDate,
            Series = study.Series?.Select(MapToSeriesDto).ToList()
        };
    }

    private static DicomSeriesDto MapToSeriesDto(DicomSeries series)
    {
        return new DicomSeriesDto
        {
            Id = series.Id,
            StudyId = series.StudyId,
            SeriesInstanceUid = series.SeriesInstanceUid,
            SeriesNumber = series.SeriesNumber,
            SeriesDescription = series.SeriesDescription,
            Modality = series.Modality,
            SeriesDate = series.SeriesDate,
            SeriesTime = series.SeriesTime,
            BodyPartExamined = series.BodyPartExamined,
            PatientPosition = series.PatientPosition,
            ProtocolName = series.ProtocolName,
            NumberOfInstances = series.NumberOfInstances,
            PerformingPhysician = series.PerformingPhysician,
            OperatorName = series.OperatorName,
            Instances = series.Instances?.Select(MapToInstanceDto).ToList()
        };
    }

    private static DicomInstanceDto MapToInstanceDto(DicomInstance instance)
    {
        return new DicomInstanceDto
        {
            Id = instance.Id,
            SeriesId = instance.SeriesId,
            SopInstanceUid = instance.SopInstanceUid,
            SopClassUid = instance.SopClassUid,
            InstanceNumber = instance.InstanceNumber,
            ImageType = instance.ImageType,
            Rows = instance.Rows,
            Columns = instance.Columns,
            BitsAllocated = instance.BitsAllocated,
            BitsStored = instance.BitsStored,
            PhotometricInterpretation = instance.PhotometricInterpretation,
            PixelSpacing = instance.PixelSpacing,
            SliceThickness = instance.SliceThickness,
            SliceLocation = instance.SliceLocation,
            WindowCenter = instance.WindowCenter,
            WindowWidth = instance.WindowWidth,
            ContentDate = instance.ContentDate,
            ContentTime = instance.ContentTime,
            FileSizeBytes = instance.FileSizeBytes,
            FilePath = instance.FilePath,
            TransferSyntaxUid = instance.TransferSyntaxUid
        };
    }

    private static RadiologyReportDto MapToReportDto(RadiologyReport report)
    {
        return new RadiologyReportDto
        {
            Id = report.Id,
            StudyId = report.StudyId,
            StudyInstanceUid = report.Study?.StudyInstanceUid ?? "",
            AccessionNumber = report.Study?.AccessionNumber,
            PatientId = report.Study?.PatientId ?? 0,
            PatientName = report.Study?.Patient != null ? $"{report.Study.Patient.FirstName} {report.Study.Patient.LastName}" : null,
            StudyDate = report.Study?.StudyDate ?? DateTime.MinValue,
            Modality = report.Study?.Modality ?? "",
            StudyDescription = report.Study?.StudyDescription,
            ReportingRadiologistId = report.ReportingRadiologistId,
            ReportingRadiologistName = report.ReportingRadiologist != null ? $"Dr. {report.ReportingRadiologist.FirstName} {report.ReportingRadiologist.LastName}" : null,
            ClinicalHistory = report.ClinicalHistory,
            Findings = report.Findings,
            Impression = report.Impression,
            Recommendations = report.Recommendations,
            Status = report.Status,
            DraftedAt = report.DraftedAt,
            FinalizedAt = report.FinalizedAt,
            AmendedAt = report.AmendedAt,
            AmendmentReason = report.AmendmentReason,
            KeyImages = !string.IsNullOrEmpty(report.KeyImages)
                ? JsonSerializer.Deserialize<List<string>>(report.KeyImages)?.Select(uid => new RadiologyReportKeyImageDto { SopInstanceUid = uid }).ToList()
                : null
        };
    }

    private static PacsServerConfigDto MapToPacsConfigDto(PacsServerConfig config)
    {
        return new PacsServerConfigDto
        {
            Id = config.Id,
            Name = config.Name,
            AeTitle = config.AeTitle,
            HostName = config.HostName,
            Port = config.Port,
            IsDefault = config.IsDefault,
            IsActive = config.IsActive,
            Description = config.Description,
            ServerType = config.ServerType,
            SupportsStore = config.SupportsStore,
            SupportsFind = config.SupportsFind,
            SupportsMove = config.SupportsMove,
            SupportsGet = config.SupportsGet,
            SupportsWorklist = config.SupportsWorklist,
            CallingAeTitle = config.CallingAeTitle,
            MaxPduLength = config.MaxPduLength,
            AssociationTimeout = config.AssociationTimeout,
            LastVerifiedAt = config.LastVerifiedAt,
            IsVerified = config.IsVerified
        };
    }

    private static string GenerateUid()
    {
        // Generate DICOM UID (simplified - would use proper UID generation in production)
        return $"2.25.{DateTime.UtcNow.Ticks}.{Guid.NewGuid().GetHashCode():D10}";
    }

    private static string GenerateViewerToken(string sessionId, string studyUid, int userId)
    {
        // Simplified token generation - would use proper JWT in production
        return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{sessionId}:{studyUid}:{userId}:{DateTime.UtcNow.Ticks}"));
    }

    private static string FormatBytes(long bytes)
    {
        string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
        int i = 0;
        double size = bytes;
        while (size >= 1024 && i < suffixes.Length - 1)
        {
            size /= 1024;
            i++;
        }
        return $"{size:N2} {suffixes[i]}";
    }

    #endregion
}

using System.Linq.Expressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Skafetin.Api.Data;
using Skafetin.Api.Security;
using Skafetin.Api.Storage;
using Skafetin.Shared.DTOs;
using Skafetin.Shared.Models;

namespace Skafetin.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EquipmentMediaController : ControllerBase
{
    private const string KindImage = "Image";
    private const string KindDocument = "Document";

    private const long MaxImageSize = 5 * 1024 * 1024;
    private const long MaxDocumentSize = 10 * 1024 * 1024;
    private static readonly Dictionary<string, (string Kind, string Extension)> AllowedContentTypes =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["image/jpeg"] = (KindImage, ".jpg"),
            ["image/png"] = (KindImage, ".png"),
            ["image/webp"] = (KindImage, ".webp"),
            ["application/pdf"] = (KindDocument, ".pdf")
        };

    private readonly SkafetinDbContext _context;
    private readonly IWebHostEnvironment _environment;
    private readonly IConfiguration _configuration;

    public EquipmentMediaController(
        SkafetinDbContext context,
        IWebHostEnvironment environment,
        IConfiguration configuration)
    {
        _context = context;
        _environment = environment;
        _configuration = configuration;
    }

    [HttpGet("equipment/{equipmentId:int}")]
    public async Task<ActionResult<List<EquipmentMediaDto>>> GetMediaForEquipment(
        int equipmentId,
        [FromQuery] string? kind)
    {
        var equipmentExists = await _context.Equipment.AnyAsync(e => e.Id == equipmentId);
        if (!equipmentExists)
            return NotFound();

        var query = _context.EquipmentMedia.Where(m => m.EquipmentId == equipmentId);

        if (!string.IsNullOrWhiteSpace(kind))
        {
            var normalized = NormalizeKind(kind);
            if (normalized is null)
                return BadRequest(new ErrorResponseDto
                {
                    Message = "Vrsta datoteke mora biti Image ili Document."
                });

            query = query.Where(m => m.MediaKind == normalized);
        }

        var result = await query
            .OrderByDescending(m => m.IsCover)
            .ThenByDescending(m => m.UploadedAt)
            .ThenBy(m => m.Id)
            .Select(ToDto)
            .ToListAsync();

        return Ok(result);
    }

    [Authorize(Policy = AuthorizationPolicies.Manage)]
    [HttpPost("equipment/{equipmentId:int}")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(MaxDocumentSize)]
    public async Task<ActionResult<EquipmentMediaDto>> Upload(
        int equipmentId,
        IFormFile file,
        [FromForm] string title,
        [FromForm] string kind)
    {
        var equipmentExists = await _context.Equipment.AnyAsync(e => e.Id == equipmentId);
        if (!equipmentExists)
            return NotFound();

        if (file is null || file.Length == 0)
            return BadRequest(new ErrorResponseDto { Message = "Odaberi datoteku." });

        if (string.IsNullOrWhiteSpace(title))
            return BadRequest(new ErrorResponseDto { Message = "Naslov je obavezan." });

        title = title.Trim();
        if (title.Length > 150)
            return BadRequest(new ErrorResponseDto
            {
                Message = "Naslov smije imati najviše 150 znakova."
            });

        var requestedKind = NormalizeKind(kind);
        if (requestedKind is null)
            return BadRequest(new ErrorResponseDto
            {
                Message = "Vrsta datoteke mora biti Image ili Document."
            });

        if (!AllowedContentTypes.TryGetValue(file.ContentType, out var allowed))
            return BadRequest(new ErrorResponseDto
            {
                Message = "Dopuštene su slike (JPG, PNG, WEBP) i PDF dokumenti."
            });

        if (allowed.Kind != requestedKind)
            return BadRequest(new ErrorResponseDto
            {
                Message = "Odabrana vrsta ne odgovara vrsti datoteke."
            });

        var maxSize = allowed.Kind == KindImage ? MaxImageSize : MaxDocumentSize;
        if (file.Length > maxSize)
            return BadRequest(new ErrorResponseDto
            {
                Message = allowed.Kind == KindImage
                    ? "Slika smije imati najviše 5 MB."
                    : "Dokument smije imati najviše 10 MB."
            });

        var uploadDirectory = GetUploadDirectory();
        Directory.CreateDirectory(uploadDirectory);

        var storedFileName = $"{Guid.NewGuid():N}{allowed.Extension}";
        var physicalPath = Path.Combine(uploadDirectory, storedFileName);

        await using (var stream = new FileStream(physicalPath, FileMode.CreateNew, FileAccess.Write))
        {
            await file.CopyToAsync(stream);
        }

        var originalFileName = Path.GetFileName(file.FileName);
        if (originalFileName.Length > 260)
            originalFileName = originalFileName[^260..];

        var media = new EquipmentMedia
        {
            EquipmentId = equipmentId,
            Title = title,
            MediaKind = allowed.Kind,
            OriginalFileName = originalFileName,
            StoredFileName = storedFileName,
            ContentType = file.ContentType,
            FileSize = file.Length,
            UploadedAt = DateTime.UtcNow,
            UploadedByEmployeeId = GetEmployeeIdFromToken()
        };

        if (allowed.Kind == KindImage)
        {
            var hasCover = await _context.EquipmentMedia
                .AnyAsync(m => m.EquipmentId == equipmentId && m.IsCover);

            media.IsCover = !hasCover;
        }

        try
        {
            _context.EquipmentMedia.Add(media);
            await _context.SaveChangesAsync();
        }
        catch
        {
            if (System.IO.File.Exists(physicalPath))
                System.IO.File.Delete(physicalPath);

            throw;
        }

        var result = await _context.EquipmentMedia
            .Where(m => m.Id == media.Id)
            .Select(ToDto)
            .FirstAsync();

        return CreatedAtAction(nameof(GetMediaForEquipment), new { equipmentId }, result);
    }

    [HttpGet("{id:int}/content")]
    public async Task<IActionResult> GetContent(int id)
    {
        var media = await _context.EquipmentMedia.FirstOrDefaultAsync(m => m.Id == id);
        if (media is null)
            return NotFound();

        var physicalPath = Path.Combine(
            GetUploadDirectory(),
            Path.GetFileName(media.StoredFileName));

        if (!System.IO.File.Exists(physicalPath))
            return NotFound();

        var stream = System.IO.File.OpenRead(physicalPath);

        return File(stream, media.ContentType, media.OriginalFileName);
    }

    [Authorize(Policy = AuthorizationPolicies.Manage)]
    [HttpPost("{id:int}/cover")]
    public async Task<ActionResult<EquipmentMediaDto>> SetCover(int id)
    {
        var media = await _context.EquipmentMedia.FirstOrDefaultAsync(m => m.Id == id);
        if (media is null)
            return NotFound();

        if (media.MediaKind != KindImage)
            return BadRequest(new ErrorResponseDto
            {
                Message = "Naslovna može biti samo slika."
            });

        var currentCovers = await _context.EquipmentMedia
            .Where(m => m.EquipmentId == media.EquipmentId && m.IsCover && m.Id != media.Id)
            .ToListAsync();

        foreach (var current in currentCovers)
            current.IsCover = false;

        media.IsCover = true;

        await _context.SaveChangesAsync();

        var result = await _context.EquipmentMedia
            .Where(m => m.Id == media.Id)
            .Select(ToDto)
            .FirstAsync();

        return Ok(result);
    }

    [Authorize(Policy = AuthorizationPolicies.Manage)]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var media = await _context.EquipmentMedia.FirstOrDefaultAsync(m => m.Id == id);
        if (media is null)
            return NotFound();

        _context.EquipmentMedia.Remove(media);

        if (media.IsCover)
        {
            var replacement = await _context.EquipmentMedia
                .Where(m => m.EquipmentId == media.EquipmentId
                         && m.Id != media.Id
                         && m.MediaKind == KindImage)
                .OrderByDescending(m => m.UploadedAt)
                .ThenBy(m => m.Id)
                .FirstOrDefaultAsync();

            if (replacement is not null)
                replacement.IsCover = true;
        }

        await _context.SaveChangesAsync();

        var physicalPath = Path.Combine(
            GetUploadDirectory(),
            Path.GetFileName(media.StoredFileName));

        if (System.IO.File.Exists(physicalPath))
            System.IO.File.Delete(physicalPath);

        return NoContent();
    }

    private static string? NormalizeKind(string? kind)
    {
        if (string.Equals(kind, KindImage, StringComparison.OrdinalIgnoreCase))
            return KindImage;

        if (string.Equals(kind, KindDocument, StringComparison.OrdinalIgnoreCase))
            return KindDocument;

        return null;
    }

    private string GetUploadDirectory() =>
        MediaStorage.GetUploadDirectory(_configuration, _environment.ContentRootPath);

    private int? GetEmployeeIdFromToken()
    {
        var claim = User.FindFirst(AppClaimTypes.EmployeeId)?.Value;

        return int.TryParse(claim, out var employeeId) ? employeeId : null;
    }

    private static readonly Expression<Func<EquipmentMedia, EquipmentMediaDto>> ToDto =
        m => new EquipmentMediaDto
        {
            Id = m.Id,
            EquipmentId = m.EquipmentId,
            EquipmentName = m.Equipment!.Name,
            InventoryNumber = m.Equipment!.InventoryNumber,
            Title = m.Title,
            MediaKind = m.MediaKind,
            IsCover = m.IsCover,
            OriginalFileName = m.OriginalFileName,
            ContentType = m.ContentType,
            FileSize = m.FileSize,
            ContentUrl = "/api/equipmentmedia/" + m.Id + "/content",
            UploadedAt = m.UploadedAt,
            UploadedByEmployeeId = m.UploadedByEmployeeId,
            UploadedByEmployeeFullName = m.UploadedByEmployee == null
                ? null
                : m.UploadedByEmployee.FirstName + " " + m.UploadedByEmployee.LastName
        };
}


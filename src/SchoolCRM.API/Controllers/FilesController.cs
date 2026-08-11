using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SchoolCRM.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FilesController : ControllerBase
{
    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".bmp", ".pdf", ".doc", ".docx" };
    private const long MaxFileSizeBytes = 10 * 1024 * 1024;

    [HttpPost("upload")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<object>> UploadAsync(IFormFile file)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { success = false, message = "No file was uploaded." });

        if (file.Length > MaxFileSizeBytes)
            return BadRequest(new { success = false, message = "File size must be 10 MB or less." });

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension))
            return BadRequest(new { success = false, message = $"File type '{extension}' is not allowed. Allowed: {string.Join(", ", AllowedExtensions)}" });

        var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
        Directory.CreateDirectory(uploadsDir);

        var fileName = $"{Guid.NewGuid():N}{extension}";
        var fullPath = Path.Combine(uploadsDir, fileName);

        await using (var stream = new FileStream(fullPath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var url = $"/uploads/{fileName}";
        return Ok(new
        {
            success = true,
            url,
            fileName = file.FileName,
            size = file.Length
        });
    }
}

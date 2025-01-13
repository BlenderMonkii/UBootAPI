using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Drawing.Imaging;
using UBootAPI.Service;

namespace UBootAPI.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileController : ControllerBase
    {
        private readonly IHeightmapService _heightmapService;

        public FileController(IHeightmapService heightmapService)
        {
            _heightmapService = heightmapService;
        }

        [HttpPost("upload")]
        public IActionResult UploadFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("Keine Datei hochgeladen.");
            }

            try
            {
                string heightmapPath = _heightmapService.GenerateHeightmap(file);
                Console.WriteLine($"Heightmap gespeichert unter: {Path.GetFileName(heightmapPath)}");
                return PhysicalFile(heightmapPath, "image/png", Path.GetFileName(heightmapPath));
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Fehler beim Verarbeiten der Datei: {ex.Message}");
            }
        }
    }
}

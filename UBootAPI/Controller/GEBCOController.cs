using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Drawing.Imaging;
using System.Drawing;
using System.Globalization;
using UBootAPI.Model;
using UBootAPI.Service;

namespace UBootAPI.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class GEBCOController : ControllerBase
    {
        IGEBCOService _gebcoService;
        public GEBCOController(IGEBCOService gebcoService)
        {
            _gebcoService = gebcoService;
        }

        [HttpGet]
        public async Task<IActionResult> GetHeightMap([FromQuery] BoundingBoxRequest request)
        {
            try
            {
                string filePath = await _gebcoService.GetGrayscaleHeightMapPathAsync(request);

                return PhysicalFile(filePath, "image/png", Path.GetFileName(filePath));
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(500, new { message = "Fehler beim Abrufen der Höhendaten", details = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = "Das Bild enthält keine Daten", details = ex.Message });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { message = "Interner Serverfehler", details = ex.Message });
            }
        }
    }
}


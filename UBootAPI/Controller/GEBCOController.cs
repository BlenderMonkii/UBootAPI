using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Drawing.Imaging;
using System.Drawing;
using UBootAPI.Model;
using UBootAPI.Service;

namespace UBootAPI.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class GEBCOController : ControllerBase
    {
        private readonly IGEBCOService _gebcoService;

        public GEBCOController(IGEBCOService gebcoService)
        {
            _gebcoService = gebcoService;
        }

        [HttpGet]
        public async Task<IActionResult> GetHeightMap([FromQuery] BoundingBoxRequest request)
        {
            try
            {
                // Abrufen und Verarbeitung in einer zentralen Methode des Services
                byte[] grayscaleMap = await _gebcoService.GetGrayscaleHeightMapAsync(request);

                // Rückgabe als PNG-Datei
                return File(grayscaleMap, "image/png", "height_map_grayscale.png");
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(500, new { message = "Fehler beim Abrufen der Höhendaten", details = ex.Message });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { message = "Interner Serverfehler", details = ex.Message });
            }
        }
    }

}


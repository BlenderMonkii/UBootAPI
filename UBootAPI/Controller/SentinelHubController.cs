using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace UBootAPI.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class SentinelHubController : ControllerBase
    {
        private string token = "eyJhbGciOiJSUzI1NiIsInR5cCIgOiAiSldUIiwia2lkIiA6ICJ3dE9hV1o2aFJJeUowbGlsYXctcWd4NzlUdm1hX3ZKZlNuMW1WNm5HX0tVIn0.eyJleHAiOjE3MzczMjQxMTQsImlhdCI6MTczNzMyMDUxNCwianRpIjoiNjFmM2RhOTAtMjQ2MS00YzlhLWI4YTktMjVmZGNiMThhMTY4IiwiaXNzIjoiaHR0cHM6Ly9zZXJ2aWNlcy5zZW50aW5lbC1odWIuY29tL2F1dGgvcmVhbG1zL21haW4iLCJzdWIiOiJiYmVlZjg0MS03OTExLTRjNjItYjY1NC1jMGU4ZGEwZjcxNjMiLCJ0eXAiOiJCZWFyZXIiLCJhenAiOiJiYzY0ZWJkYy1kNjA1LTQ3MTUtOGU2ZC0zZjA1MzIyYzY1YzEiLCJzY29wZSI6ImVtYWlsIHByb2ZpbGUiLCJjbGllbnRIb3N0IjoiNzcuMjIuMjU0LjIzMyIsImVtYWlsX3ZlcmlmaWVkIjpmYWxzZSwicGxfcHJvamVjdCI6ImE5NmNhYzJjLTA4NTItNDY2OS04ZjhkLTdlZTZhOTQ2YjI2NyIsInByZWZlcnJlZF91c2VybmFtZSI6InNlcnZpY2UtYWNjb3VudC1iYzY0ZWJkYy1kNjA1LTQ3MTUtOGU2ZC0zZjA1MzIyYzY1YzEiLCJjbGllbnRBZGRyZXNzIjoiNzcuMjIuMjU0LjIzMyIsImFjY291bnQiOiJhOTZjYWMyYy0wODUyLTQ2NjktOGY4ZC03ZWU2YTk0NmIyNjciLCJwbF93b3Jrc3BhY2UiOiJkMGMwYmYxYS1lZTI0LTQyOGQtYjQ2ZS1mZmVhZTk5OWFhNTUiLCJjbGllbnRfaWQiOiJiYzY0ZWJkYy1kNjA1LTQ3MTUtOGU2ZC0zZjA1MzIyYzY1YzEifQ.XIu2v4r0JEfeCMPJ5hn2bV9D7bbBMgbVR3mTEJRrshRZ9IYwgKAnPL01_naksbujJmbskz5PnD1gAG6k_PKEZbBqKc1152_R8YO2bqsWPo0rweTcburMoKXCNOa11ZRe5odpqcOUDGsjO6F8_Obns82AQGLOxgIkpXRlzyMiipamsLPgphtgDA8RJt0vZbpux28KtvisT4nM9OmGJUy1wKE0zRvI640ABjAJ_wU-vohYI5FNKgYFhM7Rfevj6XI3bA10UggVyEBHB1jF86ut5kgMQ6T38jlBf4N9-QNdIc5-N8MXtSCkaEyzD0EZdFM_fowwRv_SHFc3MVCufPyemw";
        string url = "https://services.sentinel-hub.com/api/v1/process";


        [HttpGet]
        public async Task<IActionResult> GetDEMData(
            [FromQuery] string lon_min,
            [FromQuery] string lat_min,
            [FromQuery] string lon_max,
            [FromQuery] string lat_max)
        {
            // API-Payload erstellen
            var payload = new
            {
                input = new
                {
                    bounds = new
                    {
                        bbox = new[] { lon_min, lat_min, lon_max, lat_max }
                    },
                    data = new[]
                    {
                        new
                        {
                            type = "DEM"
                        }
                    }
                },
                evalscript = @"
                    // Evalscript für Digital Elevation Model (DEM)
                    // Normalisiert die DEM-Daten und gibt sie als Graustufen-PNG aus.

                    // Minimal- und Maximalwerte für die Höhe in Metern
                    const minVal = 0; // Meereshöhe
                    const maxVal = 3000; // Maximaler Berg

                    // Normalisiere die DEM-Daten (Höhe in 0-1 Bereich skalieren)
                    let normalized = (DEM - minVal) / (maxVal - minVal);
                    normalized = Math.min(Math.max(normalized, 0), 1); // Clipping auf den Bereich [0, 1]

                    // Rückgabe als Graustufen-Pixelwert (0-1 entspricht Schwarz bis Weiß)
                    return [normalized];
                ",
                output = new
                {
                    width = 512,
                    height = 512,
                    responses = new[]
                    {
                        new
                        {
                            identifier = "default",
                            format = new { type = "image/tiff" } // Ausgabeformat
                        }
                    }
                }
            };

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    // Setze den Authorization-Header mit dem Token
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                    // Konvertiere das Payload in JSON
                    StringContent content = new StringContent(
                        System.Text.Json.JsonSerializer.Serialize(payload),
                        System.Text.Encoding.UTF8,
                        "application/json"
                    );

                    // Sende die Anfrage an die Sentinel Hub API
                    HttpResponseMessage response = await client.PostAsync(url, content);

                    if (response.IsSuccessStatusCode)
                    {
                        byte[] result = await response.Content.ReadAsByteArrayAsync();
                        // Rückgabe der DEM-Daten als PNG-Datei
                        return File(result, "image/tiff", "dem_data.tiff");
                    }
                    else
                    {
                        String error = await response.Content.ReadAsStringAsync();
                        return BadRequest(new { message = "Fehler beim Abrufen der DEM-Daten", details = error });
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Interner Serverfehler", details = ex.Message });
            }
        }

    }
}

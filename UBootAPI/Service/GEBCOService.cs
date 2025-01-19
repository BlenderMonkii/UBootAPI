using System.Drawing.Imaging;
using System.Drawing;
using UBootAPI.Model;
using UBootAPI.Service;
using System.Globalization;

public class GEBCOService : IGEBCOService
{
    private readonly HttpClient _httpClient;

    public GEBCOService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<byte[]> GetGrayscaleHeightMapAsync(BoundingBoxRequest request)
    {
        // 1. Abrufen der Höhendaten (PNG)
        byte[] heightMap = await GetHeightMapAsync(request);

        // 2. Konvertieren in Graustufen
        return ConvertToGrayscale(heightMap);
    }

    private async Task<byte[]> GetHeightMapAsync(BoundingBoxRequest request)
    {
        string bbox = FormatBoundingBox(request);
        string wmsUrl = $"https://wms.gebco.net/mapserv?service=WMS&version=1.3.0&request=GetMap&layers=gebco_latest&bbox={bbox}&width={request.Width}&height={request.Height}&crs=EPSG:4326&format=image/png&styles=default";

        HttpResponseMessage response = await _httpClient.GetAsync(wmsUrl);
        if (!response.IsSuccessStatusCode)
        {
            string errorDetails = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Error fetching GEBCO data: {errorDetails}");
        }

        return await response.Content.ReadAsByteArrayAsync();
    }
    private string FormatBoundingBox(BoundingBoxRequest request)
    {
        return string.Format(CultureInfo.InvariantCulture, "{0},{1},{2},{3}",
            request.MinLon, request.MinLat, request.MaxLon, request.MaxLat);
    }

    private byte[] ConvertToGrayscale(byte[] imageBytes)
    {
        using (MemoryStream ms = new MemoryStream(imageBytes))
        using (Bitmap originalImage = new Bitmap(ms))
        using (Bitmap grayscaleImage = CreateGrayscaleImage(originalImage))
        {
            using (var outputMs = new MemoryStream())
            {
                grayscaleImage.Save(outputMs, ImageFormat.Png);
                return outputMs.ToArray();
            }
        }
    }

    private Bitmap CreateGrayscaleImage(Bitmap original)
    {
        Bitmap grayscaleImage = new Bitmap(original.Width, original.Height);
        using (Graphics g = Graphics.FromImage(grayscaleImage))
        {
            ColorMatrix colorMatrix = new ColorMatrix(
                new float[][]
                {
                    new float[] { 0.3f, 0.3f, 0.3f, 0, 0 },
                    new float[] { 0.59f, 0.59f, 0.59f, 0, 0 },
                    new float[] { 0.11f, 0.11f, 0.11f, 0, 0 },
                    new float[] { 0, 0, 0, 1, 0 },
                    new float[] { 0, 0, 0, 0, 1 }
                });

            ImageAttributes imageAttributes = new ImageAttributes();
            imageAttributes.SetColorMatrix(colorMatrix);

            g.DrawImage(original, new Rectangle(0, 0, original.Width, original.Height),
                0, 0, original.Width, original.Height, GraphicsUnit.Pixel, imageAttributes);
        }

        return grayscaleImage;
    }
}

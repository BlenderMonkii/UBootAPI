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

    public async Task<string> GetGrayscaleHeightMapPathAsync(BoundingBoxRequest request)
    {
        byte[] heightMap = await GetHeightMapAsync(request);

        return SaveGrayscaleHeightMapToFile(heightMap, request);
    }

    private async Task<byte[]> GetHeightMapAsync(BoundingBoxRequest request)
    {
        string bbox = FormatBoundingBox(request);

        double lonDiff = request.MaxLon - request.MinLon;
        double latDiff = request.MaxLat - request.MinLat;

        int pixelsPerDegree = 100;

        int width = (int)(lonDiff * pixelsPerDegree);
        int height = (int)(latDiff * pixelsPerDegree);

        string wmsUrl = $"https://wms.gebco.net/mapserv?service=WMS&version=1.3.0&request=GetMap&layers=gebco_latest&bbox={bbox}&width={width}&height={height}&crs=EPSG:4326&format=image/png&styles=default";

        Console.WriteLine($"WMS URL: {wmsUrl}");

        HttpResponseMessage response = await _httpClient.GetAsync(wmsUrl);
        if (!response.IsSuccessStatusCode)
        {
            string errorDetails = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Error fetching GEBCO data: {errorDetails}");
        }

        return await response.Content.ReadAsByteArrayAsync();
    }

    private string SaveGrayscaleHeightMapToFile(byte[] heightMap, BoundingBoxRequest request)
    {
        string outputDirectory = Path.Combine(Path.GetTempPath(), "GEBCOHeightmaps");
        Directory.CreateDirectory(outputDirectory); // Stelle sicher, dass der Ordner existiert

        string fileName = $"heightmap_{request.MinLon}_{request.MinLat}_{request.MaxLon}_{request.MaxLat}.png";
        string outputPath = Path.Combine(outputDirectory, fileName);

        using (MemoryStream ms = new MemoryStream(heightMap))
        using (Bitmap originalImage = new Bitmap(ms))
        {
            if (IsImageWhite(originalImage))
            {
                throw new InvalidOperationException("Das Bild enthält keine Daten (nur weiß).");
            }

            using (Bitmap grayscaleImage = CreateGrayscaleImage(originalImage))
            {
                grayscaleImage.Save(outputPath, ImageFormat.Png);
            }
        }

        return outputPath;
    }

    private string FormatBoundingBox(BoundingBoxRequest request)
    {
        return string.Format(CultureInfo.InvariantCulture, "{0},{1},{2},{3}",
            request.MinLon, request.MinLat, request.MaxLon, request.MaxLat);
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

    private bool IsImageWhite(Bitmap bitmap)
    {
        for (int y = 0; y < bitmap.Height; y++)
        {
            for (int x = 0; x < bitmap.Width; x++)
            {
                Color pixelColor = bitmap.GetPixel(x, y);
                if (pixelColor.R != 255 || pixelColor.G != 255 || pixelColor.B != 255)
                {
                    return false;
                }
            }
        }
        return true;
    }
}


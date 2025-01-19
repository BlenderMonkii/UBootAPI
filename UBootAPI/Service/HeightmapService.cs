using System.Drawing;
using System.Drawing.Imaging;
using UBootAPI.Converter;
using UBootAPI.Factory;

namespace UBootAPI.Service
{
    public class HeightmapService : IHeightmapService
    {
        private readonly IConverterFactory _converterFactory;

        public HeightmapService(IConverterFactory converterFactory)
        {
            _converterFactory = converterFactory;
        }

        public string GenerateHeightmap(IFormFile file)
        {
            string tempFilePath = Path.Combine(Path.GetTempPath(), file.FileName);
            using (FileStream fileStream = new FileStream(tempFilePath, FileMode.Create))
            {
                file.CopyTo(fileStream);
            }

            IConverter converter = _converterFactory.GetConverter(file.FileName);

            Console.WriteLine($"Converter Type: {converter.GetType()}");
            if (converter == null)
            {
                throw new NotSupportedException("Kein Konverter für das Dateiformat gefunden.");
            }

            string outputPath = Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(file.FileName) + "_heightmap.png");
            using (var inputStream = new FileStream(tempFilePath, FileMode.Open, FileAccess.Read))
            {
                Bitmap bitmap = converter.ConvertToHeightmap(inputStream);

                bitmap.Save(outputPath, ImageFormat.Png);
            }

            return outputPath;
        }
    }
}


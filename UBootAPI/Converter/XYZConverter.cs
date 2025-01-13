using System.Drawing;
using UBootAPI.Wrapper;

namespace UBootAPI.Converter
{
    public class XYZConverter : IConverter
    {
        private readonly GDALWrapper _gdalWrapper;

        public XYZConverter(GDALWrapper gdalWrapper)
        {
            _gdalWrapper = gdalWrapper;
        }

        public Bitmap ConvertToHeightmap(Stream inputStream)
        {

            // Eingabedaten auf die Festplatte schreiben (GDAL arbeitet mit Dateien)
            string tempInputPath = Path.Combine(Path.GetTempPath(), "temp_input.xyz");
            string tempOutputPath = Path.Combine(Path.GetTempPath(), "temp_output.png");

            using (FileStream fileStream = new FileStream(tempInputPath, FileMode.Create))
            {
                inputStream.CopyTo(fileStream);
            }

            // Validierung der .xyz-Datei
            if (!ValidateXYZFile(tempInputPath))
            {
                throw new Exception($"Die Datei {tempInputPath} hat ein ungültiges Format.");
            }

            // GDALWrapper verwenden, um die XYZ-Daten zu rasterisieren
            _gdalWrapper.RasterizeXYZ(tempInputPath, tempOutputPath);

            // Ausgabe (PNG) laden und als Bitmap zurückgeben
            return _gdalWrapper.ReadPng(tempOutputPath);
        }

        private bool ValidateXYZFile(string filePath)
        {
            Console.WriteLine($"Validierung der Datei: {filePath}");

            foreach (var line in File.ReadLines(filePath))
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                var parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length != 3 || !double.TryParse(parts[0], out _) || !double.TryParse(parts[1], out _) || !double.TryParse(parts[2], out _))
                {
                    Console.WriteLine($"Ungültige Zeile gefunden: {line}");
                    return false;
                }
            }

            Console.WriteLine("Die Datei ist gültig.");
            return true;
        }
    }

}

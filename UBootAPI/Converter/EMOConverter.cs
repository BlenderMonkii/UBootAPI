using System.Drawing;
using System.Globalization;
using UBootAPI.Wrapper;

namespace UBootAPI.Converter
{
    public class EMOConverter : IConverter
    {
        private readonly GDALWrapper _gdalWrapper;

        public EMOConverter(GDALWrapper gdalWrapper)
        {
            _gdalWrapper = gdalWrapper;
        }
        public Bitmap ConvertToHeightmap(Stream inputStream)
        {
            string tempInputPath = Path.Combine(Path.GetTempPath(), $"input_{Guid.NewGuid()}.xyz");
            string tempOutputPath = Path.Combine(Path.GetTempPath(), $"output_{Guid.NewGuid()}.png");

            try
            {
                // EMO-Daten aus dem Stream in eine temporäre .xyz-Datei schreiben
                WriteEmoToXyzFile(inputStream, tempInputPath);

                // XYZ-Datei mit GDAL in eine Rasterdatei umwandeln
                _gdalWrapper.RasterizeXYZ(tempInputPath, tempOutputPath);

                // Die gerasterte PNG-Datei als Bitmap laden und zurückgeben
                return _gdalWrapper.ReadPng(tempOutputPath);
            }
            finally
            {
                //if (File.Exists(tempInputPath)) File.Delete(tempInputPath);
                //if (File.Exists(tempOutputPath)) File.Delete(tempOutputPath);
            }
        }

        private void WriteEmoToXyzFile(Stream inputStream, string outputPath)
        {
            using (var reader = new StreamReader(inputStream))
            using (var writer = new StreamWriter(outputPath))
            {
                var points = new List<(double Lat, double Lon, double Value)>();
                string line;

                while ((line = reader.ReadLine()) != null)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    var parts = line.Split(';');

                    // Validiere und wandle die Zeile um
                    double value = 0;
                    if (parts.Length < 5 ||
                        !double.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out double lat) ||  // Latitude
                        !double.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out double lon) ||  // Longitude
                        (!double.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out value) &&      // Höhenwert (Fallback auf Mittelwert)
                         !double.TryParse(parts[3], NumberStyles.Float, CultureInfo.InvariantCulture, out value) &&
                         !double.TryParse(parts[4], NumberStyles.Float, CultureInfo.InvariantCulture, out value)))
                    {
                        Console.WriteLine($"Überspringe ungültige EMO-Datenzeile: {line}");
                        continue;
                    }
                    points.Add((lat, lon, value));
                }

                // Sortiere nach Latitude (Y) und dann nach Longitude (X)
                points.Sort((a, b) =>
                {
                    int compareLat = a.Lat.CompareTo(b.Lat);
                    return compareLat != 0 ? compareLat : a.Lon.CompareTo(b.Lon);
                });

                // Schreibe sortierte Daten in das XYZ-Format
                foreach (var point in points)
                {
                    writer.WriteLine($"{point.Lon} {point.Lat} {point.Value}");
                }
            }
        }

    }
}

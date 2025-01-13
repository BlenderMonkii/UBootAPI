using OSGeo.GDAL;
using System.Diagnostics;
using System.Drawing;
using UBootAPI.Configuration;

namespace UBootAPI.Wrapper
{
    public class GDALWrapper
    {
        public GDALWrapper()
        {
            GDALConfiguration.ConfigureGdal();
        }
        public void RasterizeXYZ(string inputPath, string outputPath)
        {
            // GDAL-Befehl zur Rasterisierung von XYZ-Punktdaten
            //string command = $"gdal_grid -a invdist:power=2.0:smoothing=1.0:radius1=5:radius2=5:angle=0 " +
            //                 $"-outsize 512 512 -of GTiff {inputPath} {outputPath}";

            string command = $"gdal_translate {inputPath} {outputPath}";

            Console.WriteLine($"Ausführender Befehl: {command}");
            ExecuteCommand(command);
        }

        public Bitmap ReadGeoTiff(string inputPath)
        {
            // GeoTIFF-Daten laden und als Bitmap zurückgeben
            using var dataset = Gdal.Open(inputPath, Access.GA_ReadOnly);
            if (dataset == null)
            {
                throw new Exception("Fehler beim Laden der GeoTIFF-Datei.");
            }

            int width = dataset.RasterXSize;
            int height = dataset.RasterYSize;

            var rasterBand = dataset.GetRasterBand(1);
            float[] buffer = new float[width * height];
            rasterBand.ReadRaster(0, 0, width, height, buffer, width, height, 0, 0);

            // Bitmap erstellen
            Bitmap bitmap = new Bitmap(width, height);
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float value = buffer[y * width + x];
                    int colorValue = (int)(value * 255); // Werte normalisieren
                    colorValue = Math.Clamp(colorValue, 0, 255);
                    Color color = Color.FromArgb(colorValue, colorValue, colorValue);
                    bitmap.SetPixel(x, height - y - 1, color);
                }
            }

            return bitmap;
        }

        public Bitmap ReadPng(string inputPath)
        {
            // PNG-Daten laden und als Bitmap zurückgeben
            using var dataset = Gdal.Open(inputPath, Access.GA_ReadOnly);
            if (dataset == null)
            {
                throw new Exception("Fehler beim Laden der PNG-Datei.");
            }

            int width = dataset.RasterXSize;
            int height = dataset.RasterYSize;

            var rasterBand = dataset.GetRasterBand(1);
            byte[] buffer = new byte[width * height];
            rasterBand.ReadRaster(0, 0, width, height, buffer, width, height, 0, 0);

            // Bitmap erstellen
            Bitmap bitmap = new Bitmap(width, height);
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    byte value = buffer[y * width + x];
                    Color color = Color.FromArgb(value, value, value);
                    bitmap.SetPixel(x, height - y - 1, color);
                }
            }

            return bitmap;
        }

        private void ExecuteCommand(string command)
        {
            ProcessStartInfo processInfo = new ProcessStartInfo("cmd.exe", "/C " + command)
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            try
            {
                using (var process = Process.Start(processInfo))
                {
                    // Ausgaben lesen
                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();

                    process.WaitForExit(); // Warten, bis der Prozess beendet ist

                    // Standardausgabe anzeigen
                    if (!string.IsNullOrEmpty(output))
                    {
                        Console.WriteLine($"GDAL-Ausgabe: {output}");
                    }

                    // Fehlerausgabe anzeigen
                    if (!string.IsNullOrEmpty(error))
                    {
                        Console.WriteLine($"GDAL-Fehler: {error}");
                    }

                    // Prozessstatus prüfen
                    if (process.ExitCode != 0)
                    {
                        throw new Exception($"GDAL-Befehl fehlgeschlagen. Exit-Code: {process.ExitCode}\nFehler: {error}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fehler bei der Ausführung des Befehls: {ex.Message}");
                throw;
            }
        }
    }
}

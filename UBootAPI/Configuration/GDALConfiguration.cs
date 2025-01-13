using OSGeo.GDAL;

namespace UBootAPI.Configuration
{
    public class GDALConfiguration
    {
        public static void ConfigureGdal()
        {
            GdalConfiguration.ConfigureGdal();
            Console.WriteLine("GDAL wurde initialisiert.");
            Gdal.AllRegister();
            Console.WriteLine($"GDAL Version: {Gdal.VersionInfo(null)}");
        }
    }
}

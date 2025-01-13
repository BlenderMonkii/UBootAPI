using UBootAPI.Converter;
using UBootAPI.Wrapper;

namespace UBootAPI.Factory
{
    public class ConverterFactory : IConverterFactory
    {
        private readonly GDALWrapper _gdalWrapper;

        public ConverterFactory(GDALWrapper gdalWrapper)
        {
            _gdalWrapper = gdalWrapper;
        }

        public IConverter GetConverter(string fileName)
        {
            string extension = Path.GetExtension(fileName)?.ToLowerInvariant();

            return extension switch
            {
                ".xyz" => new XYZConverter(_gdalWrapper),
                //".tif" or ".tiff" => new GeoTiffConverter(_gdalWrapper),
                //".asc" => new ASCConverter(_gdalWrapper),
                _ => throw new NotSupportedException($"Dateiformat {extension} wird nicht unterstützt."),
            };
        }
    }
}

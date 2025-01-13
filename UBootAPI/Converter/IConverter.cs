using System.Drawing;

namespace UBootAPI.Converter
{
    public interface IConverter
    {
        Bitmap ConvertToHeightmap(Stream inputStream);
    }
}

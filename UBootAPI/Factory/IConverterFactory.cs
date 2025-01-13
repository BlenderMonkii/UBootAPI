using UBootAPI.Converter;

namespace UBootAPI.Factory
{
    public interface IConverterFactory
    {
        IConverter GetConverter(string fileName);
    }
}

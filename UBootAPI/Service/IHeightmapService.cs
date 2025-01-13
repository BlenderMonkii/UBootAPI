using System.Drawing;


namespace UBootAPI.Service
{
    public interface IHeightmapService
    {
        string GenerateHeightmap(IFormFile fileStream);
    }
}

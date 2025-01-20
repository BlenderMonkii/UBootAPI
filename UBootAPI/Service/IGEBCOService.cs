using UBootAPI.Model;

namespace UBootAPI.Service
{
    public interface IGEBCOService
    {
        Task<string> GetGrayscaleHeightMapPathAsync(BoundingBoxRequest request);
    }
}

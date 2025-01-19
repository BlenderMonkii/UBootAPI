using UBootAPI.Model;

namespace UBootAPI.Service
{
    public interface IGEBCOService
    {
        Task<byte[]> GetGrayscaleHeightMapAsync(BoundingBoxRequest request);
    }
}

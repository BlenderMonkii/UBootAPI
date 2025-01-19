namespace UBootAPI.Model
{
    public class BoundingBoxRequest
    {
        public double MinLon { get; set; } = -10;
        public double MinLat { get; set; } = -5;
        public double MaxLon { get; set; } = 10;
        public double MaxLat { get; set; } = 5;
        public int Width { get; set; } = 800;
        public int Height { get; set; } = 600;
    }

}

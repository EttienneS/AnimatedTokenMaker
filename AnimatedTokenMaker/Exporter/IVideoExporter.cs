namespace AnimatedTokenMaker.Exporter
{
    public interface IVideoExporter
    {
        string GenerateVideoFromFolder(string output, string pattern = "t%04d.png");
    }
}
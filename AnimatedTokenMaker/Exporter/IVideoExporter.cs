namespace AnimatedTokenMaker.Exporter
{
    public interface IVideoExporter
    {
        void GenerateVideoFromFolder(string output, string filename);
    }
}
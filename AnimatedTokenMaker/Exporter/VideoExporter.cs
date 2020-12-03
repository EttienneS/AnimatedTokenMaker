namespace AnimatedTokenMaker.Exporter
{
    public class VideoExporter : IVideoExporter
    {
        private readonly IFFmpegService _ffmpegService;

        public VideoExporter(IFFmpegService ffmpegService)
        {
            _ffmpegService = ffmpegService;
        }

        public void GenerateVideoFromFolder(string output, string filename)
        {
            string pattern = "t%04d.png";

            var sourceFolder = $"{output}\\{pattern}";

            _ffmpegService.EncodeFolderAsWebm(filename, sourceFolder);
        }
    }
}
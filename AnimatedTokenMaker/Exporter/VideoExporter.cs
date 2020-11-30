using System.IO;

namespace AnimatedTokenMaker.Exporter
{
    public class VideoExporter : IVideoExporter
    {
        private readonly IFFmpegService _ffmpegService;

        public VideoExporter(IFFmpegService ffmpegService)
        {
            _ffmpegService = ffmpegService;
        }

        public string GenerateVideoFromFolder(string output, string pattern = "t%04d.png")
        {
            var outputFile = Path.Combine(output, "output.webm");
            var sourceFolder = $"{output}\\{pattern}";

            _ffmpegService.EncodeFolderAsWebm(outputFile, sourceFolder);

            return outputFile;
        }
    }
}
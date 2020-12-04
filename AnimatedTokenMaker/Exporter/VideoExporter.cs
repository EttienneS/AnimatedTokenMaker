using AnimatedTokenMaker.Source;

namespace AnimatedTokenMaker.Exporter
{
    public class VideoExporter : IVideoExporter
    {
        private readonly IFFmpegService _ffmpegService;
        private readonly ISourceSetting _sourceSetting;

        public VideoExporter(IFFmpegService ffmpegService, ISourceSetting sourceSetting)
        {
            _ffmpegService = ffmpegService;
            _sourceSetting = sourceSetting;
        }

        public void GenerateVideoFromFolder(string output, string filename)
        {
            string pattern = "t%04d.png";

            var sourceFolder = $"{output}\\{pattern}";

            _ffmpegService.EncodeFolderAsWebm(filename, sourceFolder, _sourceSetting);
        }
    }
}
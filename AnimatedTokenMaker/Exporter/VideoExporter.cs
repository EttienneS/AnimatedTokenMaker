using AnimatedTokenMaker.Services;
using AnimatedTokenMaker.Source;

namespace AnimatedTokenMaker.Exporter
{
    public class VideoExporter : IVideoExporter
    {
        private readonly IEncoderService _encoderService;
        private readonly ISourceSetting _sourceSetting;

        public VideoExporter(IEncoderService encoderService, ISourceSetting sourceSetting)
        {
            _encoderService = encoderService;
            _sourceSetting = sourceSetting;
        }

        public void GenerateVideoFromFolder(string output, string filename)
        {
            string pattern = "t%04d.png";

            var sourceFolder = $"{output}\\{pattern}";

            _encoderService.EncodeFolderAsWebm(filename, sourceFolder, _sourceSetting);
        }
    }
}
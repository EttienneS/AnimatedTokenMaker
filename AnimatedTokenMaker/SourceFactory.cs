using AnimatedTokenMaker.Source;
using System.IO;
using System.Linq;

namespace AnimatedTokenMaker
{
    public class SourceFactory
    {
        private IFFmpegService _ffmpegService;
        private ISourceSetting _defaultSetting;

        public SourceFactory(IFFmpegService ffmpegService, ISourceSetting defaultSetting)
        {
            _ffmpegService = ffmpegService;
            _defaultSetting = defaultSetting;
        }

        public ISourceFile GetSource(string file)
        {
            ISourceFile layer;

            if (IsStaticImage(file))
            {
                layer = new StaticImageSource(file);
            }
            else
            {
                layer = new VideoSource(file, _ffmpegService, _defaultSetting);
            }

            return layer;
        }

        private bool IsStaticImage(string file)
        {
            var staticExtensions = new[] { "png", "bmp", "jpg", "tiff", "wmf", "exif", "emf", "ico" };
            return staticExtensions.Contains(Path.GetExtension(file).Trim('.'));
        }
    }
}
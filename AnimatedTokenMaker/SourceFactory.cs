using AnimatedTokenMaker.Source;
using System.IO;
using System.Linq;

namespace AnimatedTokenMaker
{
    public class SourceFactory
    {
        private IFFmpegService _ffmpegService;

        public SourceFactory(IFFmpegService ffmpegService)
        {
            _ffmpegService = ffmpegService;
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
                layer = new VideoSource(file, _ffmpegService);
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
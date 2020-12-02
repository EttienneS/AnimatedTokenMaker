using System.Drawing;
using System.Drawing.Imaging;

namespace AnimatedTokenMaker.Source
{
    public class StaticImageSource : SourceFileBase
    {
        private readonly Image _inputImage;

        public StaticImageSource(string inputFile)
        {
            _inputImage = Image.FromFile(inputFile);
        }

        public override void Dispose()
        {
            _inputImage.Dispose();
        }

        public override int GetFrameCount()
        {
            return 1;
        }

        public override Bitmap GetFrame(int frame, Size size)
        {
            return GetScaledOffsetFrame(size, GetImageAtFrame(frame));
        }

        private Bitmap GetImageAtFrame(int frame)
        {
            return new Bitmap(_inputImage);
        }
    }
}
using System;
using System.Drawing;

namespace AnimatedTokenMaker.Source
{
    public abstract class SourceFileBase : ISourceFile
    {
        private int _offsetX;
        private int _offsetY;
        private float _scale = 1f;

        public abstract void Dispose();

        public abstract Bitmap GetFrame(int frame, Size size);

        public abstract int GetFrameCount();

        public void SetOffset(int x, int y)
        {
            _offsetX = x;
            _offsetY = y;
        }

        public void SetScale(float scale)
        {
            _scale = scale;
        }

        internal Bitmap GetScaledOffsetFrame(Size size, Bitmap rawImage)
        {
            var scaledImage = new Bitmap(rawImage, Math.Max(5, (int)(rawImage.Width * _scale)), Math.Max(5, (int)(rawImage.Height * _scale)));
            var newFrame = new Bitmap(size.Width, size.Height);

            var adjustedOffsetX = CalculateOffset(rawImage.Width, _offsetX);
            var adjustedOffsetY = CalculateOffset(rawImage.Height, _offsetY);

            for (int y = 0; y < size.Height; y++)
            {
                for (int x = 0; x < size.Width; x++)
                {
                    var modifiedX = x + adjustedOffsetX;
                    var modifiedY = y + adjustedOffsetY;

                    if (modifiedX < 0 || modifiedY < 0 || modifiedX >= scaledImage.Width || modifiedY >= scaledImage.Height)
                    {
                        newFrame.SetPixel(x, y, Color.FromArgb(0, 0, 0, 0));
                    }
                    else
                    {
                        newFrame.SetPixel(x, y, scaledImage.GetPixel(modifiedX, modifiedY));
                    }
                }
            }
            return newFrame;
        }

        private static int CalculateOffset(int max, int offset)
        {
            return (int)(Math.Abs(offset) / 100f * max * (offset > 0 ? 1f : -1f));
        }
    }
}
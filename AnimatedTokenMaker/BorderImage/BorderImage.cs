using System.Drawing;

namespace AnimatedTokenMaker
{
    public class BorderImage : IBorderImage
    {
        private readonly Bitmap _border;

        public BorderImage(string borderImageFile)
        {
            _border = (Bitmap)Image.FromFile(borderImageFile);
        }

        public Bitmap GetEmptyBorderSizedBitmap()
        {
            return new Bitmap(_border.Width, _border.Height);
        }

        

        public Bitmap GetBitmap()
        {
            return _border;
        }
    }
}
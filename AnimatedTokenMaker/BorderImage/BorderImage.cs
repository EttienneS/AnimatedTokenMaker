using System.Drawing;

namespace AnimatedTokenMaker
{
    public class BorderImage : IBorderImage
    {
        private readonly Bitmap _border;

        private Color _color;

        private Bitmap _coloredBorder;

        public BorderImage(string borderImageFile)
        {
            _border = (Bitmap)Image.FromFile(borderImageFile);
        }

        public Bitmap GetEmptyBorderSizedBitmap()
        {
            return new Bitmap(_border.Width, _border.Height);
        }

        public Bitmap GetColoredBorderImage()
        {
            if (_coloredBorder == null)
            {
                return _border;
            }
            return _coloredBorder;
        }

        public void SetBorderColor(Color color)
        {
            _color = color;

            UpdateColoredBorder();
        }

        private void UpdateColoredBorder()
        {
            _coloredBorder = GetEmptyBorderSizedBitmap();
            for (int y = 0; y < _coloredBorder.Height; y++)
            {
                for (int x = 0; x < _coloredBorder.Width; x++)
                {
                    var px = _border.GetPixel(x, y);

                    if (px.A == 0 || (px.R == 0 && px.G == 0 && px.B == 0))
                    {
                        _coloredBorder.SetPixel(x, y, px);
                    }
                    else
                    {
                        _coloredBorder.SetPixel(x, y, _color);
                    }
                }
            }
        }
    }
}
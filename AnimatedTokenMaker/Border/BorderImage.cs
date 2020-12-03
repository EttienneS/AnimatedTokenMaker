using System.Drawing;

namespace AnimatedTokenMaker.Border
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

        public Size GetBorderSize()
        {
            return new Size(_border.Width, _border.Height);
        }

        public Bitmap GetColoredBorderImage()
        {
            if (_coloredBorder == null)
            {
                return _border;
            }
            return new Bitmap(_coloredBorder);
        }

        public void SetBorderColor(Color color)
        {
            _color = color;

            UpdateColoredBorder();
        }

        private void UpdateColoredBorder()
        {
            var borderSize = GetBorderSize();
            _coloredBorder = new Bitmap(borderSize.Width, borderSize.Height);
            for (int y = 0; y < _coloredBorder.Height; y++)
            {
                for (int x = 0; x < _coloredBorder.Width; x++)
                {
                    var borderPx = _border.GetPixel(x, y);
                    _coloredBorder.SetPixel(x, y, borderPx.Multiply(_color));
                }
            }
        }
    }
}
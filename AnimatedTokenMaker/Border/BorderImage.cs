using System.Drawing;
using System.IO;

namespace AnimatedTokenMaker.Border
{
    public class BorderImage : IBorderImage
    {
        private readonly Bitmap _border;
        private readonly Bitmap _mask;

        private Color _color;

        private Bitmap _coloredBorder;

        public BorderImage(string borderImageFile)
        {
            _border = (Bitmap)Image.FromFile(borderImageFile);

            var mask = GetMaskName(borderImageFile);
            if (File.Exists(mask))
            {
                _mask = (Bitmap)Image.FromFile(mask);
            }
            else
            {
                throw new FileNotFoundException($"Mask not found: {mask}");
            }
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

        public Bitmap GetMask()
        {
            return _mask;
        }

        public void SetBorderColor(Color color)
        {
            _color = color;

            UpdateColoredBorder();
        }

        private static string GetMaskName(string borderImageFile)
        {
            var dir = borderImageFile.Replace(Path.GetFileName(borderImageFile), string.Empty);

            var file = Path.GetFileNameWithoutExtension(borderImageFile) + "_mask" + Path.GetExtension(borderImageFile);

            return Path.Combine(dir, file);
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
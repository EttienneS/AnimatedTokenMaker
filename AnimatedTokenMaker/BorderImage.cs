using System.Drawing;

namespace AnimatedTokenMaker
{
    public class BorderImage : IBorderImage
    {
        private Bitmap _border;

        public BorderImage(string borderImageFile)
        {
            _border = (Bitmap)Image.FromFile(borderImageFile);
        }

        public Bitmap GetEmptyBorderSizedBitmap()
        {
            return new Bitmap(_border.Width, _border.Height);
        }

        public Bitmap CombineWithImage(Bitmap srcImage, int offsetX, int offsetY)
        {
            var newImage = GetEmptyBorderSizedBitmap();

            for (int y = 0; y < newImage.Height; y++)
            {
                for (int x = 0; x < newImage.Width; x++)
                {
                    var px = _border.GetPixel(x, y);
                    if (px.A == 0)
                    {
                        newImage.SetPixel(x, y, px);
                    }
                    else if (px != Color.White)
                    {
                        var oX = x + offsetX;
                        var oY = y + offsetY;
                        var sample = Color.FromArgb(0, 0, 0, 0);

                        if (oX < srcImage.Width && oY < srcImage.Height && oX > 0 && oY > 0)
                        {
                            sample = srcImage.GetPixel(oX, oY);
                        }

                        newImage.SetPixel(x, y, sample);
                    }
                    else
                    {
                        newImage.SetPixel(x, y, px);
                    }
                }
            }

            return newImage;
        }
    }
}
using System.Drawing;

namespace AnimatedTokenMaker.BorderImage
{
    public interface IBorderImage
    {
        Bitmap GetEmptyBorderSizedBitmap();

        Bitmap GetColoredBorderImage();

        void SetBorderColor(Color color);
    }
}
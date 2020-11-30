using System.Drawing;

namespace AnimatedTokenMaker.Border
{
    public interface IBorderImage
    {
        Bitmap GetEmptyBorderSizedBitmap();

        Bitmap GetColoredBorderImage();

        void SetBorderColor(Color color);
    }
}
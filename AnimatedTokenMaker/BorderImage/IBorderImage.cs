using System.Drawing;

namespace AnimatedTokenMaker
{
    public interface IBorderImage
    {
        Bitmap GetEmptyBorderSizedBitmap();

        Bitmap GetColoredBorderImage();

        void SetBorderColor(Color color);
    }
}
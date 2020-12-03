using System.Drawing;

namespace AnimatedTokenMaker.Border
{
    public interface IBorderImage
    {
        Size GetBorderSize();

        Bitmap GetColoredBorderImage();

        void SetBorderColor(Color color);
    }
}
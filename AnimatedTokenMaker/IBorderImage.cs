using System;
using System.Drawing;

namespace AnimatedTokenMaker
{
    public interface IBorderImage
    {

        Bitmap GetEmptyBorderSizedBitmap();

        Bitmap CombineWithImage(Bitmap srcImage, int offsetX, int offsetY);


    }
}
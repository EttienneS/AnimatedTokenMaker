using System.Drawing;

namespace AnimatedTokenMaker
{
    public interface ISourceFile
    {
        int GetFrameCount();

        Bitmap GetScaledFrame(int i, float scale);
    }
}
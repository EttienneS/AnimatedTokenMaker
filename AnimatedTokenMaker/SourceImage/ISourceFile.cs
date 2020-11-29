using System;
using System.Drawing;

namespace AnimatedTokenMaker
{
    public interface ISourceFile : IDisposable
    {
        int GetFrameCount();

        Bitmap GetScaledFrame(int i, float scale);
    }
}
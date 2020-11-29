using System;
using System.Drawing;

namespace AnimatedTokenMaker.Source
{
    public interface ISourceFile : IDisposable
    {
        int GetFrameCount();

        Bitmap GetScaledFrame(int i, float scale);
    }
}
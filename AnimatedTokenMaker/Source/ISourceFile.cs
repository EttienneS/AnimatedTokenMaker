using System;
using System.Drawing;

namespace AnimatedTokenMaker.Source
{
    public interface ISourceFile : IDisposable
    {
        int GetFrameCount();

        Bitmap GetFrame(int frame, Size size);

        void SetOffset(int x, int y);

        void SetScale(float scale);
    }
}
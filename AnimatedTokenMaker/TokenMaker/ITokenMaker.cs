﻿using AnimatedTokenMaker.Border;
using AnimatedTokenMaker.Source;
using System.Drawing;

namespace AnimatedTokenMaker
{
    public interface ITokenMaker
    {
        void ExportToken();
        Bitmap GetPreview(int frame = 0);
        void LoadBorder(IBorderImage border);
        void AddLayer(ISourceFile source);
        void MoveLayerDown(ISourceFile layer);
        void MoveLayerUp(ISourceFile layer);
        void RemoveLayer(ISourceFile layer);
        Size GetBorderSize();
        void SetBorderColor(Color color);
    }
}
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;

namespace AnimatedTokenMaker
{
    public interface IISourceSetting
    {
        int GetFrameRate();
        int GetMaxTime();
    }
}
using System.IO;
using System.Linq;

namespace AnimatedTokenMaker
{
    public static class FileHelpers
    {
       

        public static bool HasExtension(string file, string[] staticExtensions)
        {
            return staticExtensions.Contains(Path.GetExtension(file).Trim('.'));
        }
     
    }
}
using AnimatedTokenMaker.Source;

namespace AnimatedTokenMaker
{

    public static class TokenMakerDelegates
    {
        public delegate void ExportLayerStartedDelegate(int layer, int total);

        public delegate void ExportLayerCompletedDelegate(int layer, int total);
    }
}
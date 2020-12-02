using AnimatedTokenMaker.Source;

namespace AnimatedTokenMaker
{
    public static class LayerDelegates
    {
        public delegate void MoveLayerDownDelegate(ISourceFile layer);

        public delegate void MoveLayerUpDelegate(ISourceFile layer);

        public delegate void RemoveLayerDelegate(ISourceFile layer);

        public delegate void LayerChangedDelegate(ISourceFile layer);
    }

    public static class TokenMakerDelegates
    {
        public delegate void LayerDrawnDelegate(int layer, int total);

        public delegate void ExportLayerDelegate(int layer, int total);
    }
}
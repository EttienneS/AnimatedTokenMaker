using AnimatedTokenMaker.Border;
using AnimatedTokenMaker.Exporter;
using AnimatedTokenMaker.Source;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace AnimatedTokenMaker
{
    public class TokenMaker : ITokenMaker
    {
        private readonly List<ISourceFile> _layers;
        private readonly IVideoExporter _videoExporter;
        private readonly string _workingFolder = "temp";
        private IBorderImage _border;

        public TokenMaker()
        {
            _layers = new List<ISourceFile>();
        }

        public TokenMaker(IVideoExporter videoExporter)
        {
            _videoExporter = videoExporter;
            _layers = new List<ISourceFile>();

            if (Directory.Exists(_workingFolder))
            {
                Directory.Delete(_workingFolder, true);
            }
            Directory.CreateDirectory(_workingFolder);
        }

        public event TokenMakerDelegates.ExportLayerCompletedDelegate OnExportLayerCompleted;

        public event TokenMakerDelegates.ExportLayerStartedDelegate OnExportLayerStarted;

        public void AddLayer(ISourceFile source)
        {
            _layers.Add(source);
        }

        public void ExportToken(string filename)
        {
            var outputFolder = GetOutputFolder();
            var totalFrames = GetFrameCount();

            if (totalFrames == 1)
            {
                var image = GetCombinedImageForFrame(0);
                image.Save(filename + ".png");
            }
            else
            {
                for (int i = 0; i < totalFrames; i++)
                {
                    LayerExportStarted(i, totalFrames);
                    var newImage = GetCombinedImageForFrame(i);

                    LayerExportCompleted(i, totalFrames);
                    newImage.Save(Path.Combine(outputFolder, "t" + i.ToString("").PadLeft(4, '0') + ".png"), ImageFormat.Png);
                }

                _videoExporter.GenerateVideoFromFolder(outputFolder, filename);
            }

            Process.Start("explorer", $"\"{Path.GetDirectoryName(filename)}\"");
        }

        public Size GetBorderSize()
        {
            return _border.GetBorderSize();
        }

        public Bitmap GetCombinedImageForFrame(int frame)
        {
            var borderImage = _border.GetColoredBorderImage();
            var borderSize = _border.GetBorderSize();

            var layers = GetReversedLayers(frame, borderSize);

            if (layers.Count == 0)
            {
                return new Bitmap(borderImage);
            }
            else
            {
                var source = layers[0];
                layers.RemoveAt(0);

                if (layers.Count > 0)
                {
                    foreach (var layer in layers)
                    {
                        source = CompositLayers(layer, source);
                    }
                }

                return ApplyBorder(borderImage, _border.GetMask(), source);
            }
        }

        public int GetFrameCount()
        {
            var biggestCount = 1;

            foreach (var layer in _layers)
            {
                biggestCount = Math.Max(biggestCount, layer.GetFrameCount());
            }

            return biggestCount;
        }

        public Bitmap GetPreview(int frame = 0)
        {
            return GetCombinedImageForFrame(frame);
        }

        public void LoadBorder(IBorderImage border)
        {
            _border = border;
            _border.SetBorderColor(_color);
        }

        public void MoveLayerDown(ISourceFile layer)
        {
            var index = _layers.IndexOf(layer);

            if (index == _layers.Count - 1)
            {
                return;
            }

            _layers.RemoveAt(index);
            _layers.Insert(index + 1, layer);
        }

        public void MoveLayerUp(ISourceFile layer)
        {
            var index = _layers.IndexOf(layer);

            if (index == 0)
            {
                return;
            }

            _layers.RemoveAt(index);
            _layers.Insert(index - 1, layer);
        }

        public void RemoveLayer(ISourceFile layer)
        {
            if (_layers.Contains(layer))
            {
                _layers.Remove(layer);
            }
        }

        private Color _color = Color.White;

        public void SetBorderColor(Color color)
        {
            _color = color;
            _border.SetBorderColor(color);
        }

        private Bitmap ApplyBorder(Bitmap borderImage, Bitmap mask, Bitmap source)
        {
            return new Bitmap(MaskOutBorder(borderImage, mask, source));
        }

        private static Bitmap MaskOutBorder(Bitmap borderImage, Bitmap mask, Bitmap source)
        {
            for (int y = 0; y < source.Height; y++)
            {
                for (int x = 0; x < source.Width; x++)
                {
                    var maskpx = mask.GetPixel(x, y);
                    if (maskpx.A == 0)
                    {
                        // the image has been completely masked out
                        source.SetPixel(x, y, Color.Transparent);
                    }
                }
            }

            // mask has been applied, overlay border image
            return CompositLayers(source, borderImage);
        }


        private static Bitmap CompositLayers(Bitmap layer1, Bitmap layer2)
        {
            var newLayer = new Bitmap(layer1.Width, layer1.Height, PixelFormat.Format32bppArgb);
            var graphics = Graphics.FromImage(newLayer);
            graphics.CompositingMode = CompositingMode.SourceOver;

            graphics.DrawImage(layer1, 0, 0);
            graphics.DrawImage(layer2, 0, 0);

            return newLayer;
        }

        private string GetOutputFolder()
        {
            var outputFolder = Path.Combine(_workingFolder, "output");
            if (Directory.Exists(outputFolder))
            {
                Directory.Delete(outputFolder, true);
            }
            Directory.CreateDirectory(outputFolder);
            return outputFolder;
        }

        private List<Bitmap> GetReversedLayers(int frame, Size borderSize)
        {
            return _layers.Select(l => l.GetFrame(frame, borderSize)).Reverse().ToList();
        }

        private void LayerExportCompleted(int layer, int total)
        {
            OnExportLayerCompleted?.Invoke(layer, total);
        }

        private void LayerExportStarted(int layer, int total)
        {
            OnExportLayerStarted?.Invoke(layer, total);
        }
    }
}
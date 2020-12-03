using AnimatedTokenMaker.Border;
using AnimatedTokenMaker.Exporter;
using AnimatedTokenMaker.Source;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
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

        public Bitmap GetCombinedImageForFrame(int frame)
        {
            var borderImage = _border.GetColoredBorderImage();
            var borderSize = _border.GetBorderSize();
            var newImage = new Bitmap(borderSize.Width, borderSize.Height);

            var layers = GetReversedLayers(frame, borderSize);

            for (int y = 0; y < newImage.Height; y++)
            {
                for (int x = 0; x < newImage.Width; x++)
                {
                    var borderSample = borderImage.GetPixel(x, y);
                    var pixel = borderSample;

                    if (borderSample.A == 0)
                    {
                        pixel = Color.FromArgb(0, 0, 0, 0);
                    }
                    else
                    {
                        if (_layers.Count == 0)
                        {
                            pixel = Color.Black;
                        }
                        else
                        {
                            pixel = GetBlendedLayerValue(layers, y, x);
                        }
                    }

                    var finalPixel = Color.FromArgb(pixel.A, pixel.R, pixel.G, pixel.B);
                    newImage.SetPixel(x, y, finalPixel);
                }
            }

            for (int y = 0; y < newImage.Height; y++)
            {
                for (int x = 0; x < newImage.Width; x++)
                {
                    var borderpx = borderImage.GetPixel(x, y);
                    var pixel = newImage.GetPixel(x, y);

                    if (borderpx.A == 255)
                    {
                        pixel = borderpx;
                    }
                    else
                    {
                        pixel = pixel.Add(borderpx);
                    }
                    newImage.SetPixel(x, y, pixel);
                }
            }

            return newImage;
        }

        private static Color GetBlendedLayerValue(List<Bitmap> layers, int y, int x)
        {
            Color newColor = Color.Black;

            if (layers.Count > 0)
            {
                newColor = layers[0].GetPixel(x, y);
            }

            if (layers.Count > 1 && newColor.A < 255)
            {
                // only do this if the top level is less than 255 opacity AND there are more than 1 layers
                var reversed = layers.Select(l => l).Reverse().ToList();

                var r = 0;
                var g = 0;
                var b = 0;
                foreach (var layer in reversed)
                {
                    var sample = layer.GetPixel(x, y);

                    if (sample.A == 255)
                    {
                        r = sample.R;
                        g = sample.G;
                        b = sample.B;
                    }
                    else
                    {
                        var a = sample.A / 255f;

                        r = (int)Math.Min(255, r + (sample.R * a));
                        g = (int)Math.Min(255, g + (sample.G * a));
                        b = (int)Math.Min(255, b + (sample.B * a));
                    }
                }

                newColor = Color.FromArgb(255, r, g, b);
            }

            return newColor;
        }

        private List<Bitmap> GetReversedLayers(int frame, Size borderSize)
        {
            return _layers.Select(l => l.GetFrame(frame, borderSize)).Reverse().ToList();
        }



        public event TokenMakerDelegates.ExportLayerCompletedDelegate OnExportLayerCompleted;
        public event TokenMakerDelegates.ExportLayerStartedDelegate OnExportLayerStarted;

        private void LayerExportStarted(int layer, int total)
        {
            OnExportLayerStarted?.Invoke(layer, total);
        }

        private void LayerExportCompleted(int layer, int total)
        {
            OnExportLayerCompleted?.Invoke(layer, total);
        }

        public void ExportToken(string filename)
        {
            var outputFolder = GetOutputFolder();
            var totalFrames = GetFrameCount();
            for (int i = 0; i < totalFrames; i++)
            {
                LayerExportStarted(i, totalFrames);
                var newImage = GetCombinedImageForFrame(i);

                LayerExportCompleted(i, totalFrames);
                newImage.Save(Path.Combine(outputFolder, "t" + i.ToString("").PadLeft(4, '0') + ".png"), ImageFormat.Png);
            }

            _videoExporter.GenerateVideoFromFolder(outputFolder, filename);

            Process.Start("explorer", $"\"{Path.GetDirectoryName(filename)}\"");
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
        }

        public void AddLayer(ISourceFile source)
        {
            _layers.Add(source);
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

        public void RemoveLayer(ISourceFile layer)
        {
            if (_layers.Contains(layer))
            {
                _layers.Remove(layer);
            }
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

        public Size GetBorderSize()
        {
            return _border.GetBorderSize();
        }

        public void SetBorderColor(Color color)
        {
            _border.SetBorderColor(color);
        }
    }
}
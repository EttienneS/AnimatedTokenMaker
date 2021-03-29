using AnimatedTokenMaker.Services;
using AnimatedTokenMaker.Source;
using System;
using System.Collections.Generic;

namespace AnimatedTokenMaker
{
    public class SourceFactory
    {
        private ISourceSetting _defaultSetting;
        private delegate ISourceFile NewSourceFactory(string file);

        private Dictionary<string[], NewSourceFactory> _decoderServices;
        private NewSourceFactory _default;

        public SourceFactory(ISourceSetting defaultSetting)
        {
            _decoderServices = new Dictionary<string[], NewSourceFactory>
            {
                { new []{ "png", "bmp", "jpg", "tiff", "wmf", "exif", "emf", "ico" }, (file) => new StaticImageSource(file) },
                { new []{ "webp" }, (file) => CreateWebmuxSource(file) }
            };

            _default = (file) => new VideoSource(file, ServiceManager.Instance.FFmpegService, _defaultSetting);
            _defaultSetting = defaultSetting;
        }

        private ISourceFile CreateWebmuxSource(string file)
        {
            if (ServiceManager.Instance.WebmuxService.IsReady())
            {
                return new VideoSource(file, ServiceManager.Instance.WebmuxService, _defaultSetting);
            }
            else
            {
                throw new SourceNotReadyException(ServiceManager.Instance.WebmuxService.Message);
            }
        }

        public ISourceFile GetSource(string file)
        {
            foreach (var decoder in _decoderServices)
            {
                if (FileHelpers.HasExtension(file, decoder.Key))
                {
                    return decoder.Value.Invoke(file);
                }
            }

            return _default.Invoke(file);
        }


    }
}
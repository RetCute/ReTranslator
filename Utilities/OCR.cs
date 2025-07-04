using System;
using System.Drawing;
using System.Threading;
using PaddleOCRSharp;
using ReTranslator.Utilities;

namespace ReTranslator.Utilities
{
    public class OCRThread
    {
        private readonly OCR _ocr;
        private Thread _thread;

        public OCRThread(OCR ocr)
        {
            _ocr = ocr;
        }

        public void Start()
        {
            _thread = new Thread(Run);
            _thread.IsBackground = true;
            _thread.Start();
        }

        private void Run()
        {
            try
            {
                Logger.Info("启动OCR中...");
                OCRModelConfig config = new OCRModelConfig
                {
                    rec_infer = @"OCRModel\rec",
                    det_infer = @"OCRModel\det",
                    cls_infer = @"OCRModel\cls",
                    keys = @"OCRModel\japan_dict.txt"
                };
                OCRParameter ocrParameter = new OCRParameter
                {
                    use_angle_cls = true
                };
                _ocr.ocr = new PaddleOCREngine(config, ocrParameter);
                _ocr._isReady = true;
                Logger.Success("OCR启动成功!");
            }
            catch (Exception e)
            {
                Logger.Error("OCR启动失败: " + e);
            }
        }
    }

    public class OCR
    {
        private static readonly Lazy<OCR> _lazyInstance = new(() => new OCR());
        public static OCR Instance => _lazyInstance.Value;

        public PaddleOCREngine ocr;
        public bool _isReady = false;

        private OCR() {}

        public void Init()
        {
            var initThread = new OCRThread(this);
            initThread.Start();
        }

        public string ReadText(Bitmap image)
        {
            try
            {
                var result = ocr?.DetectText(image);
                return result?.Text ?? string.Empty;
            }
            catch (Exception ex)
            {
                Logger.Error("识别图像文字失败: " + ex);
                return string.Empty;
            }
        }

        public bool IsReady => _isReady;
    }
}

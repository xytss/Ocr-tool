using RapidOcrNet;
using SkiaSharp;

namespace OcrTool.Engine;

public sealed class OcrEngine : IDisposable
{
    private readonly RapidOcr _ocr = new();

    public OcrEngine()
    {
        string modelRoot = Path.Combine(AppContext.BaseDirectory, "models");
        RapidOcrModelSet modelSet = RapidOcrModelSet.PPOCRv6Small with
        {
            DetModelPath = Path.Combine(modelRoot, "v6", "PP-OCRv6_det_small.onnx"),
            ClsModelPath = Path.Combine(modelRoot, "v5", "ch_PP-LCNet_x0_25_textline_ori_cls_mobile.onnx"),
            RecModelPath = Path.Combine(modelRoot, "v6", "PP-OCRv6_rec_small.onnx"),
            KeysPath = Path.Combine(modelRoot, "v6", "ppocrv6_dict.txt")
        };

        _ocr.InitModels(modelSet);
    }

    public string Recognize(SKBitmap image)
    {
        OcrResult result = _ocr.Detect(image, RapidOcrOptions.PPOCRv6);
        return result.StrRes;
    }

    public void Dispose()
    {
        _ocr.Dispose();
    }
}

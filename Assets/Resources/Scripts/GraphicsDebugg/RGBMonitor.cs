using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RGBMonitor : MonoBehaviour
{
    public ComputeShader PixelCounter;
    public int ScaleY = 100;
    public Texture2D ReferenceInputTexture;
    [HideInInspector] public Texture2D InputTexture;
    [HideInInspector] public Texture2D OutputTexture;
    [HideInInspector] public Texture2D OutputTexture_Ref;
    [HideInInspector] public uint[] RGBHistogramData;
    [HideInInspector] public uint[] RGBHistogramData_Ref;

    private ComputeBuffer _histogramBuffer;
    private int _handleMain;
    private int _handleInitialize;

    private Texture2D _lastReferenceTexture;
    private Texture2D _referenceInputTexture_Formatted;
    private ComputeShader _pixelCounter_Ref;
    private ComputeBuffer _histogramBuffer_Ref;
    private int _handleMain_Ref;
    private int _handleInitialize_Ref;

    private int _channelSplit = 0;// 0 rgb / 1 r / 2 g / 3 b
    private float _updateTimer;

    private const float _updateDelay = 0.1f;
    private const string PixelCounterName =" RGBMonitorPixelCounter.compute";

    void Start()
    {
        // Spawn compute shaders
        if (PixelCounter == null)
        {
            Debug.LogError("Pixel couting compute shader missing. (" + PixelCounterName + ")");
            return;
        }
        _pixelCounter_Ref = Instantiate(PixelCounter);

        OutputTexture = new Texture2D(256, 128);
        OutputTexture_Ref = new Texture2D(256, 128);

        ////// Initialize pixel counter 
        _handleInitialize = PixelCounter.FindKernel("HistogramInitialize");
        _handleMain = PixelCounter.FindKernel("HistogramMain");
        _histogramBuffer = new ComputeBuffer(256, sizeof(uint) * 4);
        RGBHistogramData = new uint[256 * 4];

        if (_handleInitialize < 0 || _handleMain < 0 || null == _histogramBuffer || null == RGBHistogramData)
        {
            Debug.LogError(PixelCounterName + " Initialization failed.");
            return;
        }
        PixelCounter.SetBuffer(_handleMain, "HistogramBuffer", _histogramBuffer);
        PixelCounter.SetBuffer(_handleInitialize, "HistogramBuffer", _histogramBuffer);

        ////// Initialize pixel counter for reference image
        _handleInitialize_Ref = _pixelCounter_Ref.FindKernel("HistogramInitialize");
        _handleMain_Ref = _pixelCounter_Ref.FindKernel("HistogramMain");
        _histogramBuffer_Ref = new ComputeBuffer(256, sizeof(uint) * 4);
        RGBHistogramData_Ref = new uint[256 * 4];

        if (_handleInitialize_Ref < 0 || _handleMain_Ref < 0 || null == _histogramBuffer_Ref || null == RGBHistogramData_Ref)
        {
            Debug.LogError(PixelCounterName + " (for reference image) Initialization failed.");
            return;
        }
        _pixelCounter_Ref.SetBuffer(_handleMain_Ref, "HistogramBuffer", _histogramBuffer_Ref);
        _pixelCounter_Ref.SetBuffer(_handleInitialize_Ref, "HistogramBuffer", _histogramBuffer_Ref);

        UpdateReference();
    }



    void Update()
    {
        if(_updateTimer >= _updateDelay)
        {
            UpdateInputTexture();
            CountPixels();
            UpdateOutputTexture();
            _updateTimer = 0;
        }
        _updateTimer += Time.deltaTime;
    }

    public void SetChannelSplit(int option)
    {
        _channelSplit = option;
        UpdateReference();
    }

    //////functions for histagram

    public void UpdateInputTexture()
    {
        if (InputTexture != null)
        {
            DestroyImmediate(InputTexture);
        }

        InputTexture = ScreenCapture.CaptureScreenshotAsTexture();

        PixelCounter.SetTexture(_handleMain, "InputTexture", InputTexture);
    }

    public void CountPixels()
    {
        if (null == PixelCounter || null == InputTexture || 0 > _handleInitialize || 0 > _handleMain || null == _histogramBuffer || null == RGBHistogramData)
        {
            Debug.LogError("Cannot compute pixel counter histogram.");
            return;
        }

        PixelCounter.Dispatch(_handleInitialize, 256 / 64, 1, 1);
        PixelCounter.Dispatch(_handleMain, (InputTexture.width + 7) / 8, (InputTexture.height + 7) / 8, 1);

        _histogramBuffer.GetData(RGBHistogramData);
    }

    public void UpdateOutputTexture()
    {
        Color[] colors = new Color[256 * 128];
        int[,] rgbCount = new int[3, 256];
        float scale = (InputTexture.width * InputTexture.height) / (Mathf.Max(1, ScaleY));

        for (int i = 0; i < 256; i++)
        {
            for (int c = 0; c < 3; c++)
            {
                rgbCount[c, i] = (int)RGBHistogramData[i * 4 + c];
            }
        }

        for (int y = 0; y < 128; y++)
        {
            float ph = y / 128f;
            for (int x = 0; x < 256; x++)
            {
                float red = (_channelSplit == 0 || _channelSplit == 1) && (rgbCount[0, x] / scale >= ph) ? 1 : 0;
                float green = (_channelSplit == 0 || _channelSplit == 2) && (rgbCount[1, x] / scale >= ph) ? 1 : 0;
                float blue = (_channelSplit == 0 || _channelSplit == 3) && (rgbCount[2, x] / scale >= ph) ? 1 : 0;
                colors[x + y * 256] = new Color(red , green, blue);
            }
        }

        OutputTexture.SetPixels(colors);
        OutputTexture.Apply();
    }

    //////functions for histagram of reference image
    
    public void UpdateReference()
    {
        if (ReferenceInputTexture != null)
        {
            UpdateReferenceInputTexture();
            ReferenceCountPixels();
            UpdateReferenceOutputTexture();
        }
    }

    public void UpdateReferenceInputTexture()
    {
        // reformat
        if(_referenceInputTexture_Formatted == null || _referenceInputTexture_Formatted.width != ReferenceInputTexture.width || _referenceInputTexture_Formatted.height != ReferenceInputTexture.height || _lastReferenceTexture != ReferenceInputTexture)
        {
            if (_referenceInputTexture_Formatted != null)
            {
                DestroyImmediate(_referenceInputTexture_Formatted);
            }

            _referenceInputTexture_Formatted = new Texture2D(ReferenceInputTexture.width, ReferenceInputTexture.height, TextureFormat.RGBA32, false);
            Graphics.ConvertTexture(ReferenceInputTexture, _referenceInputTexture_Formatted);

            _lastReferenceTexture = ReferenceInputTexture;
        }

        _pixelCounter_Ref.SetTexture(_handleMain_Ref, "InputTexture", _referenceInputTexture_Formatted);
    }

    public void ReferenceCountPixels()
    {
        if (null == _pixelCounter_Ref || null == ReferenceInputTexture || 0 > _handleInitialize_Ref || 0 > _handleMain_Ref || null == _histogramBuffer_Ref || null == RGBHistogramData_Ref)
        {
            Debug.LogError("Cannot compute pixel counter histogram for reference texture.");
            return;
        }

        _pixelCounter_Ref.Dispatch(_handleInitialize_Ref, 256 / 64, 1, 1);
        _pixelCounter_Ref.Dispatch(_handleMain_Ref, (_referenceInputTexture_Formatted.width + 7) / 8, (_referenceInputTexture_Formatted.height + 7) / 8, 1);

        _histogramBuffer_Ref.GetData(RGBHistogramData_Ref);
    }

    public void UpdateReferenceOutputTexture()
    {
        Color[] colors = new Color[256 * 128];
        int[,] rgbCount = new int[3, 256];
        float scale = (_referenceInputTexture_Formatted.width * _referenceInputTexture_Formatted.height) / (Mathf.Max(1, ScaleY));

        for (int i = 0; i < 256; i++)
        {
            for (int c = 0; c < 3; c++)
            {
                rgbCount[c, i] = (int)RGBHistogramData_Ref[i * 4 + c];
            }
        }

        for (int y = 0; y < 128; y++)
        {
            float ph = y / 128f;
            for (int x = 0; x < 256; x++)
            {
                float red = (_channelSplit == 0 || _channelSplit == 1) && (rgbCount[0, x/2] / scale >= ph) ? 1 : 0;
                float green = (_channelSplit == 0 || _channelSplit == 2) && (rgbCount[1, x/2] / scale >= ph) ? 1 : 0;
                float blue = (_channelSplit == 0 || _channelSplit == 3) && (rgbCount[2, x/2] / scale >= ph) ? 1 : 0;
                colors[x + y * 256] = new Color(red, green, blue);
            }
        }

        OutputTexture_Ref.SetPixels(colors);
        OutputTexture_Ref.Apply();
    }


    void OnDestroy()
    {
        if (_histogramBuffer != null)
        {
            _histogramBuffer.Release();
            _histogramBuffer = null;
        }
        if (_histogramBuffer_Ref != null)
        {
            _histogramBuffer_Ref.Release();
            _histogramBuffer_Ref = null;
        }
        if (InputTexture != null)
        {
            Destroy(InputTexture);
        }
        if(_referenceInputTexture_Formatted != null)
        {
            Destroy(_referenceInputTexture_Formatted);
        }
        if (OutputTexture != null)
        {
            Destroy(OutputTexture);
        }
        if (OutputTexture_Ref != null)
        {
            Destroy(OutputTexture_Ref);
        }

    }
}

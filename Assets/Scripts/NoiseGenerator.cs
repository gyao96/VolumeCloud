using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoiseGenerator : MonoBehaviour
{
    public int seed = 0;
    public float frequency = 1.0f;
    public enum TextureChannel { R, G, B, A};
    public enum CloudNoiseType { Shape, Detail }

    [Header("Editor Settings")]
    public CloudNoiseType activeTextureType;
    public TextureChannel activeChannel;
    public bool autoUpdate;
    public bool logComputeTime = true;

    [Header("Noise Settings")]
    public int shapeResolution = 128;
    public int detailResolution = 32;
    public WorleyNoiseSetting[] shapeSettings;
    public WorleyNoiseSetting[] detailSettings;

    public ComputeShader noiseCompute;

    [Header("Viewer Settings")]
    public bool viewerEnabled = true;
    public bool viewerGrayScale = true;
    public bool viewerShowAllChannels;

    [Range(1, 5)]
    public float viewerTileAmount = 1;
    [Range(0, 1)]
    public float viewerSize = 1;

    // Private members
    const int computeThreadGroupSize = 8;
    public const string detailNoiseName = "DetailNoise";
    public const string shapeNoiseName = "ShapeNoise";

    List<ComputeBuffer> buffersToRelease;
    [SerializeField, HideInInspector]
    public RenderTexture shapeTexture;
    [SerializeField, HideInInspector]
    public RenderTexture detailTexture;
    private bool needUpdate = true;
    private TextureChannel generatedChannel;
    private CloudNoiseType generatedType;

    public bool UpdateNoise()
    {
        detailResolution = Mathf.Max(1, detailResolution);
        shapeResolution = Mathf.Max(1, shapeResolution);
        CreateTexture(ref shapeTexture, shapeResolution, shapeNoiseName);
        CreateTexture(ref detailTexture, detailResolution, detailNoiseName);

        WorleyNoiseSetting activeSetting = ActiveSetting;

        if (SettingChanged())
        {
            needUpdate = true;
        }

        if (!needUpdate || !noiseCompute || !activeSetting)
        {
            return false;
        }
        needUpdate = false;
        generatedChannel = activeChannel;
        generatedType = activeTextureType;
        var timer = System.Diagnostics.Stopwatch.StartNew ();
        buffersToRelease = new List<ComputeBuffer>();
        int activeTextureResolution = ActiveTexture.width;
        // Init Noise Compute Parameters
        noiseCompute.SetFloat("persistence", activeSetting.persistence);
        noiseCompute.SetInt("resolution", activeTextureResolution);
        noiseCompute.SetVector("channelMask", ChannelMask);
        // Set noise gen kernel data:
        ComputeBuffer minMaxBuffer = CreateBuffer(new int[] { int.MaxValue, 0 }, sizeof(int), "minMax", 0);
        UpdateWorley(ActiveSetting);
        noiseCompute.SetTexture(0, "Result", ActiveTexture);
        // Dispatch noise gen kernel
        int numThreadGroups = Mathf.CeilToInt(activeTextureResolution / (float)computeThreadGroupSize);
        noiseCompute.Dispatch(0, numThreadGroups, numThreadGroups, numThreadGroups);
        // Set normalization kernel data:
        noiseCompute.SetBuffer(1, "minMax", minMaxBuffer);
        noiseCompute.SetTexture(1, "Result", ActiveTexture);
        // Dispatch normalization kernel
        noiseCompute.Dispatch(1, numThreadGroups, numThreadGroups, numThreadGroups);
        if (logComputeTime)
        {
            // Get minmax data just to force main thread to wait until compute shaders are finished.
            // This allows us to measure the execution time.
            var minMax = new int[2];
            minMaxBuffer.GetData(minMax);
            Debug.Log($"Noise Generation: {timer.ElapsedMilliseconds}ms");
        }
        // Release buffers
        foreach (var buffer in buffersToRelease)
        {
            buffer.Release();
        }
        return true;
    }
    void UpdateWorley(WorleyNoiseSetting setting)
    {
        var prng = new System.Random(setting.seed);
        CreateWorleyPointsBuffer(prng, setting.numDivisionsA, "pointsA");
        CreateWorleyPointsBuffer(prng, setting.numDivisionsB, "pointsB");
        CreateWorleyPointsBuffer(prng, setting.numDivisionsC, "pointsC");

        noiseCompute.SetInt("numCellsA", setting.numDivisionsA);
        noiseCompute.SetInt("numCellsB", setting.numDivisionsB);
        noiseCompute.SetInt("numCellsC", setting.numDivisionsC);
        noiseCompute.SetBool("invertNoise", setting.invert);
        noiseCompute.SetInt("tile", setting.tile);

    }
    void CreateWorleyPointsBuffer(System.Random prng, int numCellsPerAxis, string bufferName)
    {
        var points = new Vector3[numCellsPerAxis * numCellsPerAxis * numCellsPerAxis];
        float cellSize = 1f / numCellsPerAxis;

        for (int x = 0; x < numCellsPerAxis; x++)
        {
            for (int y = 0; y < numCellsPerAxis; y++)
            {
                for (int z = 0; z < numCellsPerAxis; z++)
                {
                    float randomX = (float)prng.NextDouble();
                    float randomY = (float)prng.NextDouble();
                    float randomZ = (float)prng.NextDouble();
                    Vector3 randomOffset = new Vector3(randomX, randomY, randomZ) * cellSize;
                    Vector3 cellCorner = new Vector3(x, y, z) * cellSize;

                    int index = x + numCellsPerAxis * (y + z * numCellsPerAxis);
                    points[index] = cellCorner + randomOffset;
                }
            }
        }

        CreateBuffer(points, sizeof(float) * 3, bufferName);
    }
    private void CreateTexture(ref RenderTexture texture, int resolution, string name)
    {
        var format = UnityEngine.Experimental.Rendering.GraphicsFormat.R16G16B16A16_UNorm;
        if (texture == null || !texture.IsCreated() || texture.width != resolution || texture.height != resolution || texture.volumeDepth != resolution || texture.graphicsFormat != format)
        {
            // Debug.Log ("Create tex: update noise: " + updateNoise);
            if (texture != null)
            {
                texture.Release();
                texture = null;
            }
            texture = new RenderTexture(resolution, resolution, 0);
            texture.graphicsFormat = format;
            texture.volumeDepth = resolution;
            texture.enableRandomWrite = true;
            texture.dimension = UnityEngine.Rendering.TextureDimension.Tex3D;
            texture.name = name;

            texture.Create();
            // Load(name, texture);
        }
        texture.wrapMode = TextureWrapMode.Repeat;
        texture.filterMode = FilterMode.Bilinear;
    }
    ComputeBuffer CreateBuffer(System.Array data, int stride, string bufferName, int kernel = 0)
    {
        var buffer = new ComputeBuffer(data.Length, stride, ComputeBufferType.Structured);
        buffersToRelease.Add(buffer);
        buffer.SetData(data);
        noiseCompute.SetBuffer(kernel, bufferName, buffer);
        return buffer;
    }
    public WorleyNoiseSetting ActiveSetting
    {
        get
        {
            WorleyNoiseSetting[] settings = (activeTextureType == CloudNoiseType.Shape) ? shapeSettings : detailSettings;
            int activeChannelIndex = (int)activeChannel;
            if (activeChannelIndex >= settings.Length)
            {
                return null;
            }
            return settings[activeChannelIndex];
        }
    }
    public RenderTexture ActiveTexture
    {
        get
        {
            return (activeTextureType == CloudNoiseType.Shape) ? shapeTexture : detailTexture;
        }
    }
    public Vector4 ChannelMask
    {
        get
        {
            Vector4 channelWeight = new Vector4(
                (activeChannel == NoiseGenerator.TextureChannel.R) ? 1 : 0,
                (activeChannel == NoiseGenerator.TextureChannel.G) ? 1 : 0,
                (activeChannel == NoiseGenerator.TextureChannel.B) ? 1 : 0,
                (activeChannel == NoiseGenerator.TextureChannel.A) ? 1 : 0
            );
            return channelWeight;
        }
    }

    public bool SettingChanged()
    {
        return activeTextureType != generatedType || activeChannel != generatedChannel;
    }
}

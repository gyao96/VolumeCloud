using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode, ImageEffectAllowedInSceneView]
public class CloudCamera : MonoBehaviour
{
    public ComputeShader cloudShader;
    public Vector3 cloudTestParams;

    [Header("March Settings")]
    public int numStepsLight = 8;
    public float rayOffsetStrength;
    public Texture2D blueNoise;

    [Header("Base Shape")]
    public float cloudScale = 1;
    public float densityMultiplier = 1;
    public float densityOffset;
    public Vector3 shapeOffset;
    public Vector2 heightOffset;
    public Vector4 shapeNoiseWeights;

    [Header("Detail")]
    public float detailNoiseScale = 10;
    public float detailNoiseWeight = .1f;
    public Vector3 detailNoiseWeights;
    public Vector3 detailOffset;

    [Header("Lighting")]
    public float lightAbsorptionThroughCloud = 1;
    public float lightAbsorptionTowardSun = 1;
    [Range(0, 1)]
    public float darknessThreshold = .2f;
    [Range(0, 1)]
    public float forwardScattering = .83f;
    [Range(0, 1)]
    public float backScattering = .3f;
    [Range(0, 1)]
    public float baseBrightness = .8f;
    [Range(0, 1)]
    public float phaseFactor = .15f;

    [Header("Animation")]
    public float timeScale = 1;
    public float baseSpeed = 1;
    public float detailSpeed = 2;

    [Header("Sky")]
    public Color skyBase;
    public Color skyTint;

    [HideInInspector]
    public Material material;

    private RenderTexture target;
    private Camera cam;
    private Light lightSource;
    private List<ComputeBuffer> buffersToDispose;
    private NoiseGenerator noise;
    private WeatherMap whetherMap;

    void Init()
    {
        cam = Camera.current;
        lightSource = FindObjectOfType<Light>();
        buffersToDispose = new List<ComputeBuffer>();
        noise = FindObjectOfType<NoiseGenerator>();
        whetherMap = FindObjectOfType<WeatherMap>();
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Init();

        noise.UpdateNoise();
        whetherMap.UpdateMap();

        InitRenderTexture();
        CreateScene();
        SetTextures();
        SetParameters();

        cloudShader.SetTexture(0, "Source", source);
        cloudShader.SetTexture(0, "Destination", target);

        int threadGroupsX = Mathf.CeilToInt(cam.pixelWidth / 8.0f);
        int threadGroupsY = Mathf.CeilToInt(cam.pixelHeight / 8.0f);
        cloudShader.Dispatch(0, threadGroupsX, threadGroupsY, 16);

        // Blit the result texture to the screen
        Graphics.Blit(target, destination);

        foreach (var buffer in buffersToDispose)
        {
            buffer.Dispose();
        }
    }

    void CreateScene()
    {
        CloudBox cbox = FindObjectOfType<CloudBox>();

        CloudBoxData[] cboxData = new CloudBoxData[1];
        cboxData[0] = new CloudBoxData()
            {
                position = cbox.Position,
                scale = cbox.Scale,
            };

        ComputeBuffer shapeBuffer = new ComputeBuffer(1, CloudBoxData.GetSize());
        shapeBuffer.SetData(cboxData);
        cloudShader.SetBuffer(0, "cbox", shapeBuffer);

        buffersToDispose.Add(shapeBuffer);
    }

    void SetParameters()
    {
        bool lightIsDirectional = lightSource.type == LightType.Directional;
        cloudShader.SetMatrix("_CameraToWorld", cam.cameraToWorldMatrix);
        cloudShader.SetMatrix("_CameraInverseProjection", cam.projectionMatrix.inverse);
        cloudShader.SetVector("_Light", (lightIsDirectional) ? lightSource.transform.forward : lightSource.transform.position);
        cloudShader.SetBool("positionLight", !lightIsDirectional);
        cloudShader.SetFloat("scale", cloudScale);
        cloudShader.SetFloat("densityMultiplier", densityMultiplier);
        cloudShader.SetFloat("densityOffset", densityOffset);
        cloudShader.SetFloat("lightAbsorptionFromCloud", lightAbsorptionThroughCloud);
        cloudShader.SetFloat("lightAbsorptionTowardSun", lightAbsorptionTowardSun);
        cloudShader.SetFloat("darknessThreshold", darknessThreshold);
        cloudShader.SetVector("params", cloudTestParams);
        cloudShader.SetFloat("rayOffsetStrength", rayOffsetStrength);
        cloudShader.SetFloat("detailNoiseScale", detailNoiseScale);
        cloudShader.SetFloat("detailNoiseWeight", detailNoiseWeight);
        cloudShader.SetVector("shapeOffset", shapeOffset);
        cloudShader.SetVector("detailOffset", detailOffset);
        cloudShader.SetVector("detailWeights", detailNoiseWeights);
        cloudShader.SetVector("shapeNoiseWeights", shapeNoiseWeights);
        cloudShader.SetVector("phaseParams", new Vector4(forwardScattering, backScattering, baseBrightness, phaseFactor));
        cloudShader.SetInt("numStepsLight", numStepsLight);
        cloudShader.SetFloat("timeScale", (Application.isPlaying) ? timeScale : 0);
        cloudShader.SetFloat("baseSpeed", baseSpeed);
        cloudShader.SetFloat("detailSpeed", detailSpeed);
        cloudShader.SetVector("skyBaseColor", skyBase);
        cloudShader.SetVector("skyTintColor", skyTint);
        cloudShader.SetVector("_LightColor", lightSource.color);
    }

    void SetTextures()
    {
        cloudShader.SetTexture(0, "NoiseTex", noise.shapeTexture);
        cloudShader.SetTexture(0, "DetailNoiseTex", noise.detailTexture);
        cloudShader.SetTexture(0, "BlueNoise", blueNoise);
        cloudShader.SetTexture(0, "WhetherMap", whetherMap.weatherMap);
    }

    void InitRenderTexture()
    {
        if (target == null || target.width != cam.pixelWidth || target.height != cam.pixelHeight)
        {
            if (target != null)
            {
                target.Release();
            }
            target = new RenderTexture(cam.pixelWidth, cam.pixelHeight, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
            target.enableRandomWrite = true;
            target.Create();
        }
    }

    struct CloudBoxData
    {
        public Vector3 position;
        public Vector3 scale;

        public static int GetSize()
        {
            return sizeof(float) * 6;
        }
    }
}

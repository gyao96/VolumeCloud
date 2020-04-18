using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode, ImageEffectAllowedInSceneView]
public class CloudCamera : MonoBehaviour
{
    public ComputeShader cloudShader;

    private RenderTexture target;
    private Camera cam;
    private Light lightSource;
    private List<ComputeBuffer> buffersToDispose;

    void Init()
    {
        cam = Camera.current;
        lightSource = FindObjectOfType<Light>();
    }


    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Init();
        buffersToDispose = new List<ComputeBuffer>();

        InitRenderTexture();
        CreateScene();
        SetParameters();

        cloudShader.SetTexture(0, "Source", source);
        cloudShader.SetTexture(0, "Destination", target);

        int threadGroupsX = Mathf.CeilToInt(cam.pixelWidth / 8.0f);
        int threadGroupsY = Mathf.CeilToInt(cam.pixelHeight / 8.0f);
        cloudShader.Dispatch(0, threadGroupsX, threadGroupsY, 1);

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

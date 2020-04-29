using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class NoiseVisualizer : MonoBehaviour
{
    public ComputeShader slicer;
    [Range(0, 1)]
    public float viewerSliceDepth;
    Texture2D tex2D;
    Texture2D[] slices;
    int resolution;
    MeshRenderer renderer;
    NoiseGenerator noise;
    bool needUpdate;
    int activeChannel;
    // Start is called before the first frame update
    void Start()
    {
        noise = FindObjectOfType<NoiseGenerator>();
        noise.UpdateNoise();
        needUpdate = false;
        activeChannel = (int) noise.activeChannel;
        RenderTextureToTex2DSlices(noise.ActiveTexture);
        
        renderer = GetComponent<MeshRenderer>();
        UpdateTexture2D();
        renderer.material.SetTexture("_MainTex", tex2D);
    }

    // Update is called once per frame
    void Update()
    {
        if (activeChannel != (int)noise.activeChannel)
        {
            noise.UpdateNoise();
            RenderTextureToTex2DSlices(noise.ActiveTexture);
            activeChannel = (int) noise.activeChannel;
        }
        UpdateTexture2D();
        renderer.material.SetTexture("_MainTex", tex2D);
    }


    private void UpdateTexture2D()
    {
        int vis_slice_index = Mathf.FloorToInt(resolution * viewerSliceDepth);
        tex2D = slices[vis_slice_index];
    }


    private void RenderTextureToTex2DSlices(RenderTexture source)
    {
        resolution = source.width;
        const int threadGroupSize = 32;
        slices = new Texture2D[resolution];
        slicer.SetInt("resolution", resolution);
        slicer.SetTexture(0, "volumeTexture", source);

        for (int layer = 0; layer < resolution; layer++)
        {
            var slice = new RenderTexture(resolution, resolution, 0);
            slice.dimension = UnityEngine.Rendering.TextureDimension.Tex2D;
            slice.enableRandomWrite = true;
            slice.Create();

            slicer.SetTexture(0, "slice", slice);
            slicer.SetInt("layer", layer);
            int numThreadGroups = Mathf.CeilToInt(resolution / (float)threadGroupSize);
            slicer.Dispatch(0, numThreadGroups, numThreadGroups, 1);

            slices[layer] = ConvertFromRenderTexture(slice);

        }

    }

    Texture2D ConvertFromRenderTexture(RenderTexture rt)
    {
        Texture2D output = new Texture2D(rt.width, rt.height);
        RenderTexture.active = rt;
        output.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        output.Apply();
        return output;
    }
}

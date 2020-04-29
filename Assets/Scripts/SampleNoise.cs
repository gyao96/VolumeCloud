using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralNoiseProject
{
    [ExecuteInEditMode]
    public class SampleNoise : MonoBehaviour
    {
        // Start is called before the first frame update
        Texture3D texture3;
        Texture2D texture2;
        MeshRenderer renderer;
        public int seed = 0;
        public int octaves = 4;
        public float frequency = 1.0f;
        int length = 128;
        [SerializeField]
        [Range(0, 127)]
        public int depth = 28;
        void Start()
        {
            renderer = GetComponent<MeshRenderer>();
            texture3 = new Texture3D(length, length, length, TextureFormat.RGBA32, false);
            texture2 = new Texture2D(length, length, TextureFormat.RGBA32, false);
            INoise noise = new WorleyNoise(seed, 20, 1.0f);
            FractalNoise fractal = new FractalNoise(noise, octaves, frequency);
            SetNoiseTexture3D(fractal);
            UpdateTexture2D();
            renderer.material.SetTexture("_MainTex", texture2);
        }

        void SetNoiseTexture3D(FractalNoise fractal)
        {
            for (int i = 0; i < length; i++)
            {
                for (int j = 0; j < length; j++)
                {
                    for (int k = 0; k < length; k++)
                    {
                        float fx = i / (length - 1.0f);
                        float fy = j / (length - 1.0f);
                        float fz = k / (length - 1.0f);
                        float n = fractal.Sample3D(fx, fy, fz);
                        texture3.SetPixel(i, j, k, new Color(n, n, n, 1));
                    }
                }
            }
        }

        void UpdateTexture2D()
        {
            for (int i = 0; i < length; i++)
            {
                for (int j = 0; j < length; j++)
                {
                    texture2.SetPixel(i, j, texture3.GetPixel(i, j, depth));
                }
            }
            texture2.Apply();
        }

        // Update is called once per frame
        void Update()
        {
            UpdateTexture2D();
            renderer.material.SetTexture("_MainTex", texture2);
        }
    }
}


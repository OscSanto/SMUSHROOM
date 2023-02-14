using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public static class Noise {
    public static float[,] generateNoiseMap(int mapChunkSize, int mapChunkSize,int seed, float scale, int octaves, float persistance, float lacunarity, Vector2 offset){
        float[,] noiseMap = new float[mapChunkSize, mapChunkSize];

        System.Random prng = new System.Random(seed);
        Vector2[] octaveOffsets = new Vector2[octaves];

        for (int i = 0; i < octaves; i++) {
            float offsetX = prng.Next(-100000, 100000) + offset.x;
            float offsetY = prng.Next(-100000, 100000) + offset.y;
            octaveOffsets[i] = new Vector2(offsetX, offsetY);

        }
        if (scale <= 0) {
            scale = 0.0001f;
        }

        float maxNoiseWidth = float.MinValue;
        float minNoiseHeight = float.MaxValue;

        float halfWidth = mapChunkSize / 2f;
        float halfHeight = mapChunkSize / 2f;

        for (int y = 0; y < mapChunkSize; y++) {
            for (int x = 0; x < mapChunkSize; x++) {
                float amplitude = 1;
                float frequency = 1;
                float noiseHeight = 0;
                for (int i = 0; i < octaves; i++) {

                    float sampleX = (x - halfWidth) / scale * frequency + octaveOffsets [i].x;
                    float sampleY = (y - halfHeight)  / scale * frequency + octaveOffsets [i].y;

                    float perlineValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 -1; //inner function range is 0,1. With math; range from -1, 1
                    noiseHeight += perlineValue * amplitude;

                    amplitude *= persistance;
                    frequency *= lacunarity;
                }

                if (noiseHeight > maxNoiseWidth) {
                    maxNoiseWidth = noiseHeight;
                }else if (noiseHeight < minNoiseHeight) {
                    minNoiseHeight = noiseHeight;
                }
                noiseMap[x,y] = noiseHeight;
                
            }
        }

        //normalize the noisemap
        for (int y = 0; y < mapChunkSize; y++) {
            for (int x = 0; x < mapChunkSize; x++) {
                noiseMap[x, y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseWidth, noiseMap[x, y]); //max the noisemap range between 0-1
            }

        }

        return noiseMap;
    }
}
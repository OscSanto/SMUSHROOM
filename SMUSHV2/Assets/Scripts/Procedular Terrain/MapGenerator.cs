using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class MapGenerator : MonoBehaviour {
    public enum DrawMode {NoiseMap, ColorMap, Mesh}
    public DrawMode drawMode;

    private const int mapChunkSize = 241;
    [Range(0,6)]
    public int levelOfDetail; //loD
    public float noiseScale;

    public bool autoUpdate;

    public int seed;
    public Vector2 offset;
    public float meshHeightMultiplier;
    public AnimationCurve meshHeightCurve;

    [Range(0,1)]
    public float persistance;
    public float lacunarity;
    public int octaves;

    public TerrainType[] regions;

    public void GenerateMap(){
        float[,] noiseMap = Noise.generateNoiseMap(mapChunkSize, mapChunkSize, seed, noiseScale, octaves, persistance,lacunarity, offset);

        Color[] colormap = new Color [mapChunkSize * mapChunkSize];
        //episode E04: colors
        //This will divide/layer the map's height by the height of the terrain type.
        for (int y = 0; y < mapChunkSize; y++) {
            for (int x = 0; x < mapChunkSize; x++) {
                float currentHeight = noiseMap[x,y];
                for (int i = 0; i < regions.Length; i++) {
                    if (currentHeight <= regions[i].height) {
                        colormap[y * mapChunkSize + x] = regions[i].color;
                        break;
                    }
                }
            }
        }
        MapDisplay display = FindObjectOfType<MapDisplay>();

        if (drawMode == DrawMode.NoiseMap) {
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(noiseMap));
        }else if (drawMode == DrawMode.ColorMap) {
            display.DrawTexture(TextureGenerator.TextureFromColorMap(colormap, mapChunkSize,mapChunkSize));
        }else if (drawMode == DrawMode.Mesh) {
            display.DrawMesh(MeshGenerator.GeneratorTerrainMesh(noiseMap, meshHeightMultiplier, meshHeightCurve,levelOfDetail), TextureGenerator.TextureFromColorMap(colormap, mapChunkSize,mapChunkSize));
        }
    }
    void OnValidate(){
     
        if (lacunarity < 1) { 
            lacunarity = 1;
        }

        if (octaves < 0) {
            octaves = 0;
        }
    }
}
[System.Serializable]
public struct TerrainType {
    public string name; //label of terrain
    public float height;
    public Color color;
}

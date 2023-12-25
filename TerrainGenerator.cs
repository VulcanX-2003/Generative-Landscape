using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    public int width = 256;
    public int height = 256;
    public float scale = 20f;
    public float terrainHeight = 10f;
    public int octaves = 4;
    [Range(0, 1)]
    public float persistence = 0.5f;
    public float lacunarity = 2f;
    public int erosionIterations = 5;
    public float erosionStrength = 0.1f;

    void Start()
    {
        GenerateTerrain();
    }

    void GenerateTerrain()
    {
        Terrain terrain = GetComponent<Terrain>();
        terrain.terrainData = GenerateTerrain(terrain.terrainData);
    }

    TerrainData GenerateTerrain(TerrainData terrainData)
    {
        terrainData.heightmapResolution = width + 1;
        terrainData.size = new Vector3(width, terrainHeight, height);
        terrainData.SetHeights(0, 0, GenerateHeights(terrainData.heightmapResolution));
        ErodeTerrain(terrainData, erosionIterations, erosionStrength);
        SmoothTerrain(terrainData);
        return terrainData;
    }

    float[,] GenerateHeights(int resolution)
    {
        float[,] heights = new float[resolution, resolution];
        Vector2 offset = new Vector2(Random.Range(0f, 9999f), Random.Range(0f, 9999f));

        for (int x = 0; x < resolution; x++)
        {
            for (int y = 0; y < resolution; y++)
            {
                float amplitude = 1;
                float frequency = 1;
                float noiseHeight = 0;

                for (int i = 0; i < octaves; i++)
                {
                    float xCoord = (float)x / resolution * scale * frequency + offset.x;
                    float yCoord = (float)y / resolution * scale * frequency + offset.y;

                    float perlinValue = Mathf.PerlinNoise(xCoord, yCoord);
                    noiseHeight += perlinValue * amplitude;

                    amplitude *= persistence;
                    frequency *= lacunarity;
                }

                heights[x, y] = noiseHeight;
            }
        }

        return heights;
    }

    void ErodeTerrain(TerrainData terrainData, int iterations, float strength)
    {
        for (int i = 0; i < iterations; i++)
        {
            terrainData = Erode(terrainData, strength);
        }
    }

    TerrainData Erode(TerrainData terrainData, float strength)
    {
        int resolution = terrainData.heightmapResolution;
        float[,] heights = terrainData.GetHeights(0, 0, resolution, resolution);

        for (int x = 1; x < resolution - 1; x++)
        {
            for (int y = 1; y < resolution - 1; y++)
            {
                float averageHeight = (heights[x - 1, y] + heights[x + 1, y] + heights[x, y - 1] + heights[x, y + 1]) / 4f;
                float delta = averageHeight - heights[x, y];
                heights[x, y] = Mathf.Clamp01(heights[x, y] + delta * strength);
            }
        }

        terrainData.SetHeights(0, 0, heights);
        return terrainData;
    }

    void SmoothTerrain(TerrainData terrainData)
    {
        int smoothIterations = 5;

        for (int i = 0; i < smoothIterations; i++)
        {
            terrainData = Smooth(terrainData);
        }
    }

    TerrainData Smooth(TerrainData terrainData)
    {
        int resolution = terrainData.heightmapResolution;
        float[,] heights = terrainData.GetHeights(0, 0, resolution, resolution);

        for (int x = 1; x < resolution - 1; x++)
        {
            for (int y = 1; y < resolution - 1; y++)
            {
                float averageHeight = (heights[x - 1, y] + heights[x + 1, y] + heights[x, y - 1] + heights[x, y + 1]) / 4f;
                heights[x, y] = Mathf.Lerp(heights[x, y], averageHeight, 0.5f);
            }
        }

        terrainData.SetHeights(0, 0, heights);
        return terrainData;
    }
}

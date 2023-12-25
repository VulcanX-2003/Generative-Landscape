using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class ChunkLoader : MonoBehaviour
{
    public GameObject chunkPrefab;
    public int chunkSize = 10;
    public int viewDistance = 5;

    private Transform playerTransform;
    private Transform lastPlayerChunk;

    private void Start()
    {
        playerTransform = Camera.main.transform; // Assuming the main camera represents the player
        StartCoroutine(GenerateInitialTerrain());
    }

    private IEnumerator GenerateInitialTerrain()
    {
        for (int x = -viewDistance; x <= viewDistance; x++)
        {
            for (int z = -viewDistance; z <= viewDistance; z++)
            {
                Vector3 chunkPosition = new Vector3(x * chunkSize, 0f, z * chunkSize);
                InstantiateChunk(chunkPosition);
                yield return null; // Add a small delay between chunk instantiation to prevent freezing
            }
        }
    }

    private void Update()
    {
        Vector3 playerPosition = playerTransform.position;
        Vector3 playerChunk = new Vector3(
            Mathf.FloorToInt(playerPosition.x / chunkSize) * chunkSize,
            0f,
            Mathf.FloorToInt(playerPosition.z / chunkSize) * chunkSize
        );

        if (playerChunk != lastPlayerChunk.position)
        {
            LoadUnloadChunks(playerChunk);
            lastPlayerChunk.position = playerChunk;
        }
    }

    private void LoadUnloadChunks(Vector3 playerChunk)
    {
        JobHandle jobHandle = new JobHandle();

        NativeArray<Vector3> chunkPositions = new NativeArray<Vector3>((2 * viewDistance + 1) * (2 * viewDistance + 1), Allocator.TempJob);

        int index = 0;
        for (int x = -viewDistance; x <= viewDistance; x++)
        {
            for (int z = -viewDistance; z <= viewDistance; z++)
            {
                chunkPositions[index] = new Vector3(
                    (x + Mathf.FloorToInt(playerChunk.x / chunkSize)) * chunkSize,
                    0f,
                    (z + Mathf.FloorToInt(playerChunk.z / chunkSize)) * chunkSize
                );

                index++;
            }
        }

        var loadUnloadJob = new LoadUnloadChunksJob
        {
            chunkPrefab = chunkPrefab,
            chunkSize = chunkSize,
            chunkPositions = chunkPositions,
            playerPosition = playerTransform.position
        };

        jobHandle = loadUnloadJob.Schedule(index, 64);
        jobHandle.Complete();

        chunkPositions.Dispose();
    }

    private void InstantiateChunk(Vector3 position)
    {
        Instantiate(chunkPrefab, position, Quaternion.identity);
    }

    [Burst.BurstCompile]
    private struct LoadUnloadChunksJob : IJobParallelFor
    {
        public GameObject chunkPrefab;
        public int chunkSize;
        public NativeArray<Vector3> chunkPositions;
        public Vector3 playerPosition;

        public void Execute(int index)
        {
            Vector3 chunkPosition = chunkPositions[index];

            if (Vector3.Distance(chunkPosition, playerPosition) > chunkSize * (viewDistance + 1))
            {
                UnloadChunk(chunkPosition);
            }
            else
            {
                InstantiateChunk(chunkPosition);
            }
        }

        private void InstantiateChunk(Vector3 position)
        {
            Instantiate(chunkPrefab, position, Quaternion.identity);
        }

        private void UnloadChunk(Vector3 position)
        {
            // Unload or destroy the chunk at the specified position
            // You may want to implement a more sophisticated unloading mechanism based on your needs
            // For example, you could pool and reuse chunks instead of destroying them
            Destroy(GameObject.Find("Chunk(Clone)"));
        }
    }
}

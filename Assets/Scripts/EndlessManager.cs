using UnityEngine;
using System.Collections.Generic;

public class EndlessManager : MonoBehaviour
{
    public GameObject chunkPrefab;
    public int chunkSize = 100;
    public Transform player;
    
    
    
    public int viewDistance = 2; 

    // dictionary to track existing chunks to avoid adn help remove them later
    // Key: Coordinate ("0,1"), Value: The GameObject named off of coordiante stystem
    Dictionary<Vector2, GameObject> terrainChunks = new Dictionary<Vector2, GameObject>();
    List<GameObject> activeChunks = new List<GameObject>();

    void Update()
    {
        UpdateVisibleChunks();
    }

    void UpdateVisibleChunks()
    {
        // fidning out which chunk the player is standing on
        int playerChunkX = Mathf.RoundToInt(player.position.x / chunkSize);
        int playerChunkZ = Mathf.RoundToInt(player.position.z / chunkSize);

        // looping through all surrounding tiles
        for (int xOffset = -viewDistance; xOffset <= viewDistance; xOffset++)
        {
            for (int zOffset = -viewDistance; zOffset <= viewDistance; zOffset++)
            {
                Vector2 viewedChunkCoord = new Vector2(playerChunkX + xOffset, playerChunkZ + zOffset);

                if (!terrainChunks.ContainsKey(viewedChunkCoord))
                {
                    SpawnChunk(viewedChunkCoord);
                }
            }
        }
        
        CleanupOldChunks(playerChunkX, playerChunkZ);
    }

    void SpawnChunk(Vector2 coord)
    {
        Vector3 position = new Vector3(coord.x * chunkSize, 0, coord.y * chunkSize);
        GameObject newChunk = Instantiate(chunkPrefab, position, Quaternion.identity);
        newChunk.name = "Chunk_" + coord.x + "_" + coord.y;
        newChunk.transform.parent = this.transform;

        
        MeshGenerator gen = newChunk.GetComponent<MeshGenerator>();
        gen.offset = new Vector2(coord.x * chunkSize, coord.y * chunkSize); 
        gen.Init();

      
        terrainChunks.Add(coord, newChunk);
    }

    void CleanupOldChunks(int playerX, int playerZ)
    {
        
        List<Vector2> chunksToRemove = new List<Vector2>();

        foreach (var item in terrainChunks)
        {
            float dist = Vector2.Distance(item.Key, new Vector2(playerX, playerZ));
            if (dist > viewDistance + 2) 
            {
                chunksToRemove.Add(item.Key);
            }
        }

        // Execute destruction
        foreach (var key in chunksToRemove)
        {
            GameObject chunk = terrainChunks[key];
            terrainChunks.Remove(key);
            Destroy(chunk);
        }
    }
}
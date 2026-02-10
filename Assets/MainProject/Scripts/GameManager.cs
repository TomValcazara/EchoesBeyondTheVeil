using UnityEngine;

/// <summary>
/// GameManager is responsible for:
/// - Holding references to bush prefabs
/// - Randomly spawning bushes across the terrain
/// - Making sure bushes sit correctly on the terrain surface
/// </summary>
public class GameManager : MonoBehaviour
{
    // -------------------------
    // REFERENCES
    // -------------------------

    [Header("Terrain")]
    // Reference to the Terrain object in the scene
    // Used to get size and height information
    public Terrain terrain;

    [Header("Bush Prefabs")]
    // Array of bush prefabs that can be spawned
    // You will assign 5 different bush prefabs in the Inspector
    public GameObject[] bushPrefabs;

    // -------------------------
    // SPAWN SETTINGS
    // -------------------------

    [Header("Spawn Settings")]
    // Total number of bushes to spawn
    public int numberOfBushes = 50;

    // Optional vertical offset
    // Useful if the prefab pivot is not exactly at ground level
    public float yOffset = 0f;

    // Cached terrain data for performance and clarity
    private Vector3 terrainSize;
    private Vector3 terrainPos;

    // -------------------------
    // UNITY LIFECYCLE
    // -------------------------

    void Start()
    {
        // Safety check to avoid null reference errors
        if (terrain == null || bushPrefabs.Length == 0)
        {
            Debug.LogError("GameManager: Missing terrain reference or bush prefabs!");
            return;
        }

        // Store terrain size (width, height, length)
        // For your case: 200 x 600 x 200
        terrainSize = terrain.terrainData.size;
        Debug.Log("terrainSize:"+terrainSize);
        // Store terrain world position
        // Important because your terrain is offset (-100, 0, -100)
        terrainPos = terrain.transform.position;
        Debug.Log("terrainPos:"+terrainPos);
        // Start spawning bushes
        SpawnBushes();
    }

    // -------------------------
    // SPAWNING LOGIC
    // -------------------------

    /// <summary>
    /// Spawns bushes at random positions across the terrain
    /// </summary>
    void SpawnBushes()
    {
        Vector3 terrainSize = terrain.terrainData.size;
        Vector3 terrainPos = terrain.transform.position;

        // Calculate terrain center in world space
        float centerX = terrainPos.x + terrainSize.x * 0.5f;
        float centerZ = terrainPos.z + terrainSize.z * 0.5f;

        float halfX = terrainSize.x * 0.5f;
        float halfZ = terrainSize.z * 0.5f;

        for (int i = 0; i < numberOfBushes; i++)
        {
            // Random position around terrain CENTER
            float x = Random.Range(centerX - halfX, centerX + halfX);
            float z = Random.Range(centerZ - halfZ, centerZ + halfZ);

            Vector3 position = new Vector3(x, 0f, z);

            // Snap to terrain height
            float y = terrain.SampleHeight(position) + terrainPos.y;
            position.y = y + yOffset;

            GameObject prefab =
                bushPrefabs[Random.Range(0, bushPrefabs.Length)];

            Instantiate(prefab, position, Quaternion.identity, transform);
        }
    }
}

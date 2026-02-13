using UnityEngine;
using System.Collections.Generic;

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

    [Header("Noise Bush Settings")]
    public int numberOfNoiseBushes = 6; 
    
    private List<BushInteractable> allBushes = new List<BushInteractable>();
    private List<BushInteractable> noiseBushes = new List<BushInteractable>();
    private int currentNoiseIndex = 0;

    [Header("Walkable Area")]
    public float walkableHalfSize = 100f; // 200x200 area

    private string[] story;

    [Header("Hell Gate")]
    public GameObject hellGatePrefab;

    // [Header("Hell Gate")]
    // public GameObject hellGatePrefab;
    // public Transform player;
    // public float minGateDistanceFromPlayer = 40f;
    // public float walkableRadius = 100f;

    public string GetStoryText(int index)
    {
        if (index >= 0 && index < story.Length)
            return story[index];

        return "";
    }

    bool IsInsideWalkableArea(Vector3 position)
    {
        return
            position.x >= -walkableHalfSize &&
            position.x <=  walkableHalfSize &&
            position.z >= -walkableHalfSize &&
            position.z <=  walkableHalfSize;
    }
    // -------------------------
    // UNITY LIFECYCLE
    // -------------------------

    void Start()
    {
        Debug.Log("Toms game started!");

        story = new string[6];
        story[0] = "The forest whispered tonight - a new arrival had drifted in, soft as doubt, pale as confession.\nThe sleeping ones stirred, their sighs weaving beneath the roots.";
        story[1] = "He took two slices, one murmured, their voice damp with envy.\nTwo! When one was enough for salvation.";
        story[2] = "Another sighed. It’s always the gentle ones who wander too far - loving too much, laughing too loud, wanting too freely.\nTheir pity dripped like prayer wax.";
        story[3] = "The fresh soul said nothing.\nHe remembered warmth, and hands that once fit his - forbidden, fleeting, holy.";
        story[4] = "Sin, whispered the wind. Sin for loving, sin for being, sin for not hiding.\nAnd the in-between bloomed with forgiveness no one believed in.";
        story[5] = "When dawn came, the sleeping ones fell quiet — ashamed, perhaps, or simply empty.\nThe fresh soul drifted on, too bright for limbo, too tender for The Red Garden Below.";

        // Safety check to avoid null reference errors
        if (terrain == null || bushPrefabs.Length == 0)
        {
            Debug.LogError("GameManager: Missing terrain reference or bush prefabs!");
            return;
        }

        // Store terrain size (width, height, length)
        // For your case: 200 x 600 x 200
        terrainSize = terrain.terrainData.size;
        //Debug.Log("terrainSize:"+terrainSize);
        // Store terrain world position
        // Important because your terrain is offset (-100, 0, -100)
        terrainPos = terrain.transform.position;
        //Debug.Log("terrainPos:"+terrainPos);
        // Start spawning bushes
        SpawnBushes();

        SelectNoiseBushes();
        ActivateNextNoiseBush();

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
            float radius = 100f; // same as movement boundary
            Vector2 randomCircle = Random.insideUnitCircle * radius;
            float x = centerX + randomCircle.x;
            float z = centerZ + randomCircle.y;

            Vector3 position = new Vector3(x, 0f, z);

            // Snap to terrain height
            float y = terrain.SampleHeight(position) + terrainPos.y;
            position.y = y + yOffset;
            
            GameObject prefab =
                bushPrefabs[Random.Range(0, bushPrefabs.Length)];

            //Instantiate(prefab, position, Quaternion.identity, transform);
            GameObject bush = Instantiate(prefab, position, Quaternion.identity, transform);
            BushInteractable interactable = bush.GetComponent<BushInteractable>();
            if (interactable != null)
            {
                allBushes.Add(interactable);
            }

        }
    }

    void SelectNoiseBushes()
    {
        noiseBushes.Clear();

        // Only bushes inside walkable area are eligible
        List<BushInteractable> eligibleBushes = new List<BushInteractable>();

        foreach (var bush in allBushes)
        {
            if (IsInsideWalkableArea(bush.transform.position))
            {
                eligibleBushes.Add(bush);
                // DEBUG: visualize eligible bushes
                Debug.DrawLine(
                    Vector3.zero,
                    bush.transform.position,
                    Color.red,
                    100f
                );
            }
        }

        if (eligibleBushes.Count == 0)
        {
            Debug.LogWarning("No bushes inside walkable area!");
            return;
        }

        // Shuffle eligible bushes
        for (int i = 0; i < eligibleBushes.Count; i++)
        {
            int randomIndex = Random.Range(i, eligibleBushes.Count);
            var temp = eligibleBushes[i];
            eligibleBushes[i] = eligibleBushes[randomIndex];
            eligibleBushes[randomIndex] = temp;
        }

        // Pick first N as noise bushes
        for (int i = 0; i < numberOfNoiseBushes && i < eligibleBushes.Count; i++)
        {
            noiseBushes.Add(eligibleBushes[i]);
        }
    }


    void ActivateNextNoiseBush()
    {
        //Debug.Log("Inside the Trying to Activate a noise bush");

        if (currentNoiseIndex >= noiseBushes.Count)
        {
            //Debug.Log("All noise bushes completed!");
            return;
        }

        //Debug.Log("currentNoiseIndex:"+currentNoiseIndex);
        //noiseBushes[currentNoiseIndex].ActivateNoise();
        var bush = noiseBushes[currentNoiseIndex];
        bush.SetLoreIndex(currentNoiseIndex);
        bush.ActivateNoise();

    }

    public void OnNoiseBushCompleted(BushInteractable bush)
    {
        currentNoiseIndex++;

        if (currentNoiseIndex >= noiseBushes.Count)
        {
            SpawnHellGate();
        }
        else
        {
            ActivateNextNoiseBush();
        }
    }

    void SpawnHellGate()
    {
        // Vector3 center = Vector3.zero; // use your real center if different
        // Vector3 spawnPos = Vector3.zero;

        // bool validPosition = false;
        // int safety = 0;

        // while (!validPosition && safety < 50)
        // {
        //     Vector2 randomCircle = Random.insideUnitCircle * walkableRadius;
        //     Vector3 candidate = new Vector3(randomCircle.x, 0f, randomCircle.y);

        //     float distanceToPlayer = Vector3.Distance(candidate, player.position);

        //     if (distanceToPlayer >= minGateDistanceFromPlayer)
        //     {
        //         spawnPos = candidate;
        //         validPosition = true;
        //     }

        //     safety++;
        // }

        // // Snap to terrain height
        // float y = terrain.SampleHeight(spawnPos) + terrain.transform.position.y;
        // spawnPos.y = y;

        // Instantiate(hellGatePrefab, spawnPos, Quaternion.identity);
        hellGatePrefab.SetActive(true);
    }



}

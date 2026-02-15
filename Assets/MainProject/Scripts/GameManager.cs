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

    [Header("Websocket Client")]
    public WebSocketClientExample webSocketClient;

    private bool cheatingActive = false;

    [Header("Spawn Restrictions")]
    public float centerExclusionRadius = 30f;   // No bushes near center
    public float minBushSpacing = 2f;           // Minimum distance between bushes
    public int maxSpawnAttempts = 20;           // Avoid infinite loops

    [Header("Environment Assets")]
    public GameObject[] environmentPrefabs;
    public int numberOfEnvironmentAssets = 30;
    public float minEnvironmentSpacing = 2f;

    private List<Vector3> occupiedPositions = new List<Vector3>();

    public string GetStoryText(int index)
    {
        if (index >= 0 && index < story.Length)
            return story[index];

        return "";
    }

    [SerializeField] private GameObject mushroomsPrefab;
    [SerializeField] private GameObject lorePanelPrefab;

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

        // if (webSocketClient != null) //Resets the LEDs to be off
        // {
        //     webSocketClient.SendYellowLEDOFF();
        //     webSocketClient.SendGreenLEDOFF();
        // }

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

        SpawnEnvironmentAssets();

        SelectNoiseBushes();
        ActivateNextNoiseBush();

        PreWarmLoreAssets();
        PreWarmHellGate();
        
    }

    void PreWarmLoreAssets()
    {
        Vector3 hidden = new Vector3(0, -1000, 0);

        Prewarm(mushroomsPrefab, hidden);
        Prewarm(lorePanelPrefab, hidden);
    }

    void Prewarm(GameObject prefab, Vector3 pos)
    {
        if (prefab == null) return;

        GameObject temp = Instantiate(prefab, pos, Quaternion.identity);
        Destroy(temp);
    }

    void PreWarmHellGate()
    {
        if (hellGatePrefab == null) return;

        Vector3 hidden = new Vector3(0, -1000, 0);

        GameObject temp = Instantiate(hellGatePrefab, hidden, Quaternion.identity);

        // Force light initialization
        Light light = temp.GetComponentInChildren<Light>();
        if (light != null)
            light.enabled = true;

        // Force audio initialization
        AudioSource[] audios = temp.GetComponentsInChildren<AudioSource>();
        foreach (var a in audios)
        {
            a.Play();
            a.Stop();
        }

        // Force canvas initialization
        CanvasGroup[] canvases = temp.GetComponentsInChildren<CanvasGroup>();
        foreach (var cg in canvases)
        {
            cg.alpha = 1f;
        }

        Destroy(temp);
    }

    public void SetCheatingState(bool isCheating)
    {
        cheatingActive = isCheating;

        UpdateCurrentBushVisual();
    }

    void UpdateCurrentBushVisual()
    {
        if (currentNoiseIndex < noiseBushes.Count)
        {
            var bush = noiseBushes[currentNoiseIndex];
            bush.SetCheatingVisual(cheatingActive);
        }
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
            bool positionFound = false;
            int attempts = 0;

            while (!positionFound && attempts < maxSpawnAttempts)
            {
                attempts++;

                float radius = walkableHalfSize;
                Vector2 randomCircle = Random.insideUnitCircle * radius;

                Vector3 position = new Vector3(
                    randomCircle.x,
                    0f,
                    randomCircle.y
                );

                //1. Avoid center area (hellgate zone)
                if (Vector3.Distance(position, Vector3.zero) < centerExclusionRadius)
                    continue;

                //2. Avoid overlap with other bushes
                bool tooClose = false;
                foreach (var bush in allBushes)
                {
                    if (Vector3.Distance(position, bush.transform.position) < minBushSpacing)
                    {
                        tooClose = true;
                        break;
                    }
                }

                if (tooClose)
                    continue;

                //Snap to terrain
                float y = terrain.SampleHeight(position) + terrainPos.y;
                position.y = y + yOffset;

                GameObject prefab = bushPrefabs[Random.Range(0, bushPrefabs.Length)];

                Quaternion randomRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f); // Random Rotation

                GameObject _bush = Instantiate(prefab, position, randomRotation, transform);

                BushInteractable interactable = _bush.GetComponent<BushInteractable>();
                if (interactable != null)
                {
                    allBushes.Add(interactable);
                    occupiedPositions.Add(position);
                }

                positionFound = true;
            }
        }

    }

    void SpawnEnvironmentAssets()
    {
        for (int i = 0; i < numberOfEnvironmentAssets; i++)
        {
            bool positionFound = false;
            int attempts = 0;

            while (!positionFound && attempts < maxSpawnAttempts)
            {
                attempts++;

                float radius = walkableHalfSize;
                Vector2 randomCircle = Random.insideUnitCircle * radius;

                Vector3 position = new Vector3(
                    randomCircle.x,
                    0f,
                    randomCircle.y
                );

                //Avoid center
                if (Vector3.Distance(position, Vector3.zero) < centerExclusionRadius)
                    continue;

                //Avoid overlap with anything already spawned
                bool tooClose = false;
                foreach (var occupied in occupiedPositions)
                {
                    if (Vector3.Distance(position, occupied) < minEnvironmentSpacing)
                    {
                        tooClose = true;
                        break;
                    }
                }

                if (tooClose)
                    continue;

                //Snap to terrain
                float y = terrain.SampleHeight(position) + terrainPos.y;
                position.y = y + yOffset;

                GameObject prefab =
                    environmentPrefabs[Random.Range(0, environmentPrefabs.Length)];

                Quaternion randomRotation =
                    Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);

                Instantiate(prefab, position, randomRotation, transform);

                occupiedPositions.Add(position);

                positionFound = true;
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
        if (currentNoiseIndex >= noiseBushes.Count)
            return;

        var bush = noiseBushes[currentNoiseIndex];
        bush.SetLoreIndex(currentNoiseIndex);
        bush.ActivateNoise();

        // Apply cheating color if active
        bush.SetCheatingVisual(cheatingActive);
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

        if (webSocketClient != null)
            webSocketClient.SendYellowLEDON();

    }



}

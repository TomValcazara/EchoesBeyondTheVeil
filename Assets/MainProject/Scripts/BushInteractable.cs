using UnityEngine;
using System.Collections;
using System.Collections.Generic; // optional, but safe

public class BushInteractable : MonoBehaviour
{
    [HideInInspector]
    public bool isActiveNoiseBush = false;

    private AudioSource audioSource;
    private GameManager gameManager;

    [Header("Reveal Prefabs")]
    public GameObject explodeFX;
    public GameObject mushroomsPrefab;
    public GameObject crystalPrefab;
    public GameObject lorePanelPrefab;

    [Header("Reveal Offsets")]
    public Vector3 mushroomsOffset = Vector3.zero;
    public Vector3 crystalOffset = new Vector3(0, 1.2f, 0);
    public Vector3 loreOffset = new Vector3(0, 1.2f, -0.5f);

    [Header("DEBUG")]
    public float debugLiftHeight = 20f; // set to 0 later

    private Renderer bushRenderer;
    private Color originalColor;

    [SerializeField] private float requiredBrushTime = 5f;
    //[SerializeField] private ParticleSystem leafParticles;

    private float brushTimer = 0f;
    //private bool isBrushing = false;
    private bool alreadyTriggered = false;
    public bool AlreadyTriggered => alreadyTriggered;
    private int loreIndex = -1;

    public void SetLoreIndex(int index)
    {
        loreIndex = index;
    }
    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        //gameManager = FindObjectOfType<GameManager>();
        gameManager = FindFirstObjectByType<GameManager>();

        bushRenderer = GetComponentInChildren<Renderer>();
        if (bushRenderer != null)
        {
            originalColor = bushRenderer.material.color;
        }

    }

    public void ActivateNoise()
    {
        isActiveNoiseBush = true;

        // DEBUG: lift active bush for visibility
        //transform.position += Vector3.up * debugLiftHeight;

        if (bushRenderer != null)
        {
            bushRenderer.material.color = Color.magenta;
        }

        audioSource.Play();
    }

    public void DeactivateNoise()
    {
        isActiveNoiseBush = false;

        if (bushRenderer != null)
        {
            bushRenderer.material.color = originalColor;
        }
    }



    // TEMP interaction (keyboard test)
    // void Update()
    // {
    //     if (!isActiveNoiseBush) return;

    //     if (Input.GetKeyDown(KeyCode.E))
    //     {
    //         Interact();
    //     }
    // }

    public void Interact()
    {
        Debug.Log("Destroyng");
        DeactivateNoise();
        StartCoroutine(RevealSequence());
    }

    IEnumerator RevealSequence()
    {
        Vector3 pos = transform.position;

        // 1. Explosion FX
        if (explodeFX != null)
        {
            Instantiate(explodeFX, pos, Quaternion.identity);
        }

        //Stops teh Audio
        if (audioSource != null)
            audioSource.Stop();

        // Disable collider so hands stop triggering it
        Collider col = GetComponent<Collider>();
        if (col != null)
            col.enabled = false;
            
        // 2. Hide bush
        //gameObject.SetActive(false); //This lines Fucksup teh code, because nothing from teh GameObject works if it destroyed
        GetComponentInChildren<Renderer>().enabled = false;

        yield return new WaitForSeconds(0.5f);

        // 3. Spawn mushrooms
        if (mushroomsPrefab != null)
        {
            Instantiate(
                mushroomsPrefab,
                pos + mushroomsOffset,
                Quaternion.identity
            );
        }

        yield return new WaitForSeconds(0.3f);

        // 4. Spawn crystal
        GameObject crystal = null;
        if (crystalPrefab != null)
        {
            crystal = Instantiate(
                crystalPrefab,
                pos + crystalOffset,
                Quaternion.identity
            );
        }

        // 5. Spawn lore panel
        if (lorePanelPrefab != null)
        {
            // Instantiate(
            //     lorePanelPrefab,
            //     pos + loreOffset,
            //     Quaternion.identity
            // );

            GameObject panel = Instantiate(
                lorePanelPrefab,
                pos + loreOffset,
                Quaternion.identity
            );

            LorePanel lorePanel = panel.GetComponent<LorePanel>();
            if (lorePanel != null)
            {
                lorePanel.SetText(gameManager.GetStoryText(loreIndex));
            }

        }

        yield return new WaitForSeconds(0.5f);

        // 6. Notify GameManager AFTER reveal finishes
        gameManager.OnNoiseBushCompleted(this);
        Debug.Log("Done Destroyng");
    }

    private void OnTriggerStay(Collider other)
    {

        if (!isActiveNoiseBush) return;   // 👈 Only active bush reacts

        if (alreadyTriggered) return;

        // You can tag your hand "Hand" for safety
        if (other.CompareTag("Hand"))
        {
            //isBrushing = true;
            brushTimer += Time.deltaTime;

            // if (!leafParticles.isPlaying)
            //     leafParticles.Play();

            if (brushTimer >= requiredBrushTime)
            {
                TriggerBush();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Hand"))
        {
           // isBrushing = false;
            brushTimer = 0f;

            //leafParticles.Stop();
        }
    }

    private void TriggerBush()
    {
        if (alreadyTriggered) return;

        alreadyTriggered = true;

        //leafParticles.Stop();

        Interact();
    }

}

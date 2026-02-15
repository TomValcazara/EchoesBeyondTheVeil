using UnityEngine;
using System.Collections;
using System.Collections.Generic;
//using UnityEngine.InputSystem;
//using UnityEngine.InputSystem.XR;
using UnityEngine.XR;


public class BushInteractable : MonoBehaviour
{
    [HideInInspector]
    public bool isActiveNoiseBush = false;

    private AudioSource audioSource;
    private GameManager gameManager;

    [Header("Reveal Prefabs")]
    //public GameObject explodeFX;
    public GameObject mushroomsPrefab;
    //public GameObject crystalPrefab;
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

    private Vector3 originalPosition;
    private bool cheatingVisualActive = false;

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

        //Saves original bush height, in case the player uses the cheat button
        originalPosition = transform.position;

    }

    public void ActivateNoise()
    {
        isActiveNoiseBush = true;

        // DEBUG: lift active bush for visibility
        //transform.position += Vector3.up * debugLiftHeight;

        // if (bushRenderer != null)
        // {
        //     bushRenderer.material.color = Color.magenta;
        // }

        audioSource.Play();
    }

    public void SetCheatingVisual(bool isCheating)
    {
        if (bushRenderer == null) return;

        cheatingVisualActive = isCheating;

        if (isCheating)
        {
            bushRenderer.material.color = Color.magenta;

            // Lift bush
            transform.position = originalPosition + Vector3.up * 1.5f;
        }
        else
        {
            bushRenderer.material.color = originalColor;

            // Reset position
            transform.position = originalPosition;
        }
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
        // if (explodeFX != null)
        // {
        //     Instantiate(explodeFX, pos, Quaternion.identity);
        // }

        //Stops the Audio
        // if (audioSource != null)
        //     audioSource.Stop();
        if (audioSource != null)
            StartCoroutine(FadeOutAudio(audioSource, 1.5f));

        // Disable collider so hands stop triggering it
        Collider col = GetComponent<Collider>();
        if (col != null)
            col.enabled = false;
            
        // 2. Hide bush
        //gameObject.SetActive(false); //This lines Fucksup teh code, because nothing from teh GameObject works if it destroyed
        GetComponentInChildren<Renderer>().enabled = false;

        yield return new WaitForSeconds(0.5f);

        // 3. Spawn mushrooms
        GameObject mushrooms = null;
        if (mushroomsPrefab != null)
        {
            mushrooms = Instantiate(
                mushroomsPrefab,
                pos + mushroomsOffset,
                Quaternion.identity
            );
        }

        yield return new WaitForSeconds(0.3f);

        // 4. Spawn crystal
        // GameObject crystal = null;
        // if (crystalPrefab != null)
        // {
        //     crystal = Instantiate(
        //         crystalPrefab,
        //         pos + crystalOffset,
        //         Quaternion.identity
        //     );
        // }

        // 5. Spawn lore panel
        GameObject panel = null;
        if (lorePanelPrefab != null)
        {
            panel = Instantiate(
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
        //Debug.Log("Done Destroyng");

        // Start despawn timer
        StartCoroutine(FadeAndDestroyAfterDelay(mushrooms, panel, 10f));

    }

    IEnumerator FadeAndDestroyAfterDelay(GameObject mushrooms, GameObject panel, float delay)
    {
        yield return new WaitForSeconds(delay);

        float duration = 2f;
        float time = 0f;

        CanvasGroup panelGroup = null;
        if (panel != null)
            panelGroup = panel.GetComponent<CanvasGroup>();

        Renderer[] mushroomRenderers = null;
        if (mushrooms != null)
            mushroomRenderers = mushrooms.GetComponentsInChildren<Renderer>();

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;
            float alpha = Mathf.Lerp(1f, 0f, t);

            // Fade panel (UI)
            if (panelGroup != null)
                panelGroup.alpha = alpha;

            // Fade mushrooms (3D object)
            if (mushroomRenderers != null)
            {
                foreach (var r in mushroomRenderers)
                {
                    if (r.material.HasProperty("_Color"))
                    {
                        Color c = r.material.color;
                        c.a = alpha;
                        r.material.color = c;
                    }
                }
            }

            yield return null;
        }

        if (mushrooms != null) Destroy(mushrooms);
        if (panel != null) Destroy(panel);
    }

    IEnumerator FadeOutAudio(AudioSource source, float duration)
    {
        float startVolume = source.volume;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;

            source.volume = Mathf.Lerp(startVolume, 0f, t);

            yield return null;
        }

        source.Stop();
        source.volume = startVolume; // reset for next activation
    }

    private void OnTriggerStay(Collider other)
    {

        if (!isActiveNoiseBush) return;   //Only active bush reacts

        if (alreadyTriggered) return;

        // You can tag your hand "Hand" for safety
        if (other.CompareTag("Hand"))
        {
            brushTimer += Time.deltaTime;

            SendHapticToHand(other, 0.2f, 0.05f);

            if (brushTimer >= requiredBrushTime)
            {
                TriggerBush(other);
            }
        }
    }

    private void SendHapticToHand(Collider handCollider, float amplitude, float duration)
    {
        XRNode node = XRNode.LeftHand;

        if (handCollider.name.ToLower().Contains("right"))
            node = XRNode.RightHand;

        var devices = new List<InputDevice>();
        InputDevices.GetDevicesAtXRNode(node, devices);

        foreach (var device in devices)
        {
            if (device.isValid)
            {
                device.SendHapticImpulse(0u, amplitude, duration);
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

    private void TriggerBush(Collider other)
    {
        if (alreadyTriggered) return;

        alreadyTriggered = true;

        SendHapticToHand(other, 0.8f, 0.2f);

        Interact();
    }

}

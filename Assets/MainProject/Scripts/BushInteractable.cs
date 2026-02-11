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

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        //gameManager = FindObjectOfType<GameManager>();
        gameManager = FindFirstObjectByType<GameManager>();
    }

    public void ActivateNoise()
    {

        isActiveNoiseBush = true;

        // DEBUG: lift active bush for visibility
        transform.position += Vector3.up * debugLiftHeight;

        audioSource.Play();
    }

    public void DeactivateNoise()
    {
        isActiveNoiseBush = false;
        audioSource.Stop();
    }

    // TEMP interaction (keyboard test)
    void Update()
    {
        if (!isActiveNoiseBush) return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            Interact();
        }
    }

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
            Instantiate(
                lorePanelPrefab,
                pos + loreOffset,
                Quaternion.identity
            );
        }

        yield return new WaitForSeconds(0.5f);

        // 6. Notify GameManager AFTER reveal finishes
        gameManager.OnNoiseBushCompleted(this);
        Debug.Log("Done Destroyng");
    }

}

using UnityEngine;

public class BushInteractable : MonoBehaviour
{
    [HideInInspector]
    public bool isActiveNoiseBush = false;

    private AudioSource audioSource;
    private GameManager gameManager;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        //gameManager = FindObjectOfType<GameManager>();
        gameManager = FindFirstObjectByType<GameManager>();
    }

    public void ActivateNoise()
    {
        isActiveNoiseBush = true;
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
        if (isActiveNoiseBush && Input.GetKeyDown(KeyCode.E))
        {
            Interact();
        }
    }

    public void Interact()
    {
        DeactivateNoise();
        gameManager.OnNoiseBushCompleted(this);
    }
}

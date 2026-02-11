using UnityEngine;

public class HandBrushFX : MonoBehaviour
{
    private ParticleSystem leafParticles;

    private void Awake()
    {
        leafParticles = GetComponentInChildren<ParticleSystem>();
    }

    private void OnTriggerEnter(Collider other)
    {
        //Debug.Log("Touched: " + other.name);
        BushInteractable bush = other.GetComponent<BushInteractable>();
        if (bush != null && bush.isActiveNoiseBush && !bush.AlreadyTriggered)
        {
            leafParticles.Play();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        //Debug.Log("V2");
        if (other.CompareTag("Bush"))
        {
            //Debug.Log("V3");
            leafParticles.Stop();
        }
    }
}

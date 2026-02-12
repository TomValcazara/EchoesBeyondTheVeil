using UnityEngine;
using System.Collections;

public class HellGateController : MonoBehaviour
{
    public Transform player;

    public AudioSource angelAudio;
    public AudioSource whisperAudio;

    public Light gateLight;

    public float triggerDistance = 10f;

    private bool switched = false;

    void Start()
    {
        angelAudio.Play();
        whisperAudio.Stop();
        gateLight.enabled = true;
    }

    void Update()
    {
        if (switched) return;

        float distance = Vector3.Distance(player.position, transform.position);

        if (distance <= triggerDistance)
        {
            SwitchToWhispers();
        }
    }

    void SwitchToWhispers()
    {
        switched = true;

        //angelAudio.Stop();
        //whisperAudio.Play();
        StartCoroutine(FadeAudio(angelAudio, whisperAudio, 5.0f));

        gateLight.enabled = false;
    }

    IEnumerator FadeAudio(AudioSource from, AudioSource to, float duration)
    {
        float time = 0;

        float startVolume = from.volume;
        to.volume = 0;
        to.Play();

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;

            from.volume = Mathf.Lerp(startVolume, 0, t);
            to.volume = Mathf.Lerp(0, 1, t);

            yield return null;
        }

        from.Stop();
    }

}

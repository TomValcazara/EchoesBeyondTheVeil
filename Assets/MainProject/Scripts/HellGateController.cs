using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class HellGateController : MonoBehaviour
{
    public Transform playerHead;

    public AudioSource angelAudio;
    public AudioSource whisperAudio;

    public Light gateLight;

    public float triggerDistance = 10f;

    private bool switched = false;

    public CanvasGroup[] gateCanvases;

    [Header("Websocket Client")]
    public WebSocketClientExample webSocketClient;
    // void Start()
    // {
    //     angelAudio.Play();
    //     whisperAudio.Stop();
    //     gateLight.enabled = true;
    // }
    void OnEnable()
    {
        //Debug.Log("HellGate is True");
        switched = false;

        angelAudio.volume = 1f;
        whisperAudio.volume = 0f;

        angelAudio.Play();
        whisperAudio.Stop();

        gateLight.enabled = true;

        //Hide Canvas
        foreach (var canvas in gateCanvases)
        {
            canvas.alpha = 0f;
            canvas.interactable = false;
            canvas.blocksRaycasts = false;
        }

    }
    void Update()
    {
        
        if (switched) return;

        //float distance = Vector3.Distance(player.position, transform.position);
        //Vector3 playerFlat = new Vector3(player.position.x, 0, player.position.z);
        Vector3 playerFlat = new Vector3(playerHead.position.x, 0, playerHead.position.z);
        Vector3 gateFlat = new Vector3(transform.position.x, 0, transform.position.z);

        float distance = Vector3.Distance(playerFlat, gateFlat);
        
        //Debug.Log("Distance to Gate: " + distance);

        if (distance <= triggerDistance)
        {
            SwitchToWhispers();
        }
    }

    void SwitchToWhispers()
    {
        //Debug.Log("Angels Singing");

        switched = true;

        //angelAudio.Stop();
        //whisperAudio.Play();
        StartCoroutine(TransitionFinalHellGate(angelAudio, whisperAudio, 2.0f));

        //gateLight.enabled = false;
        //gateLight.intensity = Mathf.Lerp(gateLight.intensity, 0, Time.deltaTime * 3f);

        if (webSocketClient != null)
            webSocketClient.SendGreenLEDON();

    }

    IEnumerator TransitionFinalHellGate(AudioSource from, AudioSource to, float duration)
    {
        float time = 0;

        float startVolume = from.volume;
        float startIntensity = gateLight.intensity;

        to.volume = 0;
        to.Play();

        while (time < duration)
        {
            time += Time.deltaTime;
            //float t = time / duration;
            //float t = Mathf.Clamp01(time / duration);
            float t = Mathf.SmoothStep(0f, 1f, time / duration);

            //Music
            from.volume = Mathf.Lerp(startVolume, 0, t);
            to.volume = Mathf.Lerp(0, 1, t);

            //Light
            gateLight.intensity = Mathf.Lerp(startIntensity, 0, t);

            //Canvas
            foreach (var canvas in gateCanvases)
            {
                canvas.alpha = Mathf.Lerp(0f, 1f, t);
            }

            yield return null;
        }

        from.Stop();

        foreach (var canvas in gateCanvases)
        {
            canvas.alpha = 1f;
            canvas.interactable = true;
            canvas.blocksRaycasts = true;
        }

    }

    public void RestartScene()
    {
        Debug.Log("Restarting teh Scene");

        if (webSocketClient != null) //Resets the LEDs to be off
        {
            webSocketClient.SendYellowLEDOFF();
            webSocketClient.SendGreenLEDOFF();
        }

        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }

}

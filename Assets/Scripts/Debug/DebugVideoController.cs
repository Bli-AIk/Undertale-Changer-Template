using UnityEngine;

public class DebugVideoController : MonoBehaviour
{
    private UnityEngine.Video.VideoPlayer videoPlayer;
    public int skip;

    private void Start()
    {
        videoPlayer = GetComponent<UnityEngine.Video.VideoPlayer>();
    }

    private void Update()
    {
        if (!MainControl.instance.PlayerControl.isDebug)
            return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (videoPlayer.isPaused)
            {
                videoPlayer.Play();
            }
            else
            {
                videoPlayer.Pause();
            }
        }

        if (Input.GetKeyDown(KeyCode.KeypadPlus))
        {
            videoPlayer.StepForward();
        }

        if (Input.GetKeyDown(KeyCode.KeypadMinus))
        {
            videoPlayer.frame -= 1;
        }
        if (Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            videoPlayer.frame = skip;
        }
    }
}

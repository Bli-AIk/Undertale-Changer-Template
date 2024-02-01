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
                //DebugLogger.Log("video play");
            }
            else
            {
                videoPlayer.Pause();
                //DebugLogger.Log("video pause");
            }
        }

        if (Input.GetKeyDown(KeyCode.KeypadPlus))
        {
            videoPlayer.StepForward();
            //DebugLogger.Log("video +");
            //DebugLogger.Log("frame:" + videoPlayer.frame);
        }

        if (Input.GetKeyDown(KeyCode.KeypadMinus))
        {
            videoPlayer.frame -= 1;
            //DebugLogger.Log("video -");
            //DebugLogger.Log("frame:" + videoPlayer.frame);
        }
        if (Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            videoPlayer.frame = skip;
            //DebugLogger.Log("video skip");
        }
    }
}
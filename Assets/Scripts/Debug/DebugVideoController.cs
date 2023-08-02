using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class DebugVideoController : MonoBehaviour
{
    UnityEngine.Video.VideoPlayer videoPlayer;
    public int skip;
    // Start is called before the first frame update
    void Start()
    {
        videoPlayer = GetComponent<UnityEngine.Video.VideoPlayer>();

    }

    // Update is called once per frame
    void Update()
    {
        if (!MainControl.instance.OverworldControl.isDebug)
            return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (videoPlayer.isPaused)
            {
                videoPlayer.Play();
                //Debug.Log("video play");
            }
            else
            {
                videoPlayer.Pause();
                //Debug.Log("video pause");
            }
        }

        if (Input.GetKeyDown(KeyCode.KeypadPlus))
        {
            videoPlayer.StepForward();
            //Debug.Log("video +");
            //Debug.Log("frame:" + videoPlayer.frame);
        }

        if (Input.GetKeyDown(KeyCode.KeypadMinus))
        {
            videoPlayer.frame -= 1;
            //Debug.Log("video -");
            //Debug.Log("frame:" + videoPlayer.frame);
        }
        if (Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            videoPlayer.frame = skip;
            //Debug.Log("video skip");
        }
    }
}

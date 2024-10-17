using UCT.Global.Core;
using UnityEngine;
using UnityEngine.Video;

namespace Debug
{
    public class DebugVideoController : MonoBehaviour
    {
        private VideoPlayer videoPlayer;
        public int skip;

        private void Start()
        {
            videoPlayer = GetComponent<VideoPlayer>();
        }

        private void Update()
        {
            if (!MainControl.Instance.PlayerControl.isDebug)
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
}
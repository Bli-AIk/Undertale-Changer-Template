using UCT.Global.Core;
using UnityEngine;
using UnityEngine.Video;

namespace Debug
{
    public class DebugVideoController : MonoBehaviour
    {
        private VideoPlayer _videoPlayer;
        public int skip;

        private void Start()
        {
            _videoPlayer = GetComponent<VideoPlayer>();
        }

        private void Update()
        {
            if (!MainControl.Instance.playerControl.isDebug)
                return;

            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (_videoPlayer.isPaused)
                {
                    _videoPlayer.Play();
                    //Debug.Log("video play");
                }
                else
                {
                    _videoPlayer.Pause();
                    //Debug.Log("video pause");
                }
            }

            if (Input.GetKeyDown(KeyCode.KeypadPlus))
            {
                _videoPlayer.StepForward();
                //Debug.Log("video +");
                //Debug.Log("frame:" + videoPlayer.frame);
            }

            if (Input.GetKeyDown(KeyCode.KeypadMinus))
            {
                _videoPlayer.frame -= 1;
                //Debug.Log("video -");
                //Debug.Log("frame:" + videoPlayer.frame);
            }
            if (Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                _videoPlayer.frame = skip;
                //Debug.Log("video skip");
            }
        }
    }
}

using UCT.Audio;
using UCT.Core;
using UnityEngine;
using Random = UnityEngine.Random;

namespace UCT.Overworld
{
    /// <summary>
    ///     给OW Player的Animator提供事件脚本。
    /// </summary>
    public class OverworldPlayerAnimEventHelper : MonoBehaviour
    {
        private OverworldPlayerBehaviour _overworldPlayerBehaviour;

        private void Start()
        {
            _overworldPlayerBehaviour = GetComponent<OverworldPlayerBehaviour>();
        }

        public void PlayWalkAudio() //动画器引用
        {
            var walkFxRange = _overworldPlayerBehaviour.walkFxRange;
            AudioController.Instance.PlayFx(Random.Range((int)walkFxRange.x, (int)walkFxRange.y),
                MainControl.Instance.AudioControl.fxClipWalk, 1);
        }
    }
}
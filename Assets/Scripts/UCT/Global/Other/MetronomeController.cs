using System.Collections.Generic;
using UCT.Global.Audio;
using UCT.Global.Core;
using UnityEngine;

namespace UCT.Global.Other
{
    /// <summary>
    /// 节拍器控制器
    /// </summary>
    public class MetronomeController : MonoBehaviour
    {
        [Header("=== BPM相关设置 ===")]
        [Header("BGM BPM")]
        public float bpm = 120;
        private float _bpmBackup = 120;
        [Header("BGM BPM偏移")]
        public float bpmDeviation; 
        private float _bpmDeviationBackup;
        [Header("每拍所在的秒数列表")]
        public List<float> beatSeconds;
        [Space]
        [Header("=== 节拍器 ===")]
        [Header("是否播放节拍器")]
        public bool isPlayMetronome;
        [Header("当前节拍数")]
        public int currentBeatIndex; 
        [Header("下一节拍所在时间")] 
        public float nextBeatSecond;

        private void Start()
        {
            beatSeconds = MusicBpmCount(bpm, bpmDeviation);
        }

        private void Update()
        {
            if (!Mathf.Approximately(bpm, _bpmBackup) || !Mathf.Approximately(bpmDeviation, _bpmDeviationBackup))
            {
                _bpmBackup = bpm;
                _bpmDeviationBackup = bpmDeviation;
                Start();
            }
            
            CalculateAndPlayMetronome(beatSeconds);
        }

        /// <summary>
        /// 计算BGM节拍
        /// </summary>
        private static List<float> MusicBpmCount(float inputBpm, float inputBpmDeviation, float musicDuration = 0)
        {
            if (AudioController.Instance.audioSource.clip == null) return new List<float>();
            
            if (musicDuration <= 0)
                musicDuration = AudioController.Instance.audioSource.clip.length;

            var beatInterval = 60f / inputBpm;
            var currentTime = inputBpmDeviation;
            List<float> beats = new();

            while (currentTime < musicDuration)
            {
                beats.Add(currentTime);
                currentTime += beatInterval;
            }

            return beats;
        }

        /// <summary>
        /// 控制节拍器
        /// </summary>
        /// <param name="instanceBeatTimes"></param>
        private void CalculateAndPlayMetronome(List<float> instanceBeatTimes)
        {
            if (instanceBeatTimes.Count <= 0) return;

            var firstIn = true;
            while (currentBeatIndex < instanceBeatTimes.Count &&
                   AudioController.Instance.audioSource.time >= nextBeatSecond)
            {
                if (!Mathf.Approximately(bpm, _bpmBackup) || !Mathf.Approximately(bpmDeviation, _bpmDeviationBackup))
                    return;
                
                if (firstIn && isPlayMetronome)
                    AudioController.Instance.GetFx(currentBeatIndex % 4 == 0 ? 13 : 14,
                        MainControl.Instance.AudioControl.fxClipUI);

                currentBeatIndex++;

                if (currentBeatIndex < instanceBeatTimes.Count)
                {
                    nextBeatSecond = instanceBeatTimes[currentBeatIndex];
                }

                firstIn = false;
            }

            if (currentBeatIndex < instanceBeatTimes.Count) return;
            nextBeatSecond = instanceBeatTimes[0];
            currentBeatIndex = 0;
        }
    }
}
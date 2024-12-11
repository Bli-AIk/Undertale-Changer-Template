﻿using TMPro;
using UCT.Global.Core;
using UnityEngine;

namespace UCT.Global.Settings
{
    /// <summary>
    /// 用于监视游戏内的FPS。
    /// 它可被输出在脚本挂载物体的文本上。
    /// </summary>
    public class FPSMonitor : MonoBehaviour
    {
        private TMP_Text _fps;
        private float _mFPS; //帧率
        private int _mFrameUpdate; //帧数;
        private float _mLastUpdateShowTime; //上一次更新帧率的时间;
        private const float MUpdateShowDeltaTime = 0.2f; //更新帧率的时间间隔;

        private void Start()
        {
            _fps = GetComponent<TMP_Text>();
            _mLastUpdateShowTime = Time.realtimeSinceStartup;
        }

        private void Update()
        {
            if (_fps) _fps.text = SettingsStorage.isDisplayFPS ? UpdateFPS(_fps.text) : "";
        }
        
        /// <summary>
        ///     计算并返回当前帧率的字符串表示，每隔指定时间刷新一次。
        /// </summary>
        /// <param name="input">未到间隔时间时返回input</param>
        /// <returns>当前整数FPS字符串</returns>
        private string UpdateFPS(string input)
        {
            _mFrameUpdate++;
            if (!(Time.realtimeSinceStartup - _mLastUpdateShowTime >= MUpdateShowDeltaTime)) return input;
            _mFPS = _mFrameUpdate / (Time.realtimeSinceStartup - _mLastUpdateShowTime);
            _mFrameUpdate = 0;
            _mLastUpdateShowTime = Time.realtimeSinceStartup;
            return ((int)_mFPS).ToString();
        }
    }
}
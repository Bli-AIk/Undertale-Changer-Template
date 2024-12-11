﻿using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace UCT.Other
{
    /// <summary>
    ///     用于TMP动态类型字体的控制
    /// </summary>
    public class TMPDynamicFontController : MonoBehaviour
    {
        private const string Fonts = "Fonts/";
        private const string SonFonts = "Fonts/SonFonts/";
        public static TMPDynamicFontController Instance;
        private TMP_FontAsset _simsun;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            _simsun = Resources.Load<TMP_FontAsset>($"{SonFonts}SIMSUN");
        }

        public void SimsunClear(List<TMP_Text> forceMeshUpdateTexts = null)
        {
            if (!_simsun)
                Start();

            _simsun.ClearFontAssetData();
            switch (forceMeshUpdateTexts)
            {
                case { Count: <= 0 }:
                case null: return;
            }

            foreach (var tmp in forceMeshUpdateTexts)
                tmp.ForceMeshUpdate();
        }
    }
}
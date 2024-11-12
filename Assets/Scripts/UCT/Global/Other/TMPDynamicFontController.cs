using System.Collections.Generic;
using UnityEngine;
using TMPro;
namespace UCT.Global.Other
{
    /// <summary>
    /// 用于TMP动态类型字体的控制
    /// </summary>
    public class TMPDynamicFontController : MonoBehaviour
    {
        public static TMPDynamicFontController Instance;
        private TMP_FontAsset _simsun;
        private const string Fonts = "Fonts/";
        private const string SonFonts = "Fonts/SonFonts/";

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
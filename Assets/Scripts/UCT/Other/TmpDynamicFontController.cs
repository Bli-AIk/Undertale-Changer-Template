using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace UCT
{
    /// <summary>
    ///     用于TMP动态类型字体的控制
    /// </summary>
    public class TmpDynamicFontController : MonoBehaviour
    {
        private const string SonFonts = "Fonts/SonFonts/";
        private TMP_FontAsset _simsun;
        public static TmpDynamicFontController Instance { get; private set; }

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
            {
                Start();
            }

            _simsun.ClearFontAssetData();
            if (forceMeshUpdateTexts is { Count: <= 0 } or null)
            {
                return;
            }

            foreach (var tmp in forceMeshUpdateTexts)
            {
                tmp.ForceMeshUpdate();
            }
        }
    }
}
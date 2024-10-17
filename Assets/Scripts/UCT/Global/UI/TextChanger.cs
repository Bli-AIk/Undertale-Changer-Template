using System;
using TMPro;
using UCT.Global.Core;
using UnityEngine;

namespace UCT.Global.UI
{
    /// <summary>
    /// 用于字体匹配及双语字体数据修正
    /// </summary>
    public class TextChanger : MonoBehaviour
    {
        //public TMP_FontAsset assetback;
        private TMP_Text tmp;

        public bool width;//若中英混搭 则true就完事了

        [Header("US/CN")]
        public Vector4[] Options;

        //public float[] sizes;

        //public bool no = true;
        private void Start()
        {
            Set();
            Change();
        }

        public void Set()
        {
            tmp = GetComponent<TMP_Text>();

            /*
       while (tmp.font != assetback)
        {
            tmp.font = assetback;
        }
        */
            width = MainControl.Instance.OverworldControl.textWidth;
            /*
        if (tmp.font != assetback)
            tmp.font = assetback;
        */
        }

        public void Change()
        {
            if (tmp != null)
            {
                tmp.characterSpacing = Options[Convert.ToInt32(width)].x;
                tmp.wordSpacing = Options[Convert.ToInt32(width)].y;
                tmp.lineSpacing = Options[Convert.ToInt32(width)].z;
                tmp.paragraphSpacing = Options[Convert.ToInt32(width)].w;
            }
            else
            {
                Set();
                Change();
            }
        }
    }
}
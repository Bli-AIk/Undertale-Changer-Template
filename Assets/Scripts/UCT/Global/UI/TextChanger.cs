using System;
using TMPro;
using UCT.Global.Core;
using UnityEngine;
using UnityEngine.Serialization;

namespace UCT.Global.UI
{
    /// <summary>
    /// 用于字体匹配及双语字体数据修正
    /// </summary>
    public class TextChanger : MonoBehaviour
    {
        //public TMP_FontAsset assetback;
        private TMP_Text _tmp;

        public bool width;//若中英混搭 则true就完事了

        [FormerlySerializedAs("Options")] [Header("US/CN")]
        public Vector4[] options;

        //public float[] sizes;

        //public bool no = true;
        private void Start()
        {
            Set();
            Change();
        }

        public void Set()
        {
            _tmp = GetComponent<TMP_Text>();

            /*
       while (tmp.font != assetback)
        {
            tmp.font = assetback;
        }
        */
            width = MainControl.Instance.overworldControl.textWidth;
            /*
        if (tmp.font != assetback)
            tmp.font = assetback;
        */
        }

        public void Change()
        {
            if (_tmp != null)
            {
                _tmp.characterSpacing = options[Convert.ToInt32(width)].x;
                _tmp.wordSpacing = options[Convert.ToInt32(width)].y;
                _tmp.lineSpacing = options[Convert.ToInt32(width)].z;
                _tmp.paragraphSpacing = options[Convert.ToInt32(width)].w;
            }
            else
            {
                Set();
                Change();
            }
        }
    }
}
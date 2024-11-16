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
        private TMP_Text _tmp;

        public bool width;//若中英混搭 则true就完事了

        [FormerlySerializedAs("Options")] [Header("US/CN")]
        public Vector4[] options;

        public float[] fontSize = new float[2];
        private void Start()
        {
            Set();
            Change();
        }

        public void Set()
        {
            _tmp = GetComponent<TMP_Text>();
            width = MainControl.Instance.overworldControl.textWidth;
        
        }

        public void Change()
        {
            while (true)
            {
                if (_tmp != null)
                {
                    _tmp.characterSpacing = options[Convert.ToInt32(width)].x;
                    _tmp.wordSpacing = options[Convert.ToInt32(width)].y;
                    _tmp.lineSpacing = options[Convert.ToInt32(width)].z;
                    _tmp.paragraphSpacing = options[Convert.ToInt32(width)].w;

                    if (fontSize.Length >= 2 && fontSize[0] != 0 && fontSize[1] != 0)
                        _tmp.fontSize = fontSize[Convert.ToInt32(width)];
                }
                else
                {
                    Set();
                    continue;
                }

                break;
            }
        }
    }
}
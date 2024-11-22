using System;
using TMPro;
using UCT.Global.Core;
using UnityEngine;
using UnityEngine.Serialization;

namespace UCT.Global.UI
{
    /// <summary>
    ///     用于字体匹配及双语字体数据修正
    /// </summary>
    public class TextChanger : MonoBehaviour
    {
        [FormerlySerializedAs("width")] public bool isUseWidth;

        [FormerlySerializedAs("Options")] [Header("US/CN")]
        public Vector4[] options;

        [FormerlySerializedAs("fontSize")] public float[] fontSizes = new float[2];

        public Vector3[] positions = new Vector3[2];
        private TMP_Text _tmp;

        private void Start()
        {
            Set();
            Change();
        }

        public void Set()
        {
            _tmp = GetComponent<TMP_Text>();
            isUseWidth = MainControl.Instance.overworldControl.textWidth;
        }

        public void Change()
        {
            while (true)
            {
                if (_tmp)
                {
                    _tmp.characterSpacing = options[Convert.ToInt32(isUseWidth)].x;
                    _tmp.wordSpacing = options[Convert.ToInt32(isUseWidth)].y;
                    _tmp.lineSpacing = options[Convert.ToInt32(isUseWidth)].z;
                    _tmp.paragraphSpacing = options[Convert.ToInt32(isUseWidth)].w;

                    if (fontSizes.Length >= 2 && fontSizes[0] != 0 && fontSizes[1] != 0)
                        _tmp.fontSize = fontSizes[Convert.ToInt32(isUseWidth)];

                    if (positions.Length >= 2 && !(positions[0] == new Vector3() && positions[1] == new Vector3()))
                        _tmp.transform.position = positions[Convert.ToInt32(isUseWidth)];
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
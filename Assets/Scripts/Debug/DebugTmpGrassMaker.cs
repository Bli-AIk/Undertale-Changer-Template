using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Debug
{
    public class DebugTmpGrassMaker : MonoBehaviour
    {
        private TextMeshPro _tmp;
        public List<string> strings;
        public float time, timeMax;
        private bool _select;

        private void Start()
        {
            strings.Add("²Ý\n²Ý");
            strings.Add("²Ý *\n²Ý *");
            _tmp = GetComponent<TextMeshPro>();
            time = timeMax;
        }

        private void Update()
        {
            if (time < 0)
            {
                time = timeMax;
                _select = !_select;
                _tmp.text = strings[Convert.ToInt32(_select)];
            }
            else time -= Time.deltaTime;
        }
    }
}
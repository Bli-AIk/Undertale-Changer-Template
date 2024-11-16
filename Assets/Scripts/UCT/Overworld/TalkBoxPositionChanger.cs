using System;
using TMPro;
using UCT.Global.Core;
using UCT.Global.UI;
using UnityEngine;

namespace UCT.Overworld
{
    /// <summary>
    /// 修改Overworld中对话框的位置
    /// </summary>
    public class TalkBoxPositionChanger : MonoBehaviour
    {
        public static TalkBoxPositionChanger Instance;
        public BoxDrawer boxDrawer;
        public bool isUp;
        public bool haveHead;
        private TextMeshProUGUI _typeMessage;
        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            boxDrawer = GetComponent<BoxDrawer>();
            _typeMessage = GetTypeMessage();
            boxDrawer.localPosition = new Vector3(boxDrawer.localPosition.x, boxDrawer.localPosition.y, BackpackBehaviour.BoxZAxisInvisible);
        }

        private TextMeshProUGUI GetTypeMessage()
        {
            var uiTalk = GameObject.Find("BackpackCanvas/RawImage/Talk/UITalk");
;            if (!_typeMessage && uiTalk)
                _typeMessage = uiTalk.GetComponent<TextMeshProUGUI>();

            return _typeMessage;
        }
        
        public void Change(bool updateHeader = true, bool haveHeader = false, bool cleaner = true, TypeWritter typeWritter = null)
        {
            if (cleaner)
            {
                GetTypeMessage().text = "";
                if (typeWritter)
                    typeWritter.endString = "";
            }

            if (!updateHeader)
                return;
            haveHead = haveHeader;

            const float boxUpYAxis = 3.85f;
            const float boxDownYAxis = -3.85f;
            if (isUp)
            {
                boxDrawer.localPosition = new Vector3(boxDrawer.localPosition.x, boxUpYAxis, boxDrawer.localPosition.z);
                GetTypeMessage().rectTransform.anchoredPosition =
                    new Vector2(10 + 115f * Convert.ToInt32(haveHead), 139);
            }
            else
            {
                boxDrawer.localPosition = new Vector3(boxDrawer.localPosition.x, boxDownYAxis, boxDrawer.localPosition.z);
                GetTypeMessage().rectTransform.anchoredPosition =
                    new Vector2(10 + 115f * Convert.ToInt32(haveHead), -170);
            }
        }

   


        private void OnEnable()
        {
            GetTypeMessage();
            if (_typeMessage)
                _typeMessage.gameObject.SetActive(true);
        }

        private void OnDisable()
        {
            GetTypeMessage();
            if (_typeMessage)
                _typeMessage.gameObject.SetActive(false);
        }
    }
}
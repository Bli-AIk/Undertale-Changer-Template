using System;
using TMPro;
using UCT.Global.Core;
using UCT.Global.UI;
using UnityEngine;

namespace UCT.Overworld
{
    /// <summary>
    ///     修改Overworld中对话框的位置
    /// </summary>
    public class TalkBoxController : MonoBehaviour
    {
        public static TalkBoxController Instance;
        public BoxDrawer boxDrawer;
        public bool isUp;
        public bool haveHead;
        private TextMeshPro _typeMessage;
        public ItemScroller ItemScroller { get; private set; }

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            boxDrawer = GetComponent<BoxDrawer>();
            _typeMessage = GetTypeMessage();
            ItemScroller = transform.Find("ItemSelect").GetComponent<ItemScroller>();
            boxDrawer.localPosition = new Vector3(boxDrawer.localPosition.x, boxDrawer.localPosition.y,
                BackpackBehaviour.BoxZAxisInvisible);
        }


        private void OnEnable()
        {
            GetTypeMessage();
            if (_typeMessage)
            {
                _typeMessage.gameObject.SetActive(true);
            }
        }

        private void OnDisable()
        {
            GetTypeMessage();
            if (_typeMessage)
            {
                _typeMessage.gameObject.SetActive(false);
            }
        }

        private TextMeshPro GetTypeMessage()
        {
            var talkText = BackpackBehaviour.Instance.talkText;
            if (!_typeMessage && talkText)
            {
                _typeMessage = talkText;
            }

            return _typeMessage;
        }

        public void CleanText(TypeWritter typeWritter)
        {
            GetTypeMessage().text = "";
            if (typeWritter)
            {
                typeWritter.endString = "";
            }
        }

        public void SetHead(bool inputHaveHead)
        {
            haveHead = inputHaveHead;
            const float boxUpYAxis = 3.85f;
            const float boxDownYAxis = -3.85f;
            if (isUp)
            {
                boxDrawer.localPosition = new Vector3(boxDrawer.localPosition.x, boxUpYAxis, boxDrawer.localPosition.z);
                GetTypeMessage().rectTransform.anchoredPosition =
                    new Vector2(0.25f + 2.9f * Convert.ToInt32(haveHead), -0.535f);
            }
            else
            {
                boxDrawer.localPosition =
                    new Vector3(boxDrawer.localPosition.x, boxDownYAxis, boxDrawer.localPosition.z);
                GetTypeMessage().rectTransform.anchoredPosition =
                    new Vector2(0.25f + 2.9f * Convert.ToInt32(haveHead), -0.625f);
            }
        }
    }
}
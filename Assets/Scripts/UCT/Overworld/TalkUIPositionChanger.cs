using System;
using UCT.Global.UI;
using UnityEngine;
using UnityEngine.Serialization;

namespace UCT.Overworld
{
    /// <summary>
    /// 修改Overworld中对话框的位置
    /// </summary>
    public class TalkUIPositionChanger : MonoBehaviour
    {
        public static TalkUIPositionChanger Instance;
        public BoxDrawer boxDrawer;
        public bool isUp;
        public bool haveHead;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            boxDrawer = GetComponent<BoxDrawer>();
            boxDrawer.localPosition = new Vector3(boxDrawer.localPosition.x, boxDrawer.localPosition.y, BackpackBehaviour.BoxZAxisInvisible);
        }

        public void Change(bool updateHeader = true, bool haveHeader = false, bool cleaner = true, TypeWritter typeWritter = null)
        {
            if (cleaner)
            {
                BackpackBehaviour.Instance.typeMessage.text = "";
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
                BackpackBehaviour.Instance.typeMessage.rectTransform.anchoredPosition =
                    new Vector2(10 + 115f * Convert.ToInt32(haveHead), 139);
            }
            else
            {
                boxDrawer.localPosition = new Vector3(boxDrawer.localPosition.x, boxDownYAxis, boxDrawer.localPosition.z);
                BackpackBehaviour.Instance.typeMessage.rectTransform.anchoredPosition =
                    new Vector2(10 + 115f * Convert.ToInt32(haveHead), -170);
            }
        }

   


        private void OnEnable()
        {
            if (BackpackBehaviour.Instance.typeMessage != null)
                BackpackBehaviour.Instance.typeMessage.gameObject.SetActive(true);
        }

        private void OnDisable()
        {
            if (BackpackBehaviour.Instance.typeMessage != null)
                BackpackBehaviour.Instance.typeMessage.gameObject.SetActive(false);
        }
    }
}
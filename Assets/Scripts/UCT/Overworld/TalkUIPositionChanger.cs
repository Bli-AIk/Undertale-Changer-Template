using System;
using UCT.Global.UI;
using UnityEngine;

namespace UCT.Overworld
{
    /// <summary>
    /// 修改Overworld中对话框的位置
    /// </summary>
    public class TalkUIPositionChanger : MonoBehaviour
    {
        public static TalkUIPositionChanger Instance;
        public bool isUp;
        public bool haveHead;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            //gameObject.SetActive(false);
            transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, -50);
        }

        public void Change(bool updateHeader = true, bool haveHeader = false, bool cleaner = true, TypeWritter typeWritter = null)
        {
            if (cleaner)
            {
                BackpackBehaviour.Instance.typeMessage.text = "";
                if (typeWritter != null)
                    typeWritter.endString = "";
            }

            if (!updateHeader)
                return;
            haveHead = haveHeader;

            if (isUp)
            {
                transform.localPosition = new Vector3(transform.localPosition.x, 7.77f, transform.localPosition.z);
                BackpackBehaviour.Instance.typeMessage.rectTransform.anchoredPosition = new Vector2(10 + 115f * Convert.ToInt32(haveHead), 139);
            }
            else
            {
                transform.localPosition = new Vector3(transform.localPosition.x, 0, transform.localPosition.z);
                BackpackBehaviour.Instance.typeMessage.rectTransform.anchoredPosition = new Vector2(10 + 115f * Convert.ToInt32(haveHead), -170);
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
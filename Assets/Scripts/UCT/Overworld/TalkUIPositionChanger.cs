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
        private BoxDrawer _boxDrawer;
        public bool isUp;
        public bool haveHead;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            _boxDrawer = GetComponent<BoxDrawer>();
            _boxDrawer.localPosition = new Vector3(_boxDrawer.localPosition.x, _boxDrawer.localPosition.y, BackpackBehaviour.BoxZAxisInvisible);
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

            if (isUp)
            {
                _boxDrawer.localPosition = new Vector3(_boxDrawer.localPosition.x, 7.77f, _boxDrawer.localPosition.z);
                BackpackBehaviour.Instance.typeMessage.rectTransform.anchoredPosition = new Vector2(10 + 115f * Convert.ToInt32(haveHead), 139);
            }
            else
            {
                _boxDrawer.localPosition = new Vector3(_boxDrawer.localPosition.x, 0, _boxDrawer.localPosition.z);
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
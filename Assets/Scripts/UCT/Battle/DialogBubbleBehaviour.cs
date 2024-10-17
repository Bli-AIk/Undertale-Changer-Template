using TMPro;
using UCT.Global.UI;
using UnityEngine;

namespace UCT.Battle
{
    /// <summary>
    /// 战斗内敌人对话气泡控制
    /// </summary>
    public class DialogBubbleBehaviour : MonoBehaviour
    {
        public Vector2 position, size;
        public bool isBackRight;
        public float backY;
        private SpriteRenderer sprite, spriteBack;
        public TypeWritter typeWritter;
        public TextMeshPro tmp;

        private void Awake()
        {
            typeWritter = transform.Find("Text").GetComponent<TypeWritter>();
            tmp = transform.Find("Text").GetComponent<TextMeshPro>();
        }

        private void Start()
        {
            sprite = GetComponent<SpriteRenderer>();
            spriteBack = transform.Find("DialogBubbleBack").GetComponent<SpriteRenderer>();

            PositionChange();
        }

        /// <summary>
        /// 改变气泡的大小和左右箭头情况 进行赋值 之类的
        /// </summary>
        public void PositionChange()
        {
            transform.localPosition = position;
            sprite.size = size;
            spriteBack.flipX = isBackRight;
            if (!isBackRight)
            {
                spriteBack.transform.localPosition = new Vector3(-0.2396f, backY, 0);
            }
            else
            {
                spriteBack.transform.localPosition = new Vector3(4.2396f + sprite.size.x - 4, backY, 0);
            }
        }
    }
}
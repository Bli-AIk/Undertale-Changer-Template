using Alchemy.Inspector;
using TMPro;
using UCT.Core;
using UCT.Service;
using UnityEngine;

namespace UCT.Battle
{
    /// <summary>
    ///     战斗内敌人对话气泡控制
    /// </summary>
    public class DialogBubbleBehaviour : MonoBehaviour
    {
        [ReadOnly]
        public Vector2 position, size;
        [ReadOnly]
        public bool isBackRight;
        [ReadOnly]
        public float backY;
        [HideInInspector]
        public TypeWritter typeWritter;
        [HideInInspector]
        public TextMeshPro tmp;

        public float delay;
        public EnemiesXmlDialogParser.Message Message;
        private SpriteRenderer _sprite, _spriteBack;

        private void Awake()
        {
            typeWritter = transform.Find("Text").GetComponent<TypeWritter>();
            tmp = transform.Find("Text").GetComponent<TextMeshPro>();
            _sprite = GetComponent<SpriteRenderer>();
            _spriteBack = transform.Find("DialogBubbleBack").GetComponent<SpriteRenderer>();
        }

        private void Start()
        {
            PositionChange();
        }

        /// <summary>
        ///     改变气泡的大小和左右箭头情况 进行赋值 之类的
        /// </summary>
        public void PositionChange()
        {
            transform.localPosition = position;
            _sprite.size = size;
            _spriteBack.flipX = isBackRight;
            _spriteBack.transform.localPosition = !isBackRight
                ? new Vector3(-0.2396f, backY, 0)
                : new Vector3(4.2396f + _sprite.size.x - 4, backY, 0);
        }
    }
}
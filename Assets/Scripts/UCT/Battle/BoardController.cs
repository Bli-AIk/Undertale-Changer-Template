using System.Collections.Generic;
using UnityEngine;

namespace UCT.Battle
{
    /// <summary>
    /// 设定挡板，具体数据在BattlePlayerController内控制
    /// </summary>
    public class BoardController : MonoBehaviour
    {
        [Header("宽度")]
        public float width = 2.1f;

        [Header("是否为跟踪板")]
        public bool canMove;

        [Header("是否让边缘碰撞器长度随sprite宽度而变化")]
        public bool keepEdge;

        public List<Sprite> boards;
        private BoxCollider2D _boxCollider2DUp, _boxCollider2DDown;//纯纯的检测器 检测玩家在上面就把EdgeCollider掐了。具体在BattlePlayerController内控
        private EdgeCollider2D _edgeCollider2D;//默认为触发器。
        private SpriteRenderer _spriteRenderer;

        //public bool test;
        private void Awake()
        {
            _spriteRenderer = gameObject.GetComponent<SpriteRenderer>();

            _edgeCollider2D = gameObject.AddComponent<EdgeCollider2D>();

            _spriteRenderer.drawMode = SpriteDrawMode.Sliced;
            _spriteRenderer.size = new Vector2(width, 0.5f);

            _boxCollider2DUp = gameObject.AddComponent<BoxCollider2D>();
            _boxCollider2DUp.isTrigger = true;

            _boxCollider2DDown = gameObject.AddComponent<BoxCollider2D>();
            _boxCollider2DDown.isTrigger = true;
        }

        private void Start()
        {
            SetBoard(canMove, "board", 40, transform.position);
        }

        public void SetBoard(bool canMover, string setName, int layer, Vector3 startLocalPosition)
        {
            canMove = canMover;
            transform.name = setName;
            _spriteRenderer.sortingOrder = layer;
            transform.localPosition = startLocalPosition;
            SetOriginal();
        }

        private void SetOriginal()
        {
            ChangeMove();
            _edgeCollider2D.isTrigger = true;
            List<Vector2> points = new List<Vector2> { new Vector2(-_spriteRenderer.size.x / 2, 0.025f), new Vector2(_spriteRenderer.size.x / 2, 0.025f) };
            _edgeCollider2D.SetPoints(points);
            _boxCollider2DUp.size = new Vector2(_spriteRenderer.size.x, 3);
            _boxCollider2DUp.offset = new Vector2(0, 1.5f);
            _boxCollider2DDown.size = new Vector2(_spriteRenderer.size.x, 3);
            _boxCollider2DDown.offset = new Vector2(0, -1.5f);
        }

        private void Update()
        {
            if (keepEdge)
            {
                List<Vector2> points = new List<Vector2> { new Vector2(-_spriteRenderer.size.x / 2, 0.025f), new Vector2(_spriteRenderer.size.x / 2, 0.025f) };
                _edgeCollider2D.SetPoints(points);
                _boxCollider2DUp.size = new Vector2(_spriteRenderer.size.x, 3);
                _boxCollider2DDown.size = new Vector2(_spriteRenderer.size.x, 3);

                _spriteRenderer.size = new Vector2(width, 0.5f);
            }

            //if (test)  transform.position = new Vector3(Time.time, transform.position.y);
        }

        public void ChangeMove(bool isChange = false)
        {
            if (isChange)
                canMove = !canMove;
            if (!canMove)
                _spriteRenderer.sprite = Resources.Load<Sprite>("Sprites/Board/Board_unmove");
            else
                _spriteRenderer.sprite = Resources.Load<Sprite>("Sprites/Board/Board_move");
        }

        private void OnTriggerStay2D(Collider2D collision)
        {
            if (collision.CompareTag("Player"))
            {
                if (_boxCollider2DDown.IsTouching(collision))//进入的是下面
                    _edgeCollider2D.isTrigger = true;
                else if (_boxCollider2DUp.IsTouching(collision))
                    _edgeCollider2D.isTrigger = false;
            }
        }

        /*
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")
        {
            edgeCollider2D.isTrigger = true;
        }
    }
    */
    }
}
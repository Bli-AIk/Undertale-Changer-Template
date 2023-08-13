using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// �趨���壬����������BattlePlayerController�ڿ���
/// </summary>
public class BoardController : MonoBehaviour
{
    [Header("���")]
    public float width = 2.1f;
    [Header("�Ƿ�Ϊ���ٰ�")]
    public bool canMove;
    [Header("�Ƿ��ñ�Ե��ײ��������sprite��ȶ��仯")]
    public bool keepEdge;
    public List<Sprite> boards;
    BoxCollider2D boxCollider2DUp, boxCollider2DDown;//�����ļ���� ������������Ͱ�EdgeCollider���ˡ�������BattlePlayerController�ڿ�
    EdgeCollider2D edgeCollider2D;//Ĭ��Ϊ��������
    SpriteRenderer spriteRenderer;
    private void Awake()
    {
        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();

        edgeCollider2D = gameObject.AddComponent<EdgeCollider2D>();

        spriteRenderer.drawMode = SpriteDrawMode.Sliced;
        spriteRenderer.size = new Vector2(width, 0.5f);

        boxCollider2DUp = gameObject.AddComponent<BoxCollider2D>();
        boxCollider2DUp.isTrigger = true;

        boxCollider2DDown = gameObject.AddComponent<BoxCollider2D>();
        boxCollider2DDown.isTrigger = true;

    }
    private void Start()
    {
        //SetBoard(canMove, "board", 40, transform.position);
    }
    public void SetBoard(bool canMover, string setName, int layer, Vector3 startLocalPosition)
    {
        canMove = canMover;
        transform.name = setName;
        spriteRenderer.sortingOrder = layer;
        transform.localPosition = startLocalPosition;
        SetOriginal();
    }
    void SetOriginal()
    {
        ChangeMove();
        edgeCollider2D.isTrigger = true;
        List<Vector2> points = new List<Vector2>() { new Vector2(-spriteRenderer.size.x / 2, 0.025f), new Vector2(spriteRenderer.size.x / 2, 0.025f) };
        edgeCollider2D.SetPoints(points);
        boxCollider2DUp.size = new Vector2(spriteRenderer.size.x, 3);
        boxCollider2DUp.offset = new Vector2(0, 1.5f);
        boxCollider2DDown.size = new Vector2(spriteRenderer.size.x, 3);
        boxCollider2DDown.offset = new Vector2(0, -1.5f);


    }

    
    void Update()
    {
        if (keepEdge)
        {
            List<Vector2> points = new List<Vector2>() { new Vector2(-spriteRenderer.size.x / 2, 0.025f), new Vector2(spriteRenderer.size.x / 2, 0.025f) };
            edgeCollider2D.SetPoints(points);
            boxCollider2DUp.size = new Vector2(spriteRenderer.size.x, 3);
            boxCollider2DDown.size = new Vector2(spriteRenderer.size.x, 3);

            spriteRenderer.size = new Vector2(width, 0.5f);
        }
    }
  

    public void ChangeMove(bool isChange = false)
    {
        if (isChange)
            canMove = !canMove;
        if (!canMove)
            spriteRenderer.sprite = Resources.Load<Sprite>("Sprites/Board/Board_unmove");
        else
            spriteRenderer.sprite = Resources.Load<Sprite>("Sprites/Board/Board_move");
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (boxCollider2DDown.IsTouching(collision))//�����������
                edgeCollider2D.isTrigger = true;
            else if (boxCollider2DUp.IsTouching(collision))
                edgeCollider2D.isTrigger = false;
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

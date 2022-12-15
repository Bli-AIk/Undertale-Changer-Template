using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 设定挡板，具体数据在BattlePlayerController内控制
/// </summary>
public class BoardController : MonoBehaviour
{
    [Header("是否为跟踪板")]
    public bool canMove;
    [Header("是否让边缘碰撞器长度随sprite宽度而变化")]
    public bool keepEdge;
    public List<Sprite> boards;
    public BoxCollider2D boxCollider2DUp, boxCollider2DDown;//纯纯的检测器 检测玩家在上面就把EdgeCollider掐了。具体在BattlePlayerController内控
    public EdgeCollider2D edgeCollider2D;//默认为触发器。
    SpriteRenderer spriteRenderer;
    // Start is called before the first frame update
    void SetOriginal(bool isNum)
    {
        if (!isNum)
        {
            edgeCollider2D = GetComponent<EdgeCollider2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            boxCollider2DUp = GetComponents<BoxCollider2D>()[0];
            boxCollider2DDown = GetComponents<BoxCollider2D>()[1];
        }
        else
        {
            ChangeMove();
            edgeCollider2D.isTrigger = true;

            edgeCollider2D.points[0].x = -spriteRenderer.size.x / 2;
            edgeCollider2D.points[1].x = spriteRenderer.size.x / 2;
            boxCollider2DUp.size = new Vector2(spriteRenderer.size.x, 3);
            boxCollider2DDown.size = new Vector2(spriteRenderer.size.x, 3);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (keepEdge)
        {
            edgeCollider2D.points[0] = new Vector2(-spriteRenderer.size.x / 2, 0.025f);
            edgeCollider2D.points[1] = new Vector2(spriteRenderer.size.x / 2, 0.025f);
            boxCollider2DUp.size = new Vector2(spriteRenderer.size.x, 3);
            boxCollider2DDown.size = new Vector2(spriteRenderer.size.x, 3);
        }
    }
    public void SetBoard(string canMover,string setName,string layer,string startPosition)
    {
        SetOriginal(false);
        canMove = bool.Parse(canMover);
        transform.name = setName;
        spriteRenderer.sortingOrder = int.Parse(layer);
        transform.localPosition = MainControl.instance.StringVector2ToRealVector2(startPosition, transform.localPosition);
        SetOriginal(true);
    }

    public void ChangeMove(bool isChange = false)
    {
        if (isChange)
            canMove = !canMove;
        if (!canMove)
            spriteRenderer.sprite = boards[0];
        else
            spriteRenderer.sprite = boards[1];
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            if (boxCollider2DDown.IsTouching(collision))//进入的是下面
                edgeCollider2D.isTrigger = true;
            else if (boxCollider2DUp.IsTouching(collision))
                edgeCollider2D.isTrigger = false;
        }
    }
    
    /*
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            edgeCollider2D.isTrigger = true;
            
        }
    }
    */
}

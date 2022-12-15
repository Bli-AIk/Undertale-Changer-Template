using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Runtime.InteropServices;
/// <summary>
/// 回合编辑器子控
/// 文字类与TMP放置于同一Obj内
/// </summary>
public class RoundEditorObjController : MonoBehaviour
{
    //intTmp为引用data行数，selent为选择数。此两项内任意一项若选择负数则为不引用该项。
    public List<int> intTmp;
    public int intSelent;
    RoundEditorController roundEditorController;
    TextMeshPro tmp;
    SpriteRenderer tmpSprParent;
    [Header("父级颜色伴随 若勾选会自动查找父级SpriteRenderer")]
    public bool colorTmpPlusParent;
    [Header("特殊字符转换")]
    public bool spExchange;

    /*
    [Header("鼠标按下selent不变(多用于创建相关)")]
    public bool mouseMode;
    */
    private void Start()
    {
        roundEditorController = GameObject.Find("Editor").GetComponent<RoundEditorController>();
        tmp = GetComponent<TextMeshPro>();

    }
    private void Update()
    {

        if (colorTmpPlusParent && tmpSprParent == null)
        {
            tmpSprParent = transform.parent.GetComponent<SpriteRenderer>();
        }

        if (intSelent == roundEditorController.selent && intSelent >= 0)
        {
            tmp.color = new Color(1, 1, 0, 1);
            if (colorTmpPlusParent)
                tmpSprParent.color = new Color(1, 1, 0, 1);
        }
        else if (tmp.color != Color.white)
        {
            tmp.color = Color.white;
            if (colorTmpPlusParent)
                tmpSprParent.color = Color.white;

        }

    }
    /// <summary>
    /// 鼠标输入
    /// </summary>
    private void OnMouseEnter()
    {
        if (roundEditorController.selent != intSelent && roundEditorController.inputNum <= 0)
        {
            roundEditorController.selent = intSelent;
            AudioController.instance.GetFx(0, MainControl.instance.AudioControl.fxClipUI);

        }
    }

    private void OnMouseOver()
    {
        if(Input.GetMouseButtonDown(0) && roundEditorController.selent <= roundEditorController.selentMax)
        {
            if (roundEditorController.inputNum <= 0)
                roundEditorController.DownKey(/*mouseMode*/);
        }
        else if (roundEditorController.inputNum <= 0)
        {
          
            if (Input.GetMouseButtonDown(0))
            {
                if (!roundEditorController.fileSaver.files[roundEditorController.selent - roundEditorController.selentMax - 1].GetComponent<RoundEditorFileController>().isSelent)
                {

                    roundEditorController.fileSaver.files[roundEditorController.selent - roundEditorController.selentMax - 1].GetComponent<RoundEditorFileController>().isSelent = true;
                    roundEditorController.isSelentOne++;

                    AudioController.instance.GetFx(2, MainControl.instance.AudioControl.fxClipBattle);
                    if (roundEditorController.isSelentOne == 1)
                        roundEditorController.FindOneSelentNum();
                }
                else
                {
                    roundEditorController.AnimToGo(true);
                    AudioController.instance.GetFx(4, MainControl.instance.AudioControl.fxClipUI);
                }
            }
            else if (Input.GetMouseButtonDown(1))
            {
                roundEditorController.fileSaver.files[roundEditorController.selent - roundEditorController.selentMax - 1].GetComponent<RoundEditorFileController>().isSelent = false;
                roundEditorController.isSelentOne--; 
                if (roundEditorController.isSelentOne == 1)
                    roundEditorController.FindOneSelentNum();
            }

        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Runtime.InteropServices;
/// <summary>
/// �غϱ༭���ӿ�
/// ��������TMP������ͬһObj��
/// </summary>
public class RoundEditorObjController : MonoBehaviour
{
    //intTmpΪ����data������selectΪѡ������������������һ����ѡ������Ϊ�����ø��
    public List<int> intTmp;
    public int intSelect;
    RoundEditorController roundEditorController;
    TextMeshPro tmp;
    SpriteRenderer tmpSprParent;
    [Header("������ɫ���� ����ѡ���Զ����Ҹ���SpriteRenderer")]
    public bool colorTmpPlusParent;
    [Header("�����ַ�ת��")]
    public bool spExchange;

    /*
    [Header("��갴��select����(�����ڴ������)")]
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

        if (intSelect == roundEditorController.select && intSelect >= 0)
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
    /// �������
    /// </summary>
    private void OnMouseEnter()
    {
        if (roundEditorController.select != intSelect && roundEditorController.inputNum <= 0)
        {
            roundEditorController.select = intSelect;
            AudioController.instance.GetFx(0, MainControl.instance.AudioControl.fxClipUI);

        }
    }

    private void OnMouseOver()
    {
        if(Input.GetMouseButtonDown(0) && roundEditorController.select <= roundEditorController.selectMax)
        {
            if (roundEditorController.inputNum <= 0)
                roundEditorController.DownKey(/*mouseMode*/);
        }
        else if (roundEditorController.inputNum <= 0)
        {
          
            if (Input.GetMouseButtonDown(0))
            {
                if (!roundEditorController.fileSaver.files[roundEditorController.select - roundEditorController.selectMax - 1].GetComponent<RoundEditorFileController>().isSelect)
                {

                    roundEditorController.fileSaver.files[roundEditorController.select - roundEditorController.selectMax - 1].GetComponent<RoundEditorFileController>().isSelect = true;
                    roundEditorController.isSelectOne++;

                    AudioController.instance.GetFx(2, MainControl.instance.AudioControl.fxClipBattle);
                    if (roundEditorController.isSelectOne == 1)
                        roundEditorController.FindOneSelectNum();
                }
                else
                {
                    roundEditorController.AnimToGo(true);
                    AudioController.instance.GetFx(4, MainControl.instance.AudioControl.fxClipUI);
                }
            }
            else if (Input.GetMouseButtonDown(1))
            {
                roundEditorController.fileSaver.files[roundEditorController.select - roundEditorController.selectMax - 1].GetComponent<RoundEditorFileController>().isSelect = false;
                roundEditorController.isSelectOne--; 
                if (roundEditorController.isSelectOne == 1)
                    roundEditorController.FindOneSelectNum();
            }

        }
    }
}

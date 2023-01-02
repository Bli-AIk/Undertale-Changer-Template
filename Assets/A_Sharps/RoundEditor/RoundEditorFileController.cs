using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoundEditorFileController : MonoBehaviour
{
    public bool isSelect, isRename;
    SpriteRenderer spriteRenderer;
    RoundEditorController roundEditorController;
    RoundEditorObjController objController;
    public int selectNum;
    public bool isFolder;
    public List<Sprite> sprites;
    private void Start()
    {
        objController = transform.Find("UIText").GetComponent<RoundEditorObjController>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        roundEditorController = GameObject.Find("Editor").GetComponent<RoundEditorController>();
        foreach (var item in transform.parent.GetComponent<RoundEditorFileSaver>().files)
        {
            selectNum++;
            if (item == this)
                break;
            
        }
        spriteRenderer.sprite = sprites[Convert.ToInt32(isFolder)];
        objController.intSelect = selectNum + roundEditorController.selectMax;
    }

    // Update is called once per frame
    void Update()
    {
        if (isRename && roundEditorController.inputNum != selectNum)
        {
            isRename = false;
            spriteRenderer.color = Color.white;
        }
            

        if (isRename) 
        { 
            spriteRenderer.color = Color.blue;
        }
        else if (isSelect)
        {
            spriteRenderer.color = Color.red;
        }
        else if (spriteRenderer.color == Color.red)
        {
            if (objController.intSelect == roundEditorController.select)
                spriteRenderer.color = new Color(1, 1, 0, 1);
            else spriteRenderer.color = Color.white;
        }
        if (isRename)
        {
            objController.colorTmpPlusParent = false;
        }
        else
            objController.colorTmpPlusParent = !isSelect;
    }
}

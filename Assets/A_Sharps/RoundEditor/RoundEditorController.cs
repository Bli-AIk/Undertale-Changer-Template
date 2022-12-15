using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// 回合编辑器总控
/// </summary>
public class RoundEditorController : MonoBehaviour
{
    [Header("TMP导入")]
    public List<string> findTmp;//根据名称查询文字子级并通过RoundEditorObjController内的数值赋值语言包的文字内容

    [Header("目前页面 0初始化 1初始化编辑 2回合编辑 3曲线编辑")]
    public int editMode;

    public int selent;
    [Header("需要手动输入默认最大值")]
    public int selentMax;
    [Header("改名编号 0为不改名状态 初始化编辑模式内则1为地址 2为偏移 3为大小 4为伤害")]
    public int inputNum;
    [Header("有一个选定则1")]
    public int isSelentOne;
    [Header("上面的这一个选定的编号")]
    public int isSelentNum;

    [Header("初始化编辑：是否已设定Spr")]
    public bool isSetSpr;
    [Header("初始化编辑：选定碰撞编号")]
    public int boxLine;
    [Header("初始化编辑：控制模式")]
    public bool isControlBox;

    public int controlBoxMode;//0偏移1大小2伤害
    public float controlBoxBase;

    [Header("初始化编辑：遮罩模式")]
    public int maskMode;

    [Header(" ")]
    public RoundEditorFileSaver fileSaver;
    TextMeshPro uiNothing, reFlash, uiRes;
    InputField inputField;
    NoEditBulletController editBulletSpr;
    BulletController editBulletController;
    float boxWidth;//保存box宽度
    /*
    [System.Runtime.InteropServices.DllImport("user32.dll")]
    public static extern int SetCursorPos(int x, int y);
    */

    void GiveTmp(int tmpNum = 0)
    {
        for (int i = 0; i < findTmp.Count; i++)
        {
            GameObject obj = transform.Find(findTmp[i]).gameObject;
            int objTmp = obj.GetComponent<RoundEditorObjController>().intTmp[tmpNum];
            if (objTmp >= 0)
            {
                string text = MainControl.instance.BattleControl.roundEditorMax[objTmp];
                if (obj.GetComponent<RoundEditorObjController>().spExchange)
                    text = MainControl.instance.ChangeItemData(new List<string> { text }, true, new List<string>())[0];


                obj.GetComponent<TextMeshPro>().text = text.Substring(0, text.Length - 1);
            }
        }
    }

    [System.Obsolete]
    void Start()
    {
        isSetSpr = false;
        editMode = 0;
        editBulletSpr = GameObject.Find("EditBulletSpr").GetComponent<NoEditBulletController>();
        editBulletController = editBulletSpr.GetComponent<BulletController>();
        fileSaver = transform.Find("FileSave").GetComponent<RoundEditorFileSaver>();
        uiNothing = transform.Find("UINothingTip").GetComponent<TextMeshPro>();
        reFlash = transform.Find("UIFrameText/Refresh/Text UI").GetComponent<TextMeshPro>();
        inputField = GameObject.Find("Canvas/InputText").GetComponent<InputField>();
        uiRes = transform.Find("UIRes").GetComponent<TextMeshPro>();
        inputField.onValueChanged.AddListener(InputListener);
        inputField.onEndEdit.AddListener(ExitListener);
        inputField.enabled = false;
        GiveTmp(0);
        editBulletSpr.gameObject.SetActive(false);
    }
    void InputListener(string value)
    {
        if (editMode == 0)
            fileSaver.files[inputNum - 1].transform.Find("UIText").GetComponent<TextMeshPro>().text = value;
        else
        {
            uiRes.text = value;
        }

        AudioController.instance.GetFx(0, MainControl.instance.AudioControl.fxClipType);
    }
    [System.Obsolete]
    IEnumerator WWWLoadSprite(string url)
    {
        WWW www = new WWW("file://" + Application.dataPath + "/" + url);
        yield return www;
        if (string.IsNullOrEmpty(www.error) == false)
        {
            Debug.Log("error");
            AudioController.instance.GetFx(5, MainControl.instance.AudioControl.fxClipUI);
            uiRes.color = Color.red;
            isSetSpr = false;
        }
        else
        {
            isSetSpr = true;
            uiRes.color = Color.white;
            AudioController.instance.GetFx(4, MainControl.instance.AudioControl.fxClipUI);
        }
        Texture2D tex = www.texture;
        tex.filterMode = FilterMode.Point;
        Sprite spr = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.one * 0.5f, 20, 1, SpriteMeshType.FullRect);

        GiveeditBulletSpr(spr);
    }
    void GiveeditBulletSpr(Sprite sprite)
    {
        editBulletSpr.GetComponent<SpriteRenderer>().sprite = sprite;
    }
    [System.Obsolete]
    void ExitListener(string value)
    {
        if (editMode == 0)
        {
            fileSaver.files[inputNum - 1].transform.Find("UIText").GetComponent<TextMeshPro>().text = value;
            AudioController.instance.GetFx(3, MainControl.instance.AudioControl.fxClipUI);
            inputNum = 0;
        }
        else//编辑弹幕
        {
            uiRes.text = value;
            uiRes.color = Color.white;

            if (value.Length > 5)
            {

                if (value.Substring(0, 5) == "Res:/" || value.Substring(0, 5) == "Res:/")// 路径不需要.png!!!不需要.png!!!不需要.png!!!不需要.png!!!不需要.png!!!
                {
                    editBulletSpr.isSelentSprite = true;
                    editBulletSpr.GetComponent<SpriteRenderer>().sprite = null;
                    value = value.Substring(5);

                    if (value[value.Length - 4] == '.' && value[value.Length - 3] == 'p' && value[value.Length - 2] == 'n' && value[value.Length - 1] == 'g')//png修正
                        value = value.Substring(0, value.Length - 4);

                    GiveeditBulletSpr(Resources.Load<Sprite>(value));


                    if (editBulletSpr.GetComponent<SpriteRenderer>().sprite == null)
                    {
                        uiRes.color = Color.red;
                        AudioController.instance.GetFx(5, MainControl.instance.AudioControl.fxClipUI);
                        isSetSpr = false;
                    }
                    else
                    {
                        AudioController.instance.GetFx(4, MainControl.instance.AudioControl.fxClipUI);

                        isSetSpr = true;
                    }
                }
                else if (value.Substring(0, 5) == "Str:/" || value.Substring(0, 5) == "Str:/")// 路径需要.png!!!需要.png!!!需要.png!!!需要.png!!!需要.png!!!
                {
                    editBulletSpr.isSelentSprite = true;
                    editBulletSpr.GetComponent<SpriteRenderer>().sprite = null;
                    value = value.Substring(5);
                    if (value[value.Length - 4] != '.' && value[value.Length - 3] != 'p' && value[value.Length - 2] != 'n' && value[value.Length - 1] != 'g') //png修正
                        value += ".png";

                    StartCoroutine(WWWLoadSprite(value));
                }
                else if (value.Substring(0, 5) == "Set:/" || value.Substring(0, 5) == "Set:/")
                {
                    value = value.Substring(5);
                    editBulletSpr.transform.localScale = MainControl.instance.StringVector2ToRealVector2(value, editBulletSpr.transform.localScale);
                }
                else if (float.Parse(value.Substring(5)) > 0 && (value.Substring(0, 5) == "Box:/" || value.Substring(0, 5) == "Box:/"))
                {
                    for (int i = 0; i < editBulletSpr.transform.childCount; i++)
                    {
                        Sprite spr = Resources.Load<Sprite>("Sprites/BoxEdge");
                        Texture2D tex = spr.texture;

                        boxWidth = float.Parse(value.Substring(5));

                        editBulletSpr.transform.GetChild(i).GetComponent<SpriteRenderer>().sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.one * 0.5f, boxWidth, 1, SpriteMeshType.FullRect, Vector4.one);
                    }
                }
                else if (editBulletController.boxColliderList.Count > 0 && (value.Substring(0, 5) == "Off:/" || value.Substring(0, 5) == "Off:/"))
                {
                    if (value[value.Length - 1] == 'c' || value[value.Length - 1] == 'C')
                    {
                        isControlBox = true;
                        controlBoxMode = 0;

                        controlBoxBase = float.Parse(MainControl.instance.SubText(value.Substring(5)));
                    }
                    else
                    {
                        editBulletController.boxColliderList[boxLine - 1].offset = MainControl.instance.StringVector2ToRealVector2(value.Substring(5), editBulletController.boxColliderList[boxLine - 1].offset);
                        SetUiNothing(editBulletController);
                    }
                }
                else if (editBulletController.boxColliderList.Count > 0 && (value.Substring(0, 5) == "Siz:/" || value.Substring(0, 5) == "Siz:/"))
                {
                    if (value[value.Length - 1] == 'c' || value[value.Length - 1] == 'C')
                    {
                        isControlBox = true;
                        controlBoxMode = 1;
                        controlBoxBase = float.Parse(MainControl.instance.SubText(value.Substring(5)));
                    }
                    else
                    {
                        editBulletController.boxColliderList[boxLine - 1].size = MainControl.instance.StringVector2ToRealVector2(value.Substring(5), editBulletController.boxColliderList[boxLine - 1].size);
                        SetUiNothing(editBulletController);
                    }
                }
                else if (editBulletController.boxHitList.Count > 0 && (value.Substring(0, 5) == "Hur:/" || value.Substring(0, 5) == "Hur:/"))
                {

                    if (value[value.Length - 1] == 'c' || value[value.Length - 1] == 'C')
                    {
                        isControlBox = true;
                        controlBoxMode = 2;
                        controlBoxBase = float.Parse(MainControl.instance.SubText(value.Substring(5)));
                    }
                    else
                    {
                        editBulletController.boxHitList[boxLine - 1] = int.Parse(value.Substring(5));
                        SetUiNothing(editBulletController);
                    }
                }
                else if (editBulletController.boxColliderList.Count > 0 && (value.Substring(0, 5) == "Col:/" || value.Substring(0, 5) == "Col:/")) 
                {
                    value = value.Substring(5) + ';';
                    List<string> list = new List<string>();
                    MainControl.instance.MaxToOneSon(value, list);
                    editBulletController.boxColliderList[boxLine - 1].offset = MainControl.instance.StringVector2ToRealVector2(list[0], editBulletController.boxColliderList[boxLine - 1].offset);
                    editBulletController.boxColliderList[boxLine - 1].size = MainControl.instance.StringVector2ToRealVector2(list[1], editBulletController.boxColliderList[boxLine - 1].size);
                    editBulletController.boxHitList[boxLine - 1] = int.Parse(list[2]);
                    SetUiNothing(editBulletController);


                }
            }
            else
                uiRes.color = Color.red;


            inputNum = -1;//防止撞键
        }
        if (value == "" || value == null)
        {
            inputField.text = MainControl.instance.BattleControl.roundEditorMax[9].Substring(0, MainControl.instance.BattleControl.roundEditorMax[9].Length - 1) + MainControl.instance.RandomName(Random.Range(1, 5));

        }
        inputField.enabled = false;


    }
    void SetUiNothing(BulletController bulletController,bool setSpr = true)
    {
        if (boxLine > 0)
        {
            uiNothing.text = "\n" + MainControl.instance.SubText(MainControl.instance.BattleControl.roundEditorMax[25]) + boxLine + "\n"
            + MainControl.instance.SubText(MainControl.instance.BattleControl.roundEditorMax[26]) + "(" + bulletController.boxColliderList[boxLine - 1].offset.x.ToString("F4") + "),("
            + bulletController.boxColliderList[boxLine - 1].offset.y.ToString("F4") + ")\n"
            + MainControl.instance.SubText(MainControl.instance.BattleControl.roundEditorMax[27]) + "(" + bulletController.boxColliderList[boxLine - 1].size.x.ToString("F4") + "),("
            + bulletController.boxColliderList[boxLine - 1].size.y.ToString("F4") + ")\n"
            + MainControl.instance.SubText(MainControl.instance.BattleControl.roundEditorMax[28]) + bulletController.boxHitList[boxLine - 1] + "\n";
            if (setSpr)
                editBulletSpr.getBoxs[boxLine - 1].GetComponent<BulletBoxLineController>().SetSize(boxLine - 1);

        }
        else
            uiNothing.text = MainControl.instance.SubText(MainControl.instance.BattleControl.roundEditorMax[29]);

    }
    Vector2 ControlBox(Vector2 v)
    {
        if (MainControl.instance.KeyArrowToControl(KeyCode.UpArrow))
        {
            v += new Vector2(0, controlBoxBase);
        }
        else if (MainControl.instance.KeyArrowToControl(KeyCode.DownArrow))
        {
            v -= new Vector2(0, controlBoxBase);
        }

        if (MainControl.instance.KeyArrowToControl(KeyCode.LeftArrow))
        {
            v -= new Vector2(controlBoxBase, 0);
        }
        else if (MainControl.instance.KeyArrowToControl(KeyCode.RightArrow))
        {
            v += new Vector2(controlBoxBase,0);
        }

        

        return v;
    }
    void Update()
    {
        if (isControlBox)
        {
            if (MainControl.instance.KeyArrowToControl(KeyCode.X))
            {
                isControlBox = false;
                AudioController.instance.GetFx(3, MainControl.instance.AudioControl.fxClipUI);
            }
            Vector2 old = new Vector2();
            switch (controlBoxMode)
            {
                case 0://偏移
                    old = editBulletController.boxColliderList[boxLine - 1].offset;
                    editBulletController.boxColliderList[boxLine - 1].offset = ControlBox(editBulletController.boxColliderList[boxLine - 1].offset);
                    if (old != editBulletController.boxColliderList[boxLine - 1].offset)
                        SetUiNothing(editBulletController);
                    break;
                case 1://大小
                    old = editBulletController.boxColliderList[boxLine - 1].size;
                    editBulletController.boxColliderList[boxLine - 1].size = ControlBox(editBulletController.boxColliderList[boxLine - 1].size);
                    if (old != editBulletController.boxColliderList[boxLine - 1].size)
                        SetUiNothing(editBulletController);
                    break;
                case 2://伤害

                    if (MainControl.instance.KeyArrowToControl(KeyCode.UpArrow)|| MainControl.instance.KeyArrowToControl(KeyCode.LeftArrow))
                    {
                        editBulletController.boxHitList[boxLine - 1] -= (int)controlBoxBase;
                        SetUiNothing(editBulletController);
                    }
                    else if (MainControl.instance.KeyArrowToControl(KeyCode.DownArrow)|| MainControl.instance.KeyArrowToControl(KeyCode.RightArrow))
                    {
                        editBulletController.boxHitList[boxLine - 1] += (int)controlBoxBase;
                        SetUiNothing(editBulletController);
                    }

                    break;
            }
        }



        if (inputNum <= 0)
        {
            if (!isControlBox)
                InputKey();

            if (selent <= selentMax)
            {
                if (MainControl.instance.KeyArrowToControl(KeyCode.Z))
                {
                    DownKey();

                }

            }
            else
            {
                if (MainControl.instance.KeyArrowToControl(KeyCode.Z))
                {
                    if (!fileSaver.files[selent - selentMax - 1].GetComponent<RoundEditorFileController>().isSelent)
                    {
                        fileSaver.files[selent - selentMax - 1].GetComponent<RoundEditorFileController>().isSelent = true;
                        isSelentOne++;
                        if (isSelentOne == 1)
                            FindOneSelentNum();
                        AudioController.instance.GetFx(2, MainControl.instance.AudioControl.fxClipBattle);
                    }
                    else
                    {
                        AnimToGo(true);

                        AudioController.instance.GetFx(4, MainControl.instance.AudioControl.fxClipUI);

                        selent = 0;
                    }
                }
                else if (MainControl.instance.KeyArrowToControl(KeyCode.X))
                {
                    fileSaver.files[selent - selentMax - 1].GetComponent<RoundEditorFileController>().isSelent = false;
                    isSelentOne--;
                    if (isSelentOne == 1)
                        FindOneSelentNum();
                }
                if ((MainControl.instance.KeyArrowToControl(KeyCode.C)) && fileSaver.files[selent - selentMax - 1].GetComponent<RoundEditorFileController>().isSelent && editMode == 0)
                {
                    ReFileName(fileSaver.files[selent - selentMax - 1]);

                    fileSaver.files[selent - selentMax - 1].GetComponent<RoundEditorFileController>().isSelent = false;
                    isSelentOne--;
                    if (isSelentOne == 1)
                        FindOneSelentNum();
                }
            }
        }
        else if (editMode == 0)
        {
            fileSaver.files[inputNum - 1].GetComponent<RoundEditorFileController>().isRename = true;
        }

        /*
        if (Input.GetKeyDown(KeyCode.L))
        {
            ReadFile();
        }
        */
    }

    /// <summary>
    /// 执行动画 到达左F/右T侧
    /// </summary>
    public void AnimToGo(bool isDirRight)
    {
        if (isDirRight)
        {

            editBulletSpr.gameObject.SetActive(true);
            fileSaver.files[selent - selentMax - 1].GetComponent<RoundEditorFileController>().isSelent = false;
            isSelentOne = 0;
            isSelentNum = 0;
            uiNothing.transform.localPosition = new Vector3(11.85f, -10, -5.5f);
            uiNothing.text = MainControl.instance.SubText(MainControl.instance.BattleControl.roundEditorMax[29]);

            uiNothing.gameObject.SetActive(true);
            //anim
            editMode = 1;
            GameObject.Find("Main Camera").transform.DOMoveX(13.5f, 1.5f).SetEase(Ease.OutCubic);
            Transform UIFrameText = transform.Find("UIFrameText");
            void OnKill()
            {
                GiveTmp(1);
                uiRes.text = MainControl.instance.BattleControl.roundEditorMax[30].Substring(0, MainControl.instance.BattleControl.roundEditorMax[30].Length - 1);
                uiRes.transform.localPosition = new Vector3(18.65f, 8f);
                uiRes.transform.DOMoveY(2.45f, 0.75f).SetEase(Ease.OutCubic);
                UIFrameText.transform.localPosition = new Vector2(23.5f + 4, 0);
                UIFrameText.DOMoveX(23.5f, 0.75f).SetEase(Ease.OutCubic);
                transform.Find("Frame/Line (1)").transform.localPosition = new Vector3(16.845f + 5, -3);
                transform.Find("Frame/Line (1)").DOMoveX(16.845f, 0.75f * 0.75f * 0.75f).SetEase(Ease.OutCubic);
                transform.Find("Frame/Line (2)").DOMove(new Vector3(8.5f, 12, 0), 0.6f).SetEase(Ease.OutCubic);
                transform.Find("UIText").DOMove(new Vector3(17, 2.45f), 0.75f).SetEase(Ease.OutCubic);

                reFlash.text = MainControl.instance.SubText(MainControl.instance.BattleControl.roundEditorMax[21]) + MainControl.instance.SubText(MainControl.instance.BattleControl.roundEditorMax[22 + maskMode]);

            }

            uiNothing.transform.DOMoveY(-3.65f, 1.5f).SetEase(Ease.OutCubic);
            UIFrameText.DOMoveX(-4, 0.75f).SetEase(Ease.OutCubic).OnKill(OnKill);
            transform.Find("Frame/Line (1)").DOMoveX(-7, 0.75f).SetEase(Ease.OutExpo);
            transform.Find("Frame/Line (2)").DOMove(new Vector3(6.75f, 13, 0), 0.75f).SetEase(Ease.OutCubic);
            transform.Find("UIRes").DOMoveX(-8.5f, 0.75f).SetEase(Ease.OutCubic);
            transform.Find("UIText").DOMove(new Vector3(16.125f, 4), 0.5f).SetEase(Ease.OutCubic);

        }
    }

    public void FindOneSelentNum()
    {
        foreach (var item in fileSaver.files)
        {
            if (item.GetComponent<RoundEditorFileController>().isSelent)
            {
                isSelentNum = item.GetComponent<RoundEditorFileController>().selentNum - 1;
                break;
            }
        }
    }
    int editModeSelentTwo = 31;//编辑弹幕中的播放提示
    public void DownKey(/*bool mouseMode = false*/)
    {
        AudioController.instance.GetFx(1, MainControl.instance.AudioControl.fxClipUI);
        bool isFolder = false;

        if (editMode == 0)
            switch (selent)
            {
                case 1:
                    isFolder = false;
                    goto case 100000;
                case 2:
                    isFolder = true;
                    goto case 100000;
                case 100000://新建
                    GameObject newFile = fileSaver.GetFromPool();
                    fileSaver.files.Add(newFile);
                    if (fileSaver.files.Count == 1)
                    {
                        newFile.transform.localPosition = new Vector3(-1.65f, 3.25f, 0);
                        uiNothing.gameObject.SetActive(false);

                    }
                    else if ((fileSaver.files.Count - 1) % 3 != 0)
                    {
                        newFile.transform.localPosition = fileSaver.files[fileSaver.files.Count - 2].transform.position + new Vector3(3.325f, 0, 0);
                    }
                    else newFile.transform.localPosition = fileSaver.files[fileSaver.files.Count - 2].transform.position - new Vector3(2 * 3.325f, 2.475f, 0);
                    newFile.GetComponent<RoundEditorFileController>().isFolder = isFolder;
                    selent = fileSaver.files.Count + selentMax;
                    ReFileName(newFile, true, false, isFolder);
                    break;
                case 3:
                    if (isSelentOne == 1)
                    {
                        selent = isSelentNum + selentMax + 1;
                        ReFileName(fileSaver.files[isSelentNum], false, true);
                        fileSaver.files[isSelentNum].GetComponent<RoundEditorFileController>().isSelent = false;

                    }
                    break;

            }
        else if (editMode == 1)
        {
            switch (selent)
            {
                case 0://返回
                    goto default;
                case 1:
                    if (inputNum == 0)
                    {
                        inputField.transform.localPosition = new Vector3(14.4f, 5);
                        inputField.GetComponent<RectTransform>().sizeDelta = new Vector2(11.75f, 1);
                        uiRes.color = Color.blue;
                        inputNum = 1;
                        AudioController.instance.GetFx(3, MainControl.instance.AudioControl.fxClipBattle);
                        inputField.text = uiRes.text;
                        inputField.enabled = true;

                    }
                    else inputNum = 0;//防止撞enter键

                    break;
                case 2:
                    uiRes.color = Color.green;
                    if (editModeSelentTwo >= 41 || editModeSelentTwo < 31)
                        editModeSelentTwo = 31;
                    uiRes.text = MainControl.instance.SubText(MainControl.instance.BattleControl.roundEditorMax[editModeSelentTwo]);
                    editModeSelentTwo++;
                    goto default;
                case 3:
                    if (isSetSpr)
                    {
                        BulletBoxLineController newEditBullet = editBulletSpr.GetFromPool().GetComponent<BulletBoxLineController>();

                        //BoxCollider2D newBox = editBulletSpr.gameObject.AddComponent<BoxCollider2D>();
                        BoxCollider2D newBox = newEditBullet.GetComponent<BoxCollider2D>();
                        newBox.size = Vector2.one * 0.5f;
                        newBox.offset = Vector2.zero;

                        Sprite spr = Resources.Load<Sprite>("Sprites/BoxEdge");
                        Texture2D tex = spr.texture;
                        if (boxWidth <= 0)
                            boxWidth = 80;
                        newBox.GetComponent<SpriteRenderer>().sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.one * 0.5f, boxWidth, 1, SpriteMeshType.FullRect, Vector4.one);
                        editBulletController.boxColliderList.Add(newBox);
                        editBulletController.boxHitList.Add(0);
                        boxLine++;

                        newEditBullet.setNum = editBulletController.boxHitList.Count - 1;
                        newEditBullet.gameObject.SetActive(true);
                        editBulletSpr.getBoxs.Add(newEditBullet.gameObject);
                        SetUiNothing(editBulletController, false);
                    }
                    goto default;
                case 4:
                    if (isSetSpr && editBulletSpr.getBoxs.Count > 0)
                    {
                        editBulletSpr.ReturnPool(editBulletSpr.getBoxs[boxLine - 1]);
                        editBulletSpr.getBoxs.RemoveAt(boxLine - 1);
                        editBulletController.boxColliderList.RemoveAt(boxLine - 1);
                        editBulletController.boxHitList.RemoveAt(boxLine - 1);

                        boxLine--;
                        if (boxLine == 0 && editBulletSpr.getBoxs.Count > 0)
                            boxLine++;
                        
                        SetUiNothing(editBulletController, false);
                    }

                        goto default;
                case 5:
                    if (maskMode < 2)
                        maskMode++;
                    else maskMode = 0; 
                    reFlash.text = MainControl.instance.SubText(MainControl.instance.BattleControl.roundEditorMax[21]) + MainControl.instance.SubText(MainControl.instance.BattleControl.roundEditorMax[22 + maskMode]);

                    goto default;
                default:
                    inputNum = 0;
                        break;
            }
        }


    }
    void ReFileName(GameObject newFile, bool isNew = false, bool isSelent = false, bool isNewFolder = true)
    {
        if (!isSelent)
            inputNum = selent - selentMax;
        else
        {
            inputNum = isSelentNum + 1;

        }
        if (isNew)
        {
            int i = 1;
            string newFileText;
            if (isNewFolder)
                newFileText = MainControl.instance.BattleControl.roundEditorMax[8];
            else
                newFileText = MainControl.instance.BattleControl.roundEditorMax[10];
            newFileText = newFileText.Substring(0, newFileText.Length - 1);
            foreach (var item in fileSaver.files)
            {
                string text = item.transform.Find("UIText").GetComponent<TextMeshPro>().text;
                if (text.Length > newFileText.Length && text.Substring(0, newFileText.Length) == newFileText)
                {
                    i++;
                }
            }
            newFile.transform.Find("UIText").GetComponent<TextMeshPro>().text = newFileText + i;
            inputField.text = newFileText + i;
        }
        else
        {
            AudioController.instance.GetFx(3, MainControl.instance.AudioControl.fxClipBattle);
            inputField.text = newFile.transform.Find("UIText").GetComponent<TextMeshPro>().text;
        }
        inputField.enabled = true;
        inputField.transform.position = newFile.transform.position - new Vector3(0, 1.35f, 0);
        inputField.GetComponent<RectTransform>().sizeDelta = new Vector2(2.5f, 1);

    }

    /// <summary>
    /// 键盘输入
    /// </summary>
    void InputKey()
    {
        if (MainControl.instance.KeyArrowToControl(KeyCode.UpArrow) || MainControl.instance.KeyArrowToControl(KeyCode.LeftArrow))
        {
            if (selent > 0)
            {
                if (MainControl.instance.KeyArrowToControl(KeyCode.UpArrow))
                {
                    if (selent <= selentMax)
                        selent--;
                    else
                    {
                        selent -= 3;
                    }
                }
                else if (selent <= selentMax && fileSaver.files.Count > 0)
                {
                    if (editMode == 0)
                        selent = selentMax + 1;
                    else if (boxLine > 1)
                    {
                        boxLine--;
                        SetUiNothing(editBulletController, false);
                    }
                }
                else
                    selent--;
            }
            else if (editMode == 0)
                selent = selentMax + fileSaver.files.Count;
            else selent = selentMax;
            AudioController.instance.GetFx(0, MainControl.instance.AudioControl.fxClipUI);
        }
        else if (MainControl.instance.KeyArrowToControl(KeyCode.DownArrow) || (MainControl.instance.KeyArrowToControl(KeyCode.RightArrow)))
        {
            if (((selent < selentMax + fileSaver.files.Count && editMode == 0) || (selent < selentMax && editMode == 1)))
            {
                if (MainControl.instance.KeyArrowToControl(KeyCode.DownArrow))
                {
                    if (selent <= selentMax)
                        selent++;
                    else
                    {
                        selent += 3;
                        if (selent >= selentMax + fileSaver.files.Count)
                            selent = selentMax + fileSaver.files.Count;
                    }
                }
                else if (selent <= selentMax && fileSaver.files.Count > 0)
                {
                    if (editMode == 0)
                        selent = selentMax + 1;
                    else if (boxLine < editBulletSpr.getBoxs.Count) 
                    {
                        boxLine++;
                        SetUiNothing(editBulletController, false);
                    }
                }
                else
                    selent++;
            }
            else selent = 0;
            AudioController.instance.GetFx(0, MainControl.instance.AudioControl.fxClipUI);
        }
    }

    /// <summary>
    /// 读取RoundEditor下的文档(刷新)
    /// </summary>
    void ReadFile()
    {
        DirectoryInfo di = new DirectoryInfo(Application.dataPath);

        foreach (var file in di.GetFiles())
        {
            Debug.Log(file.Name);
        }
        //DirectoryInfo di = Directory.CreateDirectory(Application.dataPath + "/RoundEditor/新建文件夹 (" + Random.Range(0, 100000) + ")");

    }

}

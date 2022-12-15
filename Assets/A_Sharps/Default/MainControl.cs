using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.IO;
using System;
using System.Globalization;
/// <summary>
/// 调用所有ScriptableObject 并负责对数据和语言包的导入
/// 还包括一大堆常用的函数
/// </summary>
public class MainControl : MonoBehaviour
{
    [Header("状态 0正常 1战斗内 2编辑器")]
    public int inBattle;
    public static MainControl instance;
    public bool haveInOutBlack, noInBlack;
    public bool notPauseIn;
    Image inOutBlack;
    public OverwroldControl OverwroldControl { get; private set; }
    public ItemControl ItemControl { get; private set; }
    public PlayerControl PlayerControl { get; private set; }
    public AudioControl AudioControl { get; private set; }
    public BattleControl BattleControl { get; private set; }



    public void Initialization(int languagePack)
    {
        ItemControl = Resources.Load<ItemControl>("ItemControl");
        PlayerControl = Resources.Load<PlayerControl>("PlayerControl");
        AudioControl = Resources.Load<AudioControl>("AudioControl");
        OverwroldControl = Resources.Load<OverwroldControl>("OverwroldControl");
        if (inBattle != 0)
            BattleControl = Resources.Load<BattleControl>("BattleControl");

        if (languagePack < 0)//优先读取保存languagePack的数据
        {
            languagePack = OverwroldControl.languagePack;
        }
        if (languagePack >= Directory.GetDirectories(Application.dataPath + "\\LanguagePacks").Length + Directory.GetDirectories(Application.dataPath + "\\TextAssets\\LanguagePackage").Length)
        {
            languagePack = 0;
            OverwroldControl.languagePack = 0;
        }
       
            if (languagePack > Directory.GetDirectories(Application.dataPath + "\\TextAssets\\LanguagePackage").Length - 1)
                languagePack -= Directory.GetDirectories(Application.dataPath + "\\TextAssets\\LanguagePackage").Length;

            if (inBattle != 0)
            {
                BattleControl.roundDialogAsset = new List<string>();

                BattleControl.uiText = File.ReadAllText(Directory.GetDirectories(Application.dataPath + "\\TextAssets\\LanguagePackage")[languagePack] + "\\Battle\\UIBattleText.txt");
                for (int i = 0; i < Directory.GetFiles(Directory.GetDirectories(Application.dataPath + "\\TextAssets\\LanguagePackage")[languagePack] +"\\Battle\\Round").Length; i++)
                {
                    string file = Directory.GetFiles(Directory.GetDirectories(Application.dataPath + "\\TextAssets\\LanguagePackage")[languagePack] + "\\Battle\\Round")[i];
                    if (Directory.GetFiles(Directory.GetDirectories(Application.dataPath + "\\TextAssets\\LanguagePackage")[languagePack] + "\\Battle\\Round")[i]
                        .Substring(Directory.GetFiles(Directory.GetDirectories(Application.dataPath + "\\TextAssets\\LanguagePackage")[languagePack] + "\\Battle\\Round")[i].Length - 3) == "txt") 
                        BattleControl.roundDialogAsset.Add(File.ReadAllText(file));
                }

            BattleControl.roundEditorData = File.ReadAllText(Directory.GetDirectories(Application.dataPath + "\\TextAssets\\LanguagePackage")[languagePack] + "\\UI\\RoundEditor.txt");
            }
        OverwroldControl.menuAndSettingAsset = File.ReadAllText(Directory.GetDirectories(Application.dataPath + "\\TextAssets\\LanguagePackage")[languagePack] + "\\UI\\MenuAndSetting.txt");
        OverwroldControl.owTextsAsset = File.ReadAllText(Directory.GetDirectories(Application.dataPath + "\\TextAssets\\LanguagePackage")[languagePack] + "\\Overworld\\Overworld.txt");
        ItemControl.itemText = File.ReadAllText(Directory.GetDirectories(Application.dataPath + "\\TextAssets\\LanguagePackage")[languagePack] + "\\UI\\ItemText.txt");
     
        



        if (inBattle == 1)
        {
            LoadItemData(BattleControl.barrgeSetUpSave, BattleControl.barrgeSetUpAsset);

            LoadRound(GetComponent<RoundController>().round);

            LoadItemData(BattleControl.uiTextSave, BattleControl.uiText);
            ScreenMaxToOneSon(BattleControl.uiTextSave, BattleControl.actSave, "Act\\");
            ScreenMaxToOneSon(BattleControl.uiTextSave, BattleControl.mercySave, "Mercy\\");
            ScreenMaxToOneSon(BattleControl.uiTextSave, BattleControl.roundTextSave, "Round\\");
            BattleControl.roundTextSave = ChangeItemData(BattleControl.roundTextSave, true, new List<string>());

        }
        else if (inBattle == 2)
        {
            LoadItemData(BattleControl.roundEditorMax, BattleControl.roundEditorData);
        }


        LoadItemData(OverwroldControl.menuAndSettingSave, OverwroldControl.menuAndSettingAsset);
        LoadItemData(OverwroldControl.owTextsSave, OverwroldControl.owTextsAsset);
        LoadItemData(ItemControl.itemMax, ItemControl.itemData);
        LoadItemData(ItemControl.itemTextMax, ItemControl.itemText);

        MaxToSon(ItemControl.itemTextMax, new string[2] { "Data", "Item" }, new List<string>[2] { ItemControl.itemTextMaxData, ItemControl.itemTextMaxItem });
        ItemClassificatio();

        ItemControl.itemTextMaxData = ChangeItemData(ItemControl.itemTextMaxData, true, new List<string>());

        ItemControl.itemTextMaxItem = ChangeItemData(ItemControl.itemTextMaxItem, true, new List<string>());
        ItemControl.itemTextMaxItem = ChangeItemData(ItemControl.itemTextMaxItem, false, new List<string>());
        OverwroldControl.menuAndSettingSave = ChangeItemData(OverwroldControl.menuAndSettingSave, true, new List<string>());
        OverwroldControl.owTextsSave = ChangeItemData(OverwroldControl.owTextsSave, true, new List<string>());
        MaxToOneSon(ItemControl.itemTextMaxItem, ItemControl.itemTextMaxItemSon);



        if (OverwroldControl.textWidth != bool.Parse(ScreenMaxToOneSon(OverwroldControl.menuAndSettingSave, "LanguagePackFullWidth")))
        {
            OverwroldControl.textWidth = bool.Parse(ScreenMaxToOneSon(OverwroldControl.menuAndSettingSave, "LanguagePackFullWidth"));
            foreach (TextChanger textChanger in Resources.FindObjectsOfTypeAll(typeof(TextChanger)))
            {

                textChanger.width = OverwroldControl.textWidth;
                textChanger.Set();
                textChanger.Change();

            }


        }

        CultureInfo.CurrentCulture = CultureInfo.CreateSpecificCulture(ScreenMaxToOneSon(OverwroldControl.menuAndSettingSave, "CultureInfo"));
    }
    // Start is called before the first frame update
    private void Awake()
    {
        instance = this;
        Initialization(-1);
        //DOTween.Init(null, false);
    }
    void Start()
    {
        if (haveInOutBlack)
        {
            inOutBlack = GameObject.Find("Canvas/InOutBlack").GetComponent<Image>();
            inOutBlack.color = Color.black;
            OverwroldControl.pause = !notPauseIn;
            if (!noInBlack)
                inOutBlack.DOColor(Color.clear, 0.5f).SetEase(Ease.Linear).OnKill(EndBlack);
            else inOutBlack.color = Color.clear;
        }
       
        AudioListener.volume = OverwroldControl.mainVolume;
        OverwroldControl.isSetting = false;

        SetResolution(OverwroldControl.resolutionLevel);
        FindAndChangeAllSFX(OverwroldControl.noSFX);
        if (!OverwroldControl.isDebug && PlayerControl.playerName.Length > 0 && PlayerControl.playerName[0] == '<')
        {
            PlayerControl.playerName = "Chara";
        }
    }

    /// <summary>
    /// 给debug的好东西(迫真
    /// </summary>
    string RandomTextColor()
    {
        string text = "<color=#";
        for (int i = 0; i < 6; i++)
        {
            text += string.Format("{0:X}", UnityEngine.Random.Range(0, 16));
        }
        text += "FF>";
        return text;
    }

    void EndBlack()
    {
        OverwroldControl.pause = false;
    }
    private void Update()
    {
        if (OverwroldControl.isDebug)
        {
            PlayerControl.playerName = RandomTextColor() + 'd' + RandomTextColor() + 'e' + RandomTextColor() + 'b' + RandomTextColor() + 'u' + RandomTextColor() + 'g' + "</color></color></color></color></color>";
        }
        if (PlayerControl.hpMax < PlayerControl.hp)
            PlayerControl.hp = PlayerControl.hpMax;
       
        if (OverwroldControl.isSetting)
            return;
        if (KeyArrowToControl(KeyCode.Tab))
        {
            ChangeResolution();
        }
        if (KeyArrowToControl(KeyCode.Semicolon))
        {
            OverwroldControl.noSFX = !OverwroldControl.noSFX;
            FindAndChangeAllSFX(OverwroldControl.noSFX);
        }
        if (KeyArrowToControl(KeyCode.F4))
        {
            OverwroldControl.fullScreen = !OverwroldControl.fullScreen;
            SetResolution(OverwroldControl.resolutionLevel);
        }
    }
    /// <summary>
    /// 应用默认键位
    /// </summary>
    public void ApplyDefaultControl()
    {
        OverwroldControl.keyCodes = new List<KeyCode>
        {
            KeyCode.DownArrow,
            KeyCode.RightArrow,
            KeyCode.UpArrow,
            KeyCode.LeftArrow,
            KeyCode.Z,
            KeyCode.X,
            KeyCode.C,
            KeyCode.V,
            KeyCode.F4,
            KeyCode.None,
            KeyCode.None,
            KeyCode.Escape
        };

        OverwroldControl.keyCodesBack1 = new List<KeyCode>
        {
            KeyCode.S,
            KeyCode.D,
            KeyCode.W,
            KeyCode.A,
            KeyCode.Return,
            KeyCode.RightShift,
            KeyCode.RightControl,
            KeyCode.None,
            KeyCode.None,
            KeyCode.None,
            KeyCode.None,
            KeyCode.None
        }; 
        OverwroldControl.keyCodesBack2 = new List<KeyCode>
        {
            KeyCode.None,
            KeyCode.None,
            KeyCode.None,
            KeyCode.None,
            KeyCode.None,
            KeyCode.LeftShift,
            KeyCode.LeftControl,
            KeyCode.None,
            KeyCode.None,
            KeyCode.None,
            KeyCode.None,
            KeyCode.None
        };
    }

    /// <summary>
    /// 传入默认KeyCode并转换为游戏内键位。
    /// mode:0按下 1持续 2抬起
    /// </summary>
    public bool KeyArrowToControl(KeyCode key,int mode = 0)
    {
        switch (mode)
        {
            case 0:
                switch (key)
                {
                    case KeyCode.DownArrow:
                        return Input.GetKeyDown(OverwroldControl.keyCodes[0]) || Input.GetKeyDown(OverwroldControl.keyCodesBack1[0]) || Input.GetKeyDown(OverwroldControl.keyCodesBack2[0]);
                    case KeyCode.RightArrow:
                        return Input.GetKeyDown(OverwroldControl.keyCodes[1]) || Input.GetKeyDown(OverwroldControl.keyCodesBack1[1]) || Input.GetKeyDown(OverwroldControl.keyCodesBack2[1]);
                    case KeyCode.UpArrow:
                        return Input.GetKeyDown(OverwroldControl.keyCodes[2]) || Input.GetKeyDown(OverwroldControl.keyCodesBack1[2]) || Input.GetKeyDown(OverwroldControl.keyCodesBack2[2]);
                    case KeyCode.LeftArrow:
                        return Input.GetKeyDown(OverwroldControl.keyCodes[3]) || Input.GetKeyDown(OverwroldControl.keyCodesBack1[3]) || Input.GetKeyDown(OverwroldControl.keyCodesBack2[3]);
                    case KeyCode.Z:
                        return Input.GetKeyDown(OverwroldControl.keyCodes[4]) || Input.GetKeyDown(OverwroldControl.keyCodesBack1[4]) || Input.GetKeyDown(OverwroldControl.keyCodesBack2[4]);
                    case KeyCode.X:
                        return Input.GetKeyDown(OverwroldControl.keyCodes[5]) || Input.GetKeyDown(OverwroldControl.keyCodesBack1[5]) || Input.GetKeyDown(OverwroldControl.keyCodesBack2[5]);
                    case KeyCode.C:
                        return Input.GetKeyDown(OverwroldControl.keyCodes[6]) || Input.GetKeyDown(OverwroldControl.keyCodesBack1[6]) || Input.GetKeyDown(OverwroldControl.keyCodesBack2[6]);
                    case KeyCode.V:
                        return Input.GetKeyDown(OverwroldControl.keyCodes[7]) || Input.GetKeyDown(OverwroldControl.keyCodesBack1[7]) || Input.GetKeyDown(OverwroldControl.keyCodesBack2[7]);
                    case KeyCode.F4:
                        return Input.GetKeyDown(OverwroldControl.keyCodes[8]) || Input.GetKeyDown(OverwroldControl.keyCodesBack1[8]) || Input.GetKeyDown(OverwroldControl.keyCodesBack2[8]);
                    case KeyCode.Tab:
                        return Input.GetKeyDown(OverwroldControl.keyCodes[9]) || Input.GetKeyDown(OverwroldControl.keyCodesBack1[9]) || Input.GetKeyDown(OverwroldControl.keyCodesBack2[9]);
                    case KeyCode.Semicolon:
                        return Input.GetKeyDown(OverwroldControl.keyCodes[10]) || Input.GetKeyDown(OverwroldControl.keyCodesBack1[10]) || Input.GetKeyDown(OverwroldControl.keyCodesBack2[10]);
                    case KeyCode.Escape:
                        return Input.GetKeyDown(OverwroldControl.keyCodes[11]) || Input.GetKeyDown(OverwroldControl.keyCodesBack1[11]) || Input.GetKeyDown(OverwroldControl.keyCodesBack2[11]);

                    default:
                        return false;
                }
            case 1:
                switch (key)
                {
                    case KeyCode.DownArrow:
                        return Input.GetKey(OverwroldControl.keyCodes[0]) || Input.GetKey(OverwroldControl.keyCodesBack1[0]) || Input.GetKey(OverwroldControl.keyCodesBack2[0]);
                    case KeyCode.RightArrow:
                        return Input.GetKey(OverwroldControl.keyCodes[1]) || Input.GetKey(OverwroldControl.keyCodesBack1[1]) || Input.GetKey(OverwroldControl.keyCodesBack2[1]);
                    case KeyCode.UpArrow:
                        return Input.GetKey(OverwroldControl.keyCodes[2]) || Input.GetKey(OverwroldControl.keyCodesBack1[2]) || Input.GetKey(OverwroldControl.keyCodesBack2[2]);
                    case KeyCode.LeftArrow:
                        return Input.GetKey(OverwroldControl.keyCodes[3]) || Input.GetKey(OverwroldControl.keyCodesBack1[3]) || Input.GetKey(OverwroldControl.keyCodesBack2[3]);
                    case KeyCode.Z:
                        return Input.GetKey(OverwroldControl.keyCodes[4]) || Input.GetKey(OverwroldControl.keyCodesBack1[4]) || Input.GetKey(OverwroldControl.keyCodesBack2[4]);
                    case KeyCode.X:
                        return Input.GetKey(OverwroldControl.keyCodes[5]) || Input.GetKey(OverwroldControl.keyCodesBack1[5]) || Input.GetKey(OverwroldControl.keyCodesBack2[5]);
                    case KeyCode.C:
                        return Input.GetKey(OverwroldControl.keyCodes[6]) || Input.GetKey(OverwroldControl.keyCodesBack1[6]) || Input.GetKey(OverwroldControl.keyCodesBack2[6]);
                    case KeyCode.V:
                        return Input.GetKey(OverwroldControl.keyCodes[7]) || Input.GetKey(OverwroldControl.keyCodesBack1[7]) || Input.GetKey(OverwroldControl.keyCodesBack2[7]);
                    case KeyCode.F4:
                        return Input.GetKey(OverwroldControl.keyCodes[8]) || Input.GetKey(OverwroldControl.keyCodesBack1[8]) || Input.GetKey(OverwroldControl.keyCodesBack2[8]);
                    case KeyCode.Tab:
                        return Input.GetKey(OverwroldControl.keyCodes[9]) || Input.GetKey(OverwroldControl.keyCodesBack1[9]) || Input.GetKey(OverwroldControl.keyCodesBack2[9]);
                    case KeyCode.Semicolon:
                        return Input.GetKey(OverwroldControl.keyCodes[10]) || Input.GetKey(OverwroldControl.keyCodesBack1[10]) || Input.GetKey(OverwroldControl.keyCodesBack2[10]);
                    case KeyCode.Escape:
                        return Input.GetKey(OverwroldControl.keyCodes[11]) || Input.GetKey(OverwroldControl.keyCodesBack1[11]) || Input.GetKey(OverwroldControl.keyCodesBack2[11]);

                    default:
                        return false;
                }
            case 2:
                switch (key)
                {
                    case KeyCode.DownArrow:
                        return Input.GetKeyUp(OverwroldControl.keyCodes[0]) || Input.GetKeyUp(OverwroldControl.keyCodesBack1[0]) || Input.GetKeyUp(OverwroldControl.keyCodesBack2[0]);
                    case KeyCode.RightArrow:
                        return Input.GetKeyUp(OverwroldControl.keyCodes[1]) || Input.GetKeyUp(OverwroldControl.keyCodesBack1[1]) || Input.GetKeyUp(OverwroldControl.keyCodesBack2[1]);
                    case KeyCode.UpArrow:
                        return Input.GetKeyUp(OverwroldControl.keyCodes[2]) || Input.GetKeyUp(OverwroldControl.keyCodesBack1[2]) || Input.GetKeyUp(OverwroldControl.keyCodesBack2[2]);
                    case KeyCode.LeftArrow:
                        return Input.GetKeyUp(OverwroldControl.keyCodes[3]) || Input.GetKeyUp(OverwroldControl.keyCodesBack1[3]) || Input.GetKeyUp(OverwroldControl.keyCodesBack2[3]);
                    case KeyCode.Z:
                        return Input.GetKeyUp(OverwroldControl.keyCodes[4]) || Input.GetKeyUp(OverwroldControl.keyCodesBack1[4]) || Input.GetKeyUp(OverwroldControl.keyCodesBack2[4]);
                    case KeyCode.X:
                        return Input.GetKeyUp(OverwroldControl.keyCodes[5]) || Input.GetKeyUp(OverwroldControl.keyCodesBack1[5]) || Input.GetKeyUp(OverwroldControl.keyCodesBack2[5]);
                    case KeyCode.C:
                        return Input.GetKeyUp(OverwroldControl.keyCodes[6]) || Input.GetKeyUp(OverwroldControl.keyCodesBack1[6]) || Input.GetKeyUp(OverwroldControl.keyCodesBack2[6]);
                    case KeyCode.V:
                        return Input.GetKeyUp(OverwroldControl.keyCodes[7]) || Input.GetKeyUp(OverwroldControl.keyCodesBack1[7]) || Input.GetKeyUp(OverwroldControl.keyCodesBack2[7]);
                    case KeyCode.F4:
                        return Input.GetKeyUp(OverwroldControl.keyCodes[8]) || Input.GetKeyUp(OverwroldControl.keyCodesBack1[8]) || Input.GetKeyUp(OverwroldControl.keyCodesBack2[8]);
                    case KeyCode.Tab:
                        return Input.GetKeyUp(OverwroldControl.keyCodes[9]) || Input.GetKeyUp(OverwroldControl.keyCodesBack1[9]) || Input.GetKeyUp(OverwroldControl.keyCodesBack2[9]);
                    case KeyCode.Semicolon:
                        return Input.GetKeyUp(OverwroldControl.keyCodes[10]) || Input.GetKeyUp(OverwroldControl.keyCodesBack1[10]) || Input.GetKeyUp(OverwroldControl.keyCodesBack2[10]);
                    case KeyCode.Escape:
                        return Input.GetKeyUp(OverwroldControl.keyCodes[11]) || Input.GetKeyUp(OverwroldControl.keyCodesBack1[11]) || Input.GetKeyUp(OverwroldControl.keyCodesBack2[11]);

                    default:
                        return false;
                }
            default:
                return false;
        }
        
    }
    /// <summary>
    /// 开/关 SFX
    /// </summary>
    public void FindAndChangeAllSFX(bool isClose)
    {
        foreach (Light2D light in Resources.FindObjectsOfTypeAll(typeof(Light2D)))
        {
            light.enabled = !isClose;
        }
        /*
        foreach (Volume v in Resources.FindObjectsOfTypeAll(typeof(Volume)))
        {
            v.gameObject.SetActive(!isClose);
        }
        */
        Camera.main.GetUniversalAdditionalCameraData().renderPostProcessing = !isClose;
    }
    /// <summary>
    /// 按按tab改改分辨率那样子))
    /// </summary>
    public void ChangeResolution()
    {
        if (OverwroldControl.resolutionLevel < 4)
            OverwroldControl.resolutionLevel += 1;
        else OverwroldControl.resolutionLevel = 0;
        SetResolution(OverwroldControl.resolutionLevel);

    }
    /// <summary>
    /// 和分辨率设置配套的换算
    /// </summary>
    int ScreenSet(int y)
    {
        if (OverwroldControl.backGround)
            y = y / 9 * 16;
        else y = y / 3 * 4;
        return y;
    }
    /// <summary>
    /// 分辨率设置
    /// </summary>
    public void SetResolution(int resolution)
    {
        switch (resolution)
        {
            case 0:
                Screen.SetResolution(ScreenSet(480), 480, OverwroldControl.fullScreen);
                OverwroldControl.resolution = new Vector2(ScreenSet(480), 480);
                break;
            /* 具有显示错误 固删去
            case 1:
                Screen.SetResolution(ScreenSet(600), 600, OverwroldControl.fullScreen);
                OverwroldControl.resolution = new Vector2(ScreenSet(600), 600);
                break;
            */
            case 1:
                Screen.SetResolution(ScreenSet(768), 768, OverwroldControl.fullScreen);
                OverwroldControl.resolution = new Vector2(ScreenSet(768), 768);
                break;
            case 2:
                Screen.SetResolution(ScreenSet(864), 864, OverwroldControl.fullScreen);
                OverwroldControl.resolution = new Vector2(ScreenSet(864), 864);
                break;
            case 3:
                Screen.SetResolution(ScreenSet(960), 960, OverwroldControl.fullScreen);
                OverwroldControl.resolution = new Vector2(ScreenSet(960), 960);
                break;
            case 4:
                Screen.SetResolution(ScreenSet(1080), 1080, OverwroldControl.fullScreen);
                OverwroldControl.resolution = new Vector2(ScreenSet(1080), 1080);
                break;
            default:
                break;
        }
    }
    /// <summary>
    /// 淡出 输入跳转场景名称
    /// banMusic是渐出
    /// </summary>
    public void OutBlack(string scene, bool banMusic = false, float time = 0.5f,bool Async = true)
    {
        if (banMusic)
        {
            AudioSource bgm = GameObject.Find("BGM Source").transform.GetComponent<AudioSource>();
            if (time > 0)
                DOTween.To(() => bgm.volume, x => bgm.volume = x, 0, time).SetEase(Ease.Linear);
            else bgm.volume = 0;
        }
        OverwroldControl.pause = true;
        if (time > 0)
            inOutBlack.DOColor(Color.black, time).SetEase(Ease.Linear).OnKill(() => SwitchScene(scene));
        else
        {
            inOutBlack.color = Color.black;
            SwitchScene(scene, Async);
        }
    }
    public void OutWhite(string scene)
    {
        inOutBlack.color = new Color(1, 1, 1, 0);
        AudioController.instance.GetFx(6, AudioControl.fxClipUI);
        inOutBlack.DOColor(Color.white, 5.5f).SetEase(Ease.Linear).OnKill(() => SwitchScene(scene));
    }
    public void SwitchScene(string name, bool Async = true)
    {
        if (Async)
            SceneManager.LoadSceneAsync(name);
        else SceneManager.LoadScene(name);
    }
    /// <summary>
    /// 传入string，返回删去末尾i个字符的string
    /// </summary>
    public string SubText(string str, int i = 1)
    {
        str = str.Substring(0, str.Length - i);
        return str;
    }
    /// <summary>
    /// 随机生成一个六位长的英文)
    /// </summary>
    public string RandomName(int l = 6, string abc = "qwertyuiopasdfghjklzxcvbnmQWERTYUIOPASDFGHJKLZXCVBNM")
    {
        string text = "";
      
        for (int i = 0; i < l; i++)
        {
            text += abc[UnityEngine.Random.Range(0, abc.Length)];
        }
        return text;

    }
    /// <summary>
    /// 因为回合数据是在每回合前调入的，所以搁这
    /// 调入回合数据
    /// </summary>
    public void LoadRound(int round)
    {
        LoadItemData(BattleControl.roundSave, BattleControl.roundAsset[round]);
       
        int sp = BattleControl.roundSave.Count / 2;
        //以下部分是自动排序的相关代码

        for (int i = 0; i < BattleControl.roundSave.Count; i++)
        {
            string back = BattleControl.roundSave[i];
            float temp = MaxToLastFloat(back);
            int j = i;
            for (j = i; j > 0; j--)
            {
                if (temp < MaxToLastFloat(BattleControl.roundSave[j - 1]))
                {

                    BattleControl.roundSave[j] = BattleControl.roundSave[j - 1];   //更小的向前移动

                }
                else
                {
                    BattleControl.roundSave[j] = back;//如果插入的值大于已有的值，直接在当前位置插入，并不再与前面的数字进行比较
                    break;
                }
            }

        }

        
    }
    /// <summary>
    /// 调入数据
    /// </summary>
    public void LoadItemData(List<string> list,TextAsset texter)//保存的list 导入的text
    {
        List<int> EnterI = new List<int>();
        list.Clear();
        string text = "";
        for (int i = 0; i < texter.text.Length; i++)
        {
            
            if (texter.text[i] == '/' && texter.text[i + 1] == '*')
            {
                i++;
                while (!(texter.text[i] == '/' && texter.text[i - 1] == '*'))
                {
                    i++;
                }
                i += 2;
            }

            if (texter.text[i] != '\n' && texter.text[i] != '\r' && texter.text[i] != ';')
                text += texter.text[i];
            if (texter.text[i] == ';')
            {
                list.Add(text + ";");
                text = "";
            }
            
        }

    }
    /// <summary>
    /// 咱说就是不拿TextAsset拿string也行昂)
    /// </summary>
    public void LoadItemData(List<string> list, string texter)//保存的list 导入的text
    {
        List<int> EnterI = new List<int>();
        list.Clear();
        string text = "";
        for (int i = 0; i < texter.Length; i++)
        {

            if (texter[i] == '/' && texter[i + 1] == '*')
            {
                i++;
                while (!(texter[i] == '/' && texter[i - 1] == '*'))
                {
                    i++;
                }
                i += 2;
            }

            if (texter[i] != '\n' && texter[i] != '\r' && texter[i] != ';')
                text += texter[i];
            if (texter[i] == ';')
            {
                list.Add(text + ";");
                text = "";
            }

        }

    }
    /// <summary>
    /// 传入使用背包的哪个物体
    /// 然后就使用 打true会顺带把背包顺序整理下
    /// 然后再让打字机打个字
    /// plusText填0就自己计算
    /// </summary>
    public void UseItem(TypeWritter typeWritter, int sonSelent, int plusText = 0)
    {
        if (plusText == 0)
        {
            if (PlayerControl.myItems[sonSelent - 1] >= 20000)
                plusText = -20000 + ItemControl.itemFoods.Count / 3 + ItemControl.itemArms.Count / 2;
            else if (PlayerControl.myItems[sonSelent - 1] >= 10000)
                plusText = -10000 + ItemControl.itemFoods.Count / 3;
            else plusText = 0;
        }

        if (PlayerControl.myItems[sonSelent - 1] >= 20000)
        {
            typeWritter.TypeOpen(ItemControl.itemTextMaxItemSon[(PlayerControl.myItems[sonSelent - 1] + plusText) * 5 - 3], false, 0, 0);
            PlayerControl.wearDef = int.Parse(ItemIdGet(PlayerControl.myItems[sonSelent - 1], "Auto", 1));
            int wearInt = PlayerControl.wearArmor;
            PlayerControl.wearArmor = PlayerControl.myItems[sonSelent - 1];
            PlayerControl.myItems[sonSelent - 1] = wearInt;

            AudioController.instance.GetFx(3, AudioControl.fxClipUI);

        }
        else if (PlayerControl.myItems[sonSelent - 1] >= 10000)
        {
            typeWritter.TypeOpen(ItemControl.itemTextMaxItemSon[(PlayerControl.myItems[sonSelent - 1] + plusText) * 5 - 3], false, 0, 0);
            PlayerControl.wearAtk = int.Parse(ItemIdGet(PlayerControl.myItems[sonSelent - 1], "Auto", 1));
            int wearInt = PlayerControl.wearArm;
            PlayerControl.wearArm = PlayerControl.myItems[sonSelent - 1];
            PlayerControl.myItems[sonSelent - 1] = wearInt;

            AudioController.instance.GetFx(3, AudioControl.fxClipUI);

        }
        else//食物
        {
            typeWritter.TypeOpen(ItemControl.itemTextMaxItemSon[PlayerControl.myItems[sonSelent - 1] * 5 - 3], false,
                int.Parse(ItemIdGet(PlayerControl.myItems[sonSelent - 1], "Auto", 2)), 0);
            PlayerControl.hp += int.Parse(ItemIdGet(PlayerControl.myItems[sonSelent - 1], "Auto", 2));
            if (PlayerControl.hp > PlayerControl.hpMax)
                PlayerControl.hp = PlayerControl.hpMax;
            for (int i = 0; i < ItemControl.itemFoods.Count; i++)
            {
                if (ItemControl.itemTextMaxItemSon[PlayerControl.myItems[sonSelent - 1] * 5 - 5] == ItemControl.itemFoods[i])
                {
                    string text = ItemControl.itemFoods[i + 1];
                    text = text.Substring(1, text.Length - 1);
                    PlayerControl.myItems[sonSelent - 1] = ItemNameGetId(text, "Foods");
                    break;
                }

            }
            AudioController.instance.GetFx(2, AudioControl.fxClipUI);

        }

    }

    /// <summary>
    /// 转换特殊字符
    /// </summary>
    public List<string> ChangeItemData(List<string> list, bool isData, List<string> ex)
    {
        List<string> newList = new List<string>();
        string text = "";
        bool isXH = false;//检测是否有多个需要循环调用的特殊字符

        for (int i = 0; i < list.Count; i++)
        {
            string name = "";
            for (int j = 0; j < list[i].Length; j++)
            {
                if (name == "" && !isData)
                {
                    int k = j;
                    while (list[i][j] != '\\')
                    {
                        name += list[i][j];
                        j++;
                        if (j >= list[i].Length)
                            break;
                        
                    }
                    j = k;
                    //Debug.Log(list[i] +"/"+ name);
                }

                while (list[i][j] == '<')
                {
                    string texters = "";
                    while ((list[i][j - 1] != '>' && !isXH) || isXH)
                    {
                        texters += list[i][j];
                        j++;
                        if (j >= list[i].Length)
                        {
                            break;
                        }
                        isXH = false;
                    }
                    isXH = true;
                    text = ChangeItemDataSwitch(text, texters, isData, name, ex);
                }
                isXH = false;

                if (list[i][j] == ';')
                {
                    newList.Add(text + ";");
                    text = "";
                }
                else
                {
                    text += list[i][j];
                }

            }
        }
        return newList;
    }


    /// <summary>
    /// ChangeItemData中检测'<''>'符号的Switch语句
    /// </summary>
    string ChangeItemDataSwitch(string text, string texters, bool isData, string name, List<string> ex)
    {
        //Debug.Log(text + "/" + texters + "/" + isData);
        switch (texters)
        {
            case "</playerName>":
                if (!OverwroldControl.isDebug)
                    text += PlayerControl.playerName;
                else
                    text += "debug";
                break;



            case "</enter>"://回车
                text += "\n";
                break;

            case "</stop>"://打字机停顿
                text += "龘";
                break;

            case "</autoFoodFull>":
                text += ItemControl.itemTextMaxData[11].Substring(0, ItemControl.itemTextMaxData[11].Length - 1) + "\n";
                goto case "</autoFood>";

            case "</autoFood>":
                text += "齉";
                break;

            case "</passText>":
                text += "轂";
                break;

            /*
            此部分留给打字机实现，因为hp是变量。

            case "</autoFood>":
                int hp = int.Parse(ItemIdGet(id + 1, "Foods", 2));
                if (hp + PlayerControl.hp >= PlayerControl.hpMax)
                    texters = "</data22>";
                else
                    texters = "</data12>";
                goto default;
            */

            case "</autoCheckFood>":
                texters = "</data13>";
                goto default;

            case "</autoArm>":
                texters = "</data14>";
                goto default;
            case "</autoArmor>":
                texters = "</data15>";
                goto default;

            case "</autoCheckArm>":
                texters = "</data16>";
                goto default;

            case "</autoCheckArmor>":
                texters = "</data17>";
                goto default;

            case "</autoLoseFood>":
                texters = "</data18>";
                goto default;
            case "</autoLoseArm>":
                texters = "</data19>";
                goto default;
            case "</autoLoseArmor>":
                texters = "</data20>";
                goto default;
            case "</autoLoseOther>":
                texters = "</data21>";
                goto default;

            case "</itemHp>":
                if (name != "" && !isData)
                {
                    text += ItemIdGet(ItemNameGetId(name, "Foods"), "Auto", 2);
                    break;
                }
                else goto default;
                
            case "</itemAtk>":
                if (name != "" && !isData)
                {
                    text += ItemIdGet(ItemNameGetId(name, "Arms"), "Auto", 1);
                    break;
                }
                else goto default;

            case "</itemDef>":
                if (name != "" && !isData)
                {
                    text += ItemIdGet(ItemNameGetId(name, "Armors"), "Auto", 1);
                    break;
                }
                else goto default;

            case "</getEnemiesName>":
                if (name != "" && !isData)
                {
                    text += ex[0];
                    break;
                }
                else goto default;
            case "</getEnemiesATK>":
                if (name != "" && !isData)
                {
                    text += ex[1];
                    break;
                }
                else goto default;
            case "</getEnemiesDEF>":
                if (name != "" && !isData)
                {
                    text += ex[2];
                    break;
                }
                else goto default;

            default:
                if (texters.Length == 3 && texters[0] == '<' && texters[2] == '>')
                {
                    text += "粜" + texters[1].ToString();
                    break;
                }
                if (texters.Length > 6 && texters.Substring(0, 6) == "<Image")
                {
                    texters = texters.Substring(7);
                    texters = texters.Substring(0, texters.Length - 1);
                    int q = int.Parse(texters);
                    text += "鼵" + q + "鼵";
                    break;
                }
                if (texters.Length > 3 && texters.Substring(0, 3) == "<FX")
                {
                    texters = texters.Substring(4);
                    texters = texters.Substring(0, texters.Length - 1);
                    int q = int.Parse(texters);
                    text += "菔" + q + "菔";
                    break;
                }
                if (texters.Length > 6 && texters.Substring(0, 6) == "</data")
                {
                    text += ItemControl.itemTextMaxData[int.Parse(texters.Substring(6, texters.Length - 7))].Substring(0, ItemControl.itemTextMaxData[int.Parse(texters.Substring(6, texters.Length - 7))].Length - 1);
                }
                else if (texters.Length > 10)
                {
                    if (texters.Substring(0, 10) == "</itemName" && !isData)
                    {
                        if (name != "")
                        {
                            if (ItemNameGetId(name, texters.Substring(10, texters.Length - 11) + 's') < 10000)
                                text += ItemIdGet(ItemNameGetId(name, texters.Substring(10, texters.Length - 11) + 's'), texters.Substring(10, texters.Length - 11) + 's', 0);
                            else text += ItemIdGet(ItemNameGetId(name, texters.Substring(10, texters.Length - 11) + 's'), "Auto", 0);
                        }

                    }
                    else if (texters.Substring(0, 10) == "</autoLose")
                    {
                        switch (texters.Substring(10, texters.Length - 11) + 's')
                        {
                            case "Food":
                                text += ItemControl.itemTextMaxData[18];
                                break;

                            case "Arm":
                                text += ItemControl.itemTextMaxData[19];
                                break;

                            case "Armor":
                                text += ItemControl.itemTextMaxData[20];
                                break;

                            case "Other":
                                text += ItemControl.itemTextMaxData[21];
                                break;
                        }
                    }
                    else
                        text += texters;
                }
                else
                {
                    text += texters;
                }
                break;
        }
        return text;
    }
    /// <summary>
    /// 检测输入文本内的大写字母，转为全小写。
    /// </summary>
    public string UppercaseToLowercase(string origin)
    {
        string final = "";
        string bet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        string betS = "abcdefghijklmnopqrstuvwxyz";
        for (int i = 0; i < origin.Length; i++)
        {
            bool isPlus = false;
            for (int j = 0; j < bet.Length; j++)
            {
                if (origin[i] == bet[j])
                {
                    final += betS[j];
                    break;
                }
                else if (j == bet.Length - 1)
                    isPlus = true;
                   
            }
            if (isPlus)
                final += origin[i];
        }
        return final;
    }
    /// <summary>
    /// 输入形如(x,y)的向量
    /// 若向量形如(xRx，yRy)或(xrx，yry)，则在R左右取随机数
    /// </summary>
    public Vector2 StringVector2ToRealVector2(string stringVector2,Vector3 origin)
    {
        stringVector2 = stringVector2.Substring(1, stringVector2.Length - 2) + ",";
        Vector2 realVector2 = Vector2.zero;
        string save = "";
        bool isSetX = false;
        for (int i = 0; i < stringVector2.Length; i++)
        {
            if (stringVector2[i] == ',')
            {
                if (!isSetX)
                {
                    realVector2.x = RandomFloatChange(save, origin.x);
                    isSetX = true;
                    save = "";
                }
                else
                {
                    realVector2.y = RandomFloatChange(save, origin.y);
                    break;
                }

            }
            else 
                save += stringVector2[i];
        }
        return realVector2;
    }
    /// <summary>
    /// 形如xRx？xrx？O？来我来帮你随机分开吧嘿嘿嘿(((((
    /// 如果没有r或R的话就会返回原本的，非常的实用
    /// </summary>
    float RandomFloatChange(string text,float origin)
    {
        bool isHaveR = false;
        string save = "";
        float x1 = 0, x2 = 0;
        if (text != "O" && text != "o")
        {

            for (int i = 0; i < text.Length; i++)
            {
                if ((text[i] == 'r' || text[i] == 'R') && !isHaveR)
                {
                    x1 = float.Parse(save);
                    save = "";
                    isHaveR = true;
                }
                else save += text[i];
            }
            if (isHaveR)
            {
                x2 = float.Parse(save);
                return UnityEngine.Random.Range(x1, x2);
            }
            else
            {
                return float.Parse(text);

            }
        }
        else 
            return origin;
    }

    /*之后回来翻才意识到这不就一个强制转换的事儿）
     * 
    /// <summary>
    /// 输入形如(x,y)的向量
    /// 
    /// </summary>
    public Vector3 StringVector2ToRealVector3(string stringVector2)
    {
        Vector2 endVector2 = StringVector2ToRealVector2(stringVector2);
        return new Vector3(endVector2.x, endVector2.y);
    }
    */

    /// <summary>
    /// 输入形如(r,g,b,a)的向量
    /// 同样支持随机数
    /// </summary>
    public Color StringVector4ToRealColor(string stringVector4, Color origin)
    {
        stringVector4 = stringVector4.Substring(1, stringVector4.Length - 2) + ",";
        Color realVector4 = Color.white;
        string save = "";
        int isSet = 0;
        for (int i = 0; i < stringVector4.Length; i++)
        {
            if (stringVector4[i] == ',')
            {
                switch (isSet)
                {
                    case 0:
                        realVector4.r = RandomFloatChange(save, origin.r);
                        goto default;

                    case 1:
                        realVector4.g = RandomFloatChange(save, origin.g);
                        goto default;

                    case 2:
                        realVector4.b = RandomFloatChange(save, origin.b);
                        goto default;

                    case 3:
                        realVector4.a = RandomFloatChange(save, origin.a);
                        break;

                    default:
                        isSet++;
                        save = "";
                        break;
                }

            }
            else 
                save += stringVector4[i];
        }
        return realVector4;
    }
    /// <summary>
    /// 在num1与num2之间判断 符合后返回num2.否则传回num1.
    /// </summary>
    public float JudgmentNumber(bool isGreater, float num1, float num2)
    {
        if (isGreater)
        {
            if (num1 > num2)
                num1 = num2;
        }
        else
        {
            if (num1 < num2)
                num1 = num2;
        }
        return num1;
    }

    /// <summary>
    /// 分配Item数据
    /// </summary>
    void ItemClassificatio()
    {
        ItemControl.itemFoods.Clear();
        ItemControl.itemArms.Clear();
        ItemControl.itemArmors.Clear();
        ItemControl.itemOthers.Clear();
        for (int i = 0; i < ItemControl.itemMax.Count; i++)//总物品数
        {
            string countText = ItemControl.itemMax[i];
            string[] text = new string[4];
            int texti = 0;
            for (int k = 0; k < countText.Length; k++)//单物品遍历 寻找\符
            {
                if (countText[k] == '\\')
                    texti++;
                else if (countText[k] != ';')
                    text[texti] += countText[k].ToString();

                if (texti == 3 && countText[k] == ';')
                {
                    for (int j = 0; j < text.Length; j++)
                    {
                        if (j != 1)
                            ItemClassificatioAdd(text[1], text[j]);
                    }
                    texti = 0;
                }
            }
        }
    }
    /// <summary>
    /// ItemClassificatio的一个子void
    /// </summary>
    void ItemClassificatioAdd(string i, string origin)
    {
        if (origin != "null")
            switch (i)
            {
                case "Foods":
                    ItemControl.itemFoods.Add(origin);
                    break;
                case "Arms":
                    ItemControl.itemArms.Add(origin);
                    break;
                case "Armors":
                    ItemControl.itemArmors.Add(origin);
                    break;
                case "Others":
                    ItemControl.itemOthers.Add(origin);
                    break;
                default:
                    ItemControl.itemOthers.Add(origin);
                    break;
            }
    }
    /// <summary>
    /// 检测 '\'字符然后分割文本到子List
    /// 批量处理string
    /// </summary>
    public void MaxToOneSon(List<string> parentList, List<string> sonList, char font = '\\')
    {
        sonList.Clear();
        string text = "";
        for (int i = 0; i < parentList.Count; i++)
        {
            for (int j = 0; j < parentList[i].Length; j++)
            {
                if (parentList[i][j] == font || parentList[i][j] == ';')
                {
                    sonList.Add(text);
                    text = "";
                }
                else text += parentList[i][j];
            }
        }
    }

    /// <summary>
    /// 检测 '\'字符然后分割文本到子List
    /// 传入一个string
    /// </summary>
    public void MaxToOneSon(string parentString, List<string> sonList, char font = '\\')
    {
        sonList.Clear();
        string text = "";

        for (int j = 0; j < parentString.Length; j++)
        {
            if (parentString[j] == font || parentString[j] == ';')
            {
                sonList.Add(text);
                text = "";
            }
            else text += parentString[j];
        }

    }
    /// <summary>
    /// 检测到第一个'\'字符就传出
    /// </summary>
    public string MaxToOneSon(string original, char font = '\\')
    {
        string final = "";
        for (int i = 0; i < original.Length; i++)
        {
            if (original[i] != '\\')
                final += original[i];
            else break;
        }
        return final;
    }
    /// <summary>
    /// 反向检测第一个'\'字符就传出，可选忽视掉最后的 ; 号。
    /// </summary>
    public float MaxToLastFloat(string original, bool noLast = true, char font = '\\')
    {
        if (noLast && original.Substring(original.Length - 1) == ";")
            original = original.Substring(0, original.Length - 1);
        string changed = "";
        for (int i = 0; i < original.Length; i++)
        {
            changed += original[original.Length - i - 1];
        }
        changed = MaxToOneSon(changed, font);
        original = "";
        for (int i = 0; i < changed.Length; i++)
        {
            original += changed[changed.Length - i - 1];
        }
        if (float.TryParse(original, out float y))
            return y;
        else
            return 99999999;
    }
    /// <summary>
    /// 用于游戏内文本读取
    /// 传入数据名称返回文本包文本
    /// 给第一个 返第二个)
    /// </summary>
    public string ScreenMaxToOneSon(List<string> parentList, string screen)
    {
        for (int i = 0; i < parentList.Count; i++)
        {
            if (parentList[i].Length > screen.Length && MaxToOneSon(parentList[i]) == screen)
            {
                return SubText(parentList[i].Substring(screen.Length + 1));
            }
        }

        return "null";
    }
    
    /// <summary>
    /// 用于游戏内文本读取
    /// 传入数据名称返回所有同名的文本包文本
    /// </summary>
    public List<string> ScreenMaxToAllSon(List<string> parentList, string screen)
    {
        List<string> list = new List<string>();
        for (int i = 0; i < parentList.Count; i++) 
        {
            if (parentList[i].Length > screen.Length && MaxToOneSon(parentList[i]) == screen)
            {
                list.Add(SubText(parentList[i].Substring(screen.Length + 1)));
            }
        }

        return list;
    }
    /// <summary>
    /// 检测list的前几个字符是否与传入的string screen相同。
    /// 若相同则分割文本到子List
    /// </summary>
    public void ScreenMaxToOneSon(List<string> parentList, List<string> sonList,string screen)
    {
        sonList.Clear();
        for (int i = 0; i < parentList.Count; i++)
        {
            if (parentList[i].Substring(0, screen.Length) == screen)
            {
                sonList.Add(parentList[i].Substring(screen.Length));
            }
        }
    }
    /// <summary>
    /// 再分配文本包
    /// </summary>
    void MaxToSon(List<string> max, string[] text, List<string>[] son)
    {
        //max.Clear();
        for (int i = 0; i < son.Length; i++)
        {
            son[i].Clear();
        }
        for (int i = 0; i < max.Count; i++)
        {
            for (int j = 0; j < text.Length; j++)
            {
                if (max[i].Substring(0,text[j].Length) == text[j])
                {
                    son[j].Add(max[i].Substring(text[j].Length + 1));
                }
            }

        }
    }
    public List<int> ListOrderChanger(List<int> original)
    {
        List<int> newList = new List<int>();
        int plusNum = original.Count;
        for (int i = 0; i < original.Count; i++)
        {
            if (original[i] != 0)
            {
                newList.Add(original[i]);
                plusNum--;
            }
        }
        for (int i = 0; i < plusNum; i++)
        {
            newList.Add(0);
        }

        return newList;
    }
    /// <summary>
    /// 通过Id获取Item信息：
    /// type：Foods Arms Armors Others Auto
    /// num：0语言包名称 
    ///     1/2：data1/2.
    ///     请勿多输.
    ///     Arm和Armor只有1
    /// </summary>
    public string ItemIdGet(int id, string type, int num)
    {
        int realId;
        List<string> list;
        string idName;//获取编号名称
        switch (type)
        {
            case "Foods":
                list = ItemControl.itemFoods;
                realId = id * 3 - 3;
                idName = list[realId];
                break;
            case "Arms":
                list = ItemControl.itemArms;
                realId = id * 2 - 2;
                idName = list[realId];
                break;
            case "Armors":
                list = ItemControl.itemArmors;
                realId = id * 2 - 2;
                idName = list[realId];
                break;
            case "Others":
                list = ItemControl.itemOthers;
                realId = id * 3 - 3;
                idName = list[realId];
                break;
            case "Auto":
                if (id < 10000)
                {
                    list = ItemControl.itemFoods;
                    realId = id * 3 - 3;
                    idName = list[realId];
                }
                else if (id < 20000)
                {
                    list = ItemControl.itemArms;
                    realId = (id - 10000) * 2 - 2;
                    idName = list[realId];
                }
                else if (id < 30000)
                {
                    list = ItemControl.itemArmors;
                    realId = (id - 20000) * 2 - 2;
                    idName = list[realId];
                }
                else
                {
                    list = ItemControl.itemOthers;
                    realId = (id - 30000) * 3 - 3;
                    idName = list[realId];
                }
                break;
            default:
                goto case "Others";
        }
        string subText = "";
        if (num == 0)//获取语言包内的名称
        {
            for (int i = 0; i < ItemControl.itemTextMaxItem.Count; i++)
            {
                if (ItemControl.itemTextMaxItem[i].Substring(0, idName.Length) == idName)
                {
                    idName = ItemControl.itemTextMaxItem[i].Substring(idName.Length + 1);
                    break;
                }
            }
            for (int i = 0; i < idName.Length; i++)
            {
                if (idName[i] == '\\')
                {
                    break;
                }
                else subText += idName[i];
            }
        }
        else
        {
            subText = list[realId + num];
        }
        return subText;
    }
    /// <summary>
    /// 识别到0后传出
    /// </summary>
    public int FindMax(List<int> list)
    {
        int max = list.Count;
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i] == 0)
            {
                max = i;
                break;
            }
        }
        return max;
    }

    /// <summary>
    /// 通过物品数据名称搞到它的id.
    /// type：Foods Arms Armors Others
    /// </summary>
    public int ItemNameGetId(string name,string type)
    {
        int id = 0, listInt = 0;
        List<string> list;
        switch (type)
        {
            case "Foods":
                list = ItemControl.itemFoods;
                listInt = 3;
                break;

            case "Arms":
                list = ItemControl.itemArms;
                listInt = 2;
                id += 10000;
                break;

            case "Armors":
                list = ItemControl.itemArmors;
                listInt = 2;
                id += 20000;
                break;

            case "Oters":
                list = ItemControl.itemOthers;
                listInt = 3;
                id += 30000;
                break;
            default:
                return 0;
        }
        if (list != null) 
        {
            for (int i = 0; i < list.Count / listInt; i++)
            {
                if (list[i * listInt] == name)
                {
                    id += i + 1;
                    break;
                }
                    
            }
        }
        return id;

    }
}

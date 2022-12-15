using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// ��Ҫ����Overworld��������ͨ�û�������
/// </summary>
[CreateAssetMenu(fileName = "OverwroldControl", menuName = "OverwroldControl")]
public class OverwroldControl : ScriptableObject
{
    public int languagePack;
    public bool pause;//��������ʱ���ֹ��Ҳ������±���

    //UI
    public bool textWidth;//����ȫ���
    public int resolutionLevel;//�ֱ��ʵ�
    public bool fullScreen;//ȫ������
    public bool backGround;//�Ƿ����ò�߿�
    public float mainVolume;//ȫ������
    public bool noSFX;//��Ч ������Ч��ʾ
    public bool openFPS;//��ʾFPS
    public Vector2 resolution;//�ֱ���
    public string owTextsAsset;
    public List<string> owTextsSave;
    public string menuAndSettingAsset;
    public List<string> menuAndSettingSave;
    public string safeText;
    public bool isSetting;
    public List<KeyCode> keyCodes, keyCodesBack1, keyCodesBack2;//��������˳��




    public bool isDebug;
}

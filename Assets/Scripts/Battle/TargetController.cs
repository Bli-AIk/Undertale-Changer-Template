using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;
/// <summary>
/// ����Target
/// </summary>
public class TargetController : MonoBehaviour
{
    Animator anim;
    bool pressZ;
    [Header("������ɵ��˺�")]
    public int hitDamage;
    TextMeshPro hitUI, hitUIb;
    GameObject bar;
    public GameObject hpBar;
    [Header("��������")]
    public int select;

    [Header("�������� Ҫ����Ĺ���")]
    public EnemiesController hitMonster;
    
    void Start()
    {
        anim = GetComponent<Animator>();
        hitUIb = transform.Find("Move/HitTextB").GetComponent<TextMeshPro>();
        hitUI = hitUIb.transform.GetChild(0).GetComponent<TextMeshPro>();
        bar = transform.Find("Bar").gameObject;
        hpBar = transform.Find("Move/EnemiesHp/EnemiesHpOn").gameObject;
    }
    private void OnEnable()
    {
        if(anim == null)
            anim = GetComponent<Animator>();

        //anim.enabled = true;
        anim.SetBool("Hit", false);
        anim.SetFloat("MoveSpeed", 1);
        pressZ = true;
    }
    
    void Update()
    {
        if (!pressZ)
        {
            if (MainControl.instance.KeyArrowToControl(KeyCode.Z))
            {
                pressZ = true;
                anim.SetBool("Hit", true);
                anim.SetFloat("MoveSpeed", 0);
                AudioController.instance.GetFx(0, MainControl.instance.AudioControl.fxClipBattle);
                Hit();

            }
        }
    }
    /// <summary>
    /// ��������ʱ���еļ���
    /// </summary>
    void Hit()
    {
        if (Mathf.Abs(bar.transform.localPosition.x) > 0.8f)
            hitDamage = (int)
                (2.2f / 13.2f * (14 - Mathf.Abs(bar.transform.localPosition.x))//׼ȷ��ϵ��
                * (MainControl.instance.PlayerControl.atk + MainControl.instance.PlayerControl.wearAtk
                - MainControl.instance.BattleControl.enemiesDEF[select] + Random.Range(0, 2)));
        else
            hitDamage = (int)
                 (2.2f / 13.2f * (14 - 0.8f)//׼ȷ��ϵ��
                 * (MainControl.instance.PlayerControl.atk + MainControl.instance.PlayerControl.wearAtk
                 - MainControl.instance.BattleControl.enemiesDEF[select] + Random.Range(0, 2)));

        if (hitDamage <= 0)
        {
            hitDamage = 0;

            hitUI.text = "<color=grey>MISS";
            hitUIb.text = "MISS";
        }
        else
        {
            hitUI.text = "<color=red>" + hitDamage.ToString();
            hitUIb.text = hitDamage.ToString();
        }

    }
    //���½�����anim
    void HitAnim()
    {

        hitMonster.anim.SetBool("Hit", true);
        hpBar.transform.localScale = new Vector3((float)MainControl.instance.BattleControl.enemiesHp[select * 2] / MainControl.instance.BattleControl.enemiesHp[select * 2 + 1], 1);
        MainControl.instance.BattleControl.enemiesHp[select * 2] -= hitDamage;
        DOTween.To(() => hpBar.transform.localScale, x => hpBar.transform.localScale = x, new Vector3((float)MainControl.instance.BattleControl.enemiesHp[select * 2] / MainControl.instance.BattleControl.enemiesHp[select * 2 + 1], 1), 
            0.75f).SetEase(Ease.OutSine);
    }
    void OpenPressZ()
    {
        pressZ = false;
    }
    void ClosePressZ()
    {
        pressZ = true;
    }

    void NotActive()
    {
        //anim.enabled = false;
        gameObject.SetActive(false);
    }
}

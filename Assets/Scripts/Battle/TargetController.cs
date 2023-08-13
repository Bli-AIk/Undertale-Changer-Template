using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;
/// <summary>
/// 控制Target
/// </summary>
public class TargetController : MonoBehaviour
{
    Animator anim;
    bool pressZ;
    [Header("攻击造成的伤害")]
    public int hitDamage;
    TextMeshPro hitUI, hitUIb;
    GameObject bar;
    public GameObject hpBar;
    [Header("父级传入")]
    public int select;

    [Header("父级传入 要击打的怪物")]
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
    /// 攻击敌人时进行的计算
    /// </summary>
    void Hit()
    {
        if (Mathf.Abs(bar.transform.localPosition.x) > 0.8f)
            hitDamage = (int)
                (2.2f / 13.2f * (14 - Mathf.Abs(bar.transform.localPosition.x))//准确度系数
                * (MainControl.instance.PlayerControl.atk + MainControl.instance.PlayerControl.wearAtk
                - MainControl.instance.BattleControl.enemiesDEF[select] + Random.Range(0, 2)));
        else
            hitDamage = (int)
                 (2.2f / 13.2f * (14 - 0.8f)//准确度系数
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
    //以下皆用于anim
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

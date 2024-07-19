using DG.Tweening;
using TMPro;
using UnityEngine;

/// <summary>
/// Control Target
/// </summary>
public class TargetController : MonoBehaviour
{
    private Animator anim;
    private bool pressZ;

    [Header("Damage from attack")]
    public int hitDamage;

    private TextMeshPro hitUI, hitUIb;
    private GameObject bar;
    public GameObject hpBar;

    [Header("parent incoming")]
    public int select;

    [Header("Parent passes in monster to hit")]
    public EnemiesController hitMonster;

    private void Start()
    {
        anim = GetComponent<Animator>();
        hitUIb = transform.Find("Move/HitTextB").GetComponent<TextMeshPro>();
        hitUI = hitUIb.transform.GetChild(0).GetComponent<TextMeshPro>();
        bar = transform.Find("Bar").gameObject;
        hpBar = transform.Find("Move/EnemiesHp/EnemiesHpOn").gameObject;
    }

    private void OnEnable()
    {
        if (anim == null)
            anim = GetComponent<Animator>();

        //anim.enabled = true;; //anim.enabled = true
        anim.SetBool("Hit", false);
        anim.SetFloat("MoveSpeed", 1);
        pressZ = true;
    }

    private void Update()
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
    /// Calculations performed when attacking an enemy
    /// </summary>
    private void Hit()
    {
        if (Mathf.Abs(bar.transform.localPosition.x) > 0.8f)
            hitDamage = (int)
                (2.2f / 13.2f * (14 - Mathf.Abs(bar.transform.localPosition.x))
                //accuracy factor
                * (MainControl.instance.PlayerControl.atk + MainControl.instance.PlayerControl.wearAtk
                - MainControl.instance.BattleControl.enemiesDEF[select] + Random.Range(0, 2)));
        else
            hitDamage = (int)
                 (2.2f / 13.2f * (14 - 0.8f)
                 //accuracy factor
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

    // All of the following are for anim
    private void HitAnim()
    {
        hitMonster.anim.SetBool("Hit", true);
        hpBar.transform.localScale = new Vector3((float)MainControl.instance.BattleControl.enemiesHp[select * 2] / MainControl.instance.BattleControl.enemiesHp[select * 2 + 1], 1);
        MainControl.instance.BattleControl.enemiesHp[select * 2] -= hitDamage;
        DOTween.To(() => hpBar.transform.localScale, x => hpBar.transform.localScale = x, new Vector3((float)MainControl.instance.BattleControl.enemiesHp[select * 2] / MainControl.instance.BattleControl.enemiesHp[select * 2 + 1], 1),
            0.75f).SetEase(Ease.OutSine);
    }

    private void OpenPressZ()
    {
        pressZ = false;
    }

    private void ClosePressZ()
    {
        pressZ = true;
    }

    private void NotActive()
    {
        //anim.enabled = false;; //anim.enabled = false.
        gameObject.SetActive(false);
    }
}

using DG.Tweening;
using TMPro;
using UCT.Global.Audio;
using UCT.Global.Core;
using UnityEngine;

namespace UCT.Battle
{
    /// <summary>
    /// 控制Target
    /// </summary>
    public class TargetController : MonoBehaviour
    {
        private Animator anim;
        private bool pressZ;

        [Header("攻击造成的伤害")]
        public int hitDamage;

        private TextMeshPro hitUI, hitUIb;
        private GameObject bar;
        public GameObject hpBar;

        [Header("父级传入")]
        public int select;

        [Header("父级传入 要击打的怪物")]
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

            //anim.enabled = true;
            anim.SetBool("Hit", false);
            anim.SetFloat("MoveSpeed", 1);
            pressZ = true;
        }

        private void Update()
        {
            if (!pressZ)
            {
                if (MainControl.Instance.KeyArrowToControl(KeyCode.Z))
                {
                    pressZ = true;
                    anim.SetBool("Hit", true);
                    anim.SetFloat("MoveSpeed", 0);
                    AudioController.instance.GetFx(0, MainControl.Instance.AudioControl.fxClipBattle);
                    Hit();
                }
            }
        }

        /// <summary>
        /// 攻击敌人时进行的计算
        /// </summary>
        private void Hit()
        {
            if (Mathf.Abs(bar.transform.localPosition.x) > 0.8f)
                hitDamage = (int)
                    (2.2f / 13.2f * (14 - Mathf.Abs(bar.transform.localPosition.x))//准确度系数
                                  * (MainControl.Instance.PlayerControl.atk + MainControl.Instance.PlayerControl.wearAtk
                                      - MainControl.Instance.BattleControl.enemiesDEF[select] + Random.Range(0, 2)));
            else
                hitDamage = (int)
                    (2.2f / 13.2f * (14 - 0.8f)//准确度系数
                                  * (MainControl.Instance.PlayerControl.atk + MainControl.Instance.PlayerControl.wearAtk
                                      - MainControl.Instance.BattleControl.enemiesDEF[select] + Random.Range(0, 2)));

            if (hitDamage <= 0)
            {
                hitDamage = 0;

                hitUI.text = "<color=grey>MISS";
                hitUIb.text = "MISS";
            }
            else
            {
                hitUI.text = "<color=red>" + hitDamage;
                hitUIb.text = hitDamage.ToString();
            }
        }

        //以下皆用于anim
        private void HitAnim()
        {
            hitMonster.anim.SetBool("Hit", true);
            hpBar.transform.localScale = new Vector3((float)MainControl.Instance.BattleControl.enemiesHp[select * 2] / MainControl.Instance.BattleControl.enemiesHp[select * 2 + 1], 1);
            MainControl.Instance.BattleControl.enemiesHp[select * 2] -= hitDamage;
            DOTween.To(() => hpBar.transform.localScale, x => hpBar.transform.localScale = x, new Vector3((float)MainControl.Instance.BattleControl.enemiesHp[select * 2] / MainControl.Instance.BattleControl.enemiesHp[select * 2 + 1], 1),
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
            //anim.enabled = false;
            gameObject.SetActive(false);
        }
    }
}
using DG.Tweening;
using TMPro;
using UCT.Control;
using UCT.Global.Audio;
using UCT.Global.Core;
using UCT.Service;
using UnityEngine;
using DataHandlerService = UCT.Service.DataHandlerService;

namespace UCT.Battle
{
    /// <summary>
    ///     控制Target
    /// </summary>
    public class TargetController : MonoBehaviour
    {
        private static readonly int Hit = Animator.StringToHash("Hit");
        private static readonly int MoveSpeed = Animator.StringToHash("MoveSpeed");

        [Header("攻击造成的伤害")]
        public int hitDamage;

        public GameObject hpBar;

        [Header("父级传入")]
        public int select;

        [Header("父级传入 要击打的怪物")]
        public EnemiesController hitMonster;

        private Animator _anim;
        private GameObject _bar;

        private TextMeshPro _hitUI, _hitUIb;
        private bool _pressZ;

        private void Start()
        {
            _anim = GetComponent<Animator>();
            _hitUIb = transform.Find("Move/HitTextB").GetComponent<TextMeshPro>();
            _hitUI = _hitUIb.transform.GetChild(0).GetComponent<TextMeshPro>();
            _bar = transform.Find("Bar").gameObject;
            hpBar = transform.Find("Move/EnemiesHp/EnemiesHpOn").gameObject;
        }

        private void Update()
        {
            if (_pressZ)
            {
                return;
            }

            if (!InputService.GetKeyDown(KeyCode.Z))
            {
                return;
            }

            _pressZ = true;
            _anim.SetBool(Hit, true);
            _anim.SetFloat(MoveSpeed, 0);
            AudioController.Instance.PlayFx(0, MainControl.Instance.AudioControl.fxClipBattle);
            HitEnemy();
        }

        private void OnEnable()
        {
            if (!_anim)
            {
                _anim = GetComponent<Animator>();
            }

            _anim.SetBool(Hit, false);
            _anim.SetFloat(MoveSpeed, 1);
            _pressZ = true;
        }

        /// <summary>
        ///     攻击敌人时进行的计算
        /// </summary>
        private void HitEnemy()
        {
            var accuracyFactor = 2.2f / 13.2f * (14 - Mathf.Abs(_bar.transform.localPosition.x));
            hitDamage = Mathf.FloorToInt(accuracyFactor *
                                         (MainControl.Instance.playerControl.atk + DataHandlerService
                                              .GetItemFormDataName(MainControl.Instance.playerControl
                                                  .wearWeapon).Data.Value
                                          - MainControl.Instance.selectUIController
                                              .enemiesControllers[select].def +
                                          Random.Range(0, 2)));

            WeaponItem weaponItem = null;
            if (DataHandlerService.GetItemFormDataName(MainControl.Instance.playerControl.wearArmor) is WeaponItem weapon)
            {
                weaponItem = weapon;
                weaponItem.OnAttack(0);
            }

            if (hitDamage <= 0)
            {
                hitDamage = 0;

                _hitUI.text = "<color=grey>MISS";
                _hitUIb.text = "MISS";
                weaponItem?.OnMiss(0);
            }
            else
            {
                _hitUI.text = $"<color=red>{hitDamage}";
                _hitUIb.text = hitDamage.ToString();
                weaponItem?.OnHit(0);
            }
        }

        //以下皆用于anim
        private void HitAnim()
        {
            hitMonster.anim.SetBool(Hit, true);
            var enemiesController = MainControl.Instance.selectUIController.enemiesControllers[select];
            hpBar.transform.localScale =
                new Vector3(
                    Mathf.Clamp(enemiesController.hp / (float)enemiesController.hpMax, 0, Mathf.Infinity), 1);

            enemiesController.hp -= hitDamage;

            DOTween.To(() => hpBar.transform.localScale, x => hpBar.transform.localScale = x,
                    new Vector3(Mathf.Clamp(enemiesController.hp / (float)enemiesController.hpMax, 0, Mathf.Infinity),
                        1), 0.75f)
                .SetEase(Ease.OutSine);
        }

        private void CheckDeath()
        {
            var enemiesController = MainControl.Instance.selectUIController.enemiesControllers[select];
            if (enemiesController.hp >= 0)
            {
                return;
            }

            enemiesController.Enemy.state = EnemyState.Dead;
            enemiesController.spriteSplitController.enabled = true;
        }

        private void OpenPressZ()
        {
            _pressZ = false;
        }

        private void ClosePressZ()
        {
            _pressZ = true;
        }

        private void NotActive()
        {
            gameObject.SetActive(false);
        }
    }
}
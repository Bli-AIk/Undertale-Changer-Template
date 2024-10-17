using UCT.Global.Core;
using UnityEngine;

namespace UCT.Battle
{
    public class EnemiesHpLineController : MonoBehaviour
    {
        private SpriteRenderer greenSprite;

        [Header("0¿ª")]
        public int number;

        private void Start()
        {
            transform.localScale = Vector2.zero;
            greenSprite = transform.GetChild(0).GetComponent<SpriteRenderer>();
        }

        private void Update()
        {
            if (MainControl.Instance.selectUIController.selectUI == 1 && MainControl.Instance.selectUIController.selectLayer == 1)
            {
                if (MainControl.Instance.BattleControl.enemiesHp.Count - 1 < number * 2)
                    transform.localScale = Vector2.zero;
                else
                {
                    transform.localScale = new Vector3(42, 7.25f, 1);
                    greenSprite.transform.localScale = new Vector3((float)MainControl.Instance.BattleControl.enemiesHp[number * 2] / MainControl.Instance.BattleControl.enemiesHp[number * 2 + 1], greenSprite.transform.localScale.y);
                }
            }
        }
    }
}
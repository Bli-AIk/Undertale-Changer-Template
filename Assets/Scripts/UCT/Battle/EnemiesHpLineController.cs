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
            if (MainControl.instance.selectUIController.selectUI == 1 && MainControl.instance.selectUIController.selectLayer == 1)
            {
                if (MainControl.instance.BattleControl.enemiesHp.Count - 1 < number * 2)
                    transform.localScale = Vector2.zero;
                else
                {
                    transform.localScale = new Vector3(42, 7.25f, 1);
                    greenSprite.transform.localScale = new Vector3((float)MainControl.instance.BattleControl.enemiesHp[number * 2] / MainControl.instance.BattleControl.enemiesHp[number * 2 + 1], greenSprite.transform.localScale.y);
                }
            }
        }
    }
}
using UCT.Global.Core;
using UnityEngine;
using UnityEngine.Serialization;

namespace UCT.Battle
{
    public class EnemiesHpLineController : MonoBehaviour
    {
        [Header("对应怪物IDª")]
        [FormerlySerializedAs("number")] public int enemyId;

        private SpriteRenderer _greenSprite;

        private void Start()
        {
            transform.localScale = Vector2.zero;
            _greenSprite = transform.GetChild(0).GetComponent<SpriteRenderer>();
        }

        private void Update()
        {
            if (MainControl.Instance.selectUIController.selectedButton != SelectUIController.SelectedButton.Fight ||
                MainControl.Instance.selectUIController.selectedLayer !=
                SelectUIController.SelectedLayer.NameLayer)
            {
                return;
            }

            if (MainControl.Instance.selectUIController.enemiesControllers.Count - 1 < enemyId)
            {
                transform.localScale = Vector2.zero;
            }
            else
            {
                transform.localScale = new Vector3(42, 7.25f, 1);
                _greenSprite.transform.localScale =
                    new Vector3(
                        (float)MainControl.Instance.selectUIController.enemiesControllers[enemyId].hp /
                        MainControl.Instance.selectUIController.enemiesControllers[enemyId].hpMax,
                        _greenSprite.transform.localScale.y);
            }
        }
    }
}
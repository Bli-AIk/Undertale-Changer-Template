using UCT.Global.Core;
using UnityEngine;

namespace UCT.Battle
{
    public class EnemiesHpLineController : MonoBehaviour
    {
        [Header("对应怪物IDª")]
        public int enemyId;

        private SpriteRenderer _redSprite, _greenSprite;

        private void Start()
        {
            transform.localScale = Vector2.zero;
            _redSprite = GetComponent<SpriteRenderer>();
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

            var isShowThis = enemyId < MainControl.Instance.selectUIController.enemiesControllers.Count &&
                             MainControl.Instance.selectUIController.enemiesControllers[enemyId].Enemy.state == EnemyState.Default;
            _redSprite.enabled = isShowThis;
            _greenSprite.enabled = isShowThis;
            
            if (!isShowThis)
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
                        Mathf.Clamp((float)MainControl.Instance.selectUIController.enemiesControllers[enemyId].hp /
                                    MainControl.Instance.selectUIController.enemiesControllers[enemyId].hpMax, 0,
                            Mathf.Infinity),
                        _greenSprite.transform.localScale.y);
            }
        }
    }
}
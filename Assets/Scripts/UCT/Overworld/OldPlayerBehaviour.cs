using System;
using UCT.Global.Audio;
using UCT.Global.Core;
using UCT.Global.Settings;
using UCT.Service;
using UnityEngine;

namespace UCT.Overworld
{
    /// <summary>
    ///     Overworld中的玩家控制器
    /// </summary>
    [Obsolete]
    public class OldPlayerBehaviour : MonoBehaviour
    {
        public int animDirectionX, animDirectionY;

        public OldOverworldObjTrigger saveOwObj;
        public float owTimer; //0.1秒，防止调查OW冲突

        private BoxCollider2D _boxCollider;

        private BoxCollider2D _boxTrigger;


        private void Start()
        {
            _boxTrigger = transform.Find("Trigger").GetComponent<BoxCollider2D>();
            _boxCollider = GetComponent<BoxCollider2D>();
            _boxTrigger.transform.localPosition = _boxCollider.offset;
            transform.position = MainControl.Instance.overworldControl.playerScenePos;
            animDirectionX = (int)MainControl.Instance.overworldControl.animDirection.x;
            animDirectionY = (int)MainControl.Instance.overworldControl.animDirection.y;
        }

        private void Update()
        {
            if (MainControl.Instance.overworldControl.isSetting || SettingsStorage.pause) return;

            if (owTimer > 0) owTimer -= Time.deltaTime;

            if (!saveOwObj || !Mathf.Approximately(BackpackBehaviour.Instance.optionsBox.localPosition.z,
                    BackpackBehaviour.BoxZAxisInvisible)) return;
            if (!saveOwObj.isTriggerMode && (saveOwObj.isTriggerMode ||
                                             !InputService.GetKeyDown(KeyCode.Z) ||
                                             (saveOwObj.playerDir != Vector2.one &&
                                              !Mathf.Approximately(saveOwObj.playerDir.x, animDirectionX) &&
                                              !Mathf.Approximately(saveOwObj.playerDir.y, animDirectionY)) ||
                                             BackpackBehaviour.Instance.select != 0)) return;
            if (owTimer <= 0)
            {
                if (saveOwObj.changeScene)
                {
                    if (saveOwObj.onlyDir == 0 || (saveOwObj.onlyDir == -1 && animDirectionX != 0) ||
                        (saveOwObj.onlyDir == 1 && animDirectionY != 0))
                    {
                        MainControl.Instance.overworldControl.playerScenePos = saveOwObj.newPlayerPos;
                        MainControl.Instance.overworldControl.animDirection =
                            new Vector2(animDirectionX, animDirectionY);
                        GameUtilityService.FadeOutAndSwitchScene(saveOwObj.sceneName, Color.black,
                            saveOwObj.banMusic);
                        saveOwObj.gameObject.SetActive(false);
                        saveOwObj = null;
                    }
                }
                else
                {
                    bool isUp;
                    if (saveOwObj.setIsUp)
                        isUp = saveOwObj.isUp;
                    else
                        isUp = transform.position.y < saveOwObj.mainCamera.transform.position.y - 1.25f;

                    if (saveOwObj.openAnim)
                        saveOwObj.AnimTypeText(isUp);
                    else
                        saveOwObj.TypeText(isUp);

                    if (saveOwObj.isSave)
                    {
                        AudioController.Instance.GetFx(2, MainControl.Instance.AudioControl.fxClipUI);
                        if (MainControl.Instance.playerControl.hp < MainControl.Instance.playerControl.hpMax)
                            MainControl.Instance.playerControl.hp = MainControl.Instance.playerControl.hpMax;
                    }
                }
            }

            owTimer = 0.1f;
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (!collision.transform.CompareTag("owObjTrigger")) return;

            var owObj = collision.transform.GetComponent<OldOverworldObjTrigger>();
            saveOwObj = owObj;
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (!collision.transform.CompareTag("owObjTrigger")) return;

            var owObj = collision.transform.GetComponent<OldOverworldObjTrigger>();
            if (owObj == saveOwObj) saveOwObj = null;
        }
    }
}
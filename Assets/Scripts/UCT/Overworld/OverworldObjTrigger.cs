using System;
using System.Collections.Generic;
using DG.Tweening;
using UCT.Global.Audio;
using UCT.Global.Core;
using UCT.Global.UI;
using UCT.Service;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UCT.Overworld
{
    /// <summary>
    /// OWObj触发器相关 配合玩家射线
    /// 用于读取并显示文本然后显示出来
    /// </summary>

    public class OverworldObjTrigger : MonoBehaviour
    {
        //若为true，则碰到就触发。false，按Z触发。
        public bool isTriggerMode;

        public bool setIsUp;
        public bool isUp;
        public string text;

        [Header("检测玩家动画方向 0,0为不检测")]
        public Vector2 playerDir;

        [Header("存档相关")]
        public bool isSave;

        public bool saveFullHp;
        private int _saveSelect;
        private bool _saveOpen;

        [Header("插入摄像机动画相关")]
        public bool openAnim;

        public Vector3 animEndPosPlus;
        public float animTime;
        public Ease animEase;
        public CameraFollowPlayer mainCamera;
        public bool endInBattle;

        [Header("需要渐出就填正数时间")]
        public float stopTime = -1;

        [Header("OW跳场景 只给trigger")]
        public bool changeScene;

        public bool banMusic;
        public string sceneName;
        public Vector3 newPlayerPos;

        [Header("OW跳场景锁定进入时方向 0无 -1左右 1上下")]
        public int onlyDir;

        private AudioSource _bgm;
        private TypeWritter _typeWritter;

        [Header("结束时调用动画器并将下设为true")]
        public bool endAnim;

        public string animRoute;
        public string animBoolName;

        [Header("结束时物体自身关闭")]
        public bool endSelf;

        [Header("确定目前打字的物体")]
        private bool _isTyping;

        [Header("结束时执行方法")]
        public List<string> funNames;

        private void Start()
        {
            transform.tag = "owObjTrigger";
            mainCamera = TalkBoxPositionChanger.Instance.transform.parent.GetComponent<CameraFollowPlayer>();
            _typeWritter = BackpackBehaviour.Instance.typeWritter;
            _bgm = AudioController.Instance.audioSource;
        }

        private void Update()
        {
            if (_saveOpen)
            {
                if (GameUtilityService.KeyArrowToControl(KeyCode.LeftArrow) || GameUtilityService.KeyArrowToControl(KeyCode.RightArrow))
                {
                    _saveSelect = Convert.ToInt32(!Convert.ToBoolean(_saveSelect));

                    BackpackBehaviour.Instance.saveHeart.anchoredPosition = new Vector2(-258 + _saveSelect * 180, -44);
                }
                if (GameUtilityService.KeyArrowToControl(KeyCode.Z))
                {
                    switch (_saveSelect)
                    {
                        case 0:

                            SaveController.SaveData(MainControl.Instance.playerControl, "Data" + MainControl.Instance.saveDataId);
                            _saveSelect = 2;
                            AudioController.Instance.GetFx(12, MainControl.Instance.AudioControl.fxClipUI);
                            var playerName = MainControl.Instance.playerControl.playerName;

                            BackpackBehaviour.Instance.saveHeart.anchoredPosition = new Vector2(10000, 10000);

                            BackpackBehaviour.Instance.saveText.text = $"<color=yellow>{TextProcessingService.PadStringToLength(playerName, 10)}LV{TextProcessingService.PadStringToLength(MainControl.Instance.playerControl.lv.ToString(), 7)}{TextProcessingService.GetRealTime((int)MainControl.Instance.playerControl.gameTime)}\n{TextProcessingService.GetFirstChildStringByPrefix(MainControl.Instance.OverworldControl.settingSave, SceneManager.GetActiveScene().name)}\n{TextProcessingService.RichTextWithEnd("size", 1, "\n")}  {TextProcessingService.GetFirstChildStringByPrefix(MainControl.Instance.OverworldControl.settingSave, "Saved")}";
                            MainControl.Instance.playerControl.saveScene = SceneManager.GetActiveScene().name;
                            PlayerPrefs.SetInt("languagePack", MainControl.Instance.languagePackId);
                            PlayerPrefs.SetInt("dataNumber", MainControl.Instance.saveDataId);
                            PlayerPrefs.SetInt("hdResolution", Convert.ToInt32(MainControl.Instance.OverworldControl.isUsingHDFrame));
                            PlayerPrefs.SetInt("noSFX", Convert.ToInt32(MainControl.Instance.OverworldControl.noSfx));
                            PlayerPrefs.SetInt("vsyncMode", Convert.ToInt32(MainControl.Instance.OverworldControl.vsyncMode));
                            break;

                        case 1:
                            goto default;
                        default:
                            BackpackBehaviour.Instance.saveHeart.anchoredPosition = new Vector2(10000, 10000);
                            BackpackBehaviour.Instance.saveBox.localPosition = new Vector3(BackpackBehaviour.Instance.saveBox.localPosition.x, BackpackBehaviour.Instance.saveBox.localPosition.y, -50);
                            BackpackBehaviour.Instance.saveText.text = "";
                            PressZ();
                            _saveOpen = false;
                            break;

                        case 2:
                            goto default;
                    }
                }
                else if (GameUtilityService.KeyArrowToControl(KeyCode.X))
                {
                    BackpackBehaviour.Instance.saveHeart.anchoredPosition = new Vector2(10000, 10000);
                    BackpackBehaviour.Instance.saveBox.localPosition = new Vector3(BackpackBehaviour.Instance.saveBox.localPosition.x, BackpackBehaviour.Instance.saveBox.localPosition.y, -50);
                    BackpackBehaviour.Instance.saveText.text = "";
                    PressZ();
                    _saveOpen = false;
                }
            }
            //检测相关见PlayerBehaviour
            if (_isTyping && GameUtilityService.KeyArrowToControl(KeyCode.Z) && !_typeWritter.isRunning)
            {
                PressZ();
            }
        }

        private void PressZ()
        {
            if (BackpackBehaviour.Instance.typeMessage.text != "")
            {
                if (endAnim)
                {
                    GameObject.Find(animRoute).GetComponent<Animator>().SetBool(animBoolName, true);
                }

                if (endSelf)
                    gameObject.SetActive(false);
            }

            _isTyping = false;
            BackpackBehaviour.Instance.typeMessage.text = "";
            TalkBoxPositionChanger.Instance.boxDrawer.localPosition = new Vector3(
                TalkBoxPositionChanger.Instance.boxDrawer.localPosition.x,
                TalkBoxPositionChanger.Instance.boxDrawer.localPosition.y, BackpackBehaviour.BoxZAxisInvisible);
            if (isSave && !_saveOpen)
            {
                Save();
                return;
            }

            MainControl.Instance.playerControl.canMove = true;
            MainControl.Instance.OverworldControl.pause = false;

            foreach (var item in funNames)
            {
                var methodInfo = typeof(OverworldObjTrigger).GetMethod(item);
                if (methodInfo == null)
                {
                    Global.Other.Debug.Log(item + "检测失败", gameObject);
                }
                else
                {
                    methodInfo.Invoke(this, Array.Empty<object>());
                }
            }
        }

        private void Save()
        {
            _saveOpen = true;
            _saveSelect = 0;

            BackpackBehaviour.Instance.saveBox.localPosition = new Vector3(BackpackBehaviour.Instance.saveBox.localPosition.x, BackpackBehaviour.Instance.saveBox.localPosition.y, 5);
            var playerName = MainControl.Instance.playerControl.playerName;

            BackpackBehaviour.Instance.saveText.text = TextProcessingService.PadStringToLength(playerName, 10) + "LV" + TextProcessingService.PadStringToLength(MainControl.Instance.playerControl.lv.ToString(), 7) +
                                                     TextProcessingService.GetRealTime((int)MainControl.Instance.playerControl.gameTime) + "\n" +
                                                     TextProcessingService.GetFirstChildStringByPrefix(MainControl.Instance.OverworldControl.settingSave, SceneManager.GetActiveScene().name) + "\n<size=1>\n</size>  " +
                                                     TextProcessingService.GetFirstChildStringByPrefix(MainControl.Instance.OverworldControl.settingSave, "Save") + "         " + TextProcessingService.GetFirstChildStringByPrefix(MainControl.Instance.OverworldControl.settingSave, "Back")
                ;
            BackpackBehaviour.Instance.saveHeart.anchoredPosition = new Vector2(-258, -44);
        }

        /// <summary>
        /// 激活打字。第二个参数别动
        /// </summary>
        public void TypeText(bool inputIsUp, bool isMusic = true)
        {
            _isTyping = true;
            MainControl.Instance.playerControl.canMove = false;
            MainControl.Instance.OverworldControl.pause = true;
            TalkBoxPositionChanger.Instance.Change(true, true);

            if (TalkBoxPositionChanger.Instance.boxDrawer.localPosition.z < 0)
            {
                TalkBoxPositionChanger.Instance.boxDrawer.localPosition = new Vector3(
                    TalkBoxPositionChanger.Instance.boxDrawer.localPosition.x,
                    TalkBoxPositionChanger.Instance.boxDrawer.localPosition.y, BackpackBehaviour.BoxZAxisVisible);
            }
            TalkBoxPositionChanger.Instance.isUp = inputIsUp;

            if (_typeWritter == null)
                _typeWritter = BackpackBehaviour.Instance.typeWritter;

            _typeWritter.TypeOpen(
                TextProcessingService.GetFirstChildStringByPrefix(MainControl.Instance.OverworldControl.sceneTextsSave,
                    text), false, 0, 1, BackpackBehaviour.Instance.typeMessage);
            if (endInBattle)
                _typeWritter.EndInBattle();

            if (isMusic && stopTime >= 0)
                _bgm.DOFade(0, stopTime);
        }

        public void AnimTypeText(bool inputIsUp)
        {
            MainControl.Instance.playerControl.canMove = false;
            MainControl.Instance.OverworldControl.pause = true;
            mainCamera.isFollow = false;
            mainCamera.transform.DOLocalMove(animEndPosPlus, animTime).SetEase(animEase).OnKill(() => TypeText(inputIsUp, false));
            DOTween.To(() => mainCamera.followPosition, x => mainCamera.followPosition = x, animEndPosPlus, animTime).SetEase(animEase);

            if (stopTime >= 0)
                _bgm.DOFade(0, stopTime);
        }
    }
}
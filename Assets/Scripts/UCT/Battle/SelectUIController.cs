using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DG.Tweening;
using TMPro;
using UCT.Audio;
using UCT.Battle.BattleConfigs;
using UCT.Battle.Enemies;
using UCT.Control;
using UCT.Core;
using UCT.Service;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;
using Timer = Plugins.Timer.Source.Timer;

namespace UCT.Battle
{
    /// <summary>
    ///     Battle场景中的UI控制器
    ///     也负责玩家回合的控制
    /// </summary>
    public class SelectUIController : MonoBehaviour
    {
        public enum SelectedButton
        {
            Fight,
            Act,
            Item,
            Mercy
        }

        /// <summary>
        ///     表示当前选择的界面层。
        /// </summary>
        public enum SelectedLayer
        {
            /// <summary>
            ///     按钮层，仅能选择按钮。
            /// </summary>
            ButtonLayer,

            /// <summary>
            ///     名称层，用于显示、选择怪物名称。
            /// </summary>
            NameLayer,

            /// <summary>
            ///     选项层，用于选择选项。
            /// </summary>
            OptionLayer,

            /// <summary>
            ///     对话层，用于显示对话。
            /// </summary>
            NarratorLayer,

            /// <summary>
            ///     回合层，在此层时进入敌方回合。
            /// </summary>
            TurnLayer
        }

        public enum UITextMode
        {
            None,
            Hit,
            Food
        }

        private const string UITextPrefix = "<indent=10></indent>* ";

        private static readonly int IsFlashing = Shader.PropertyToID("_IsFlashing");
        private static readonly int ColorFlash = Shader.PropertyToID("_ColorFlash");
        private static readonly int ColorOn = Shader.PropertyToID("_ColorOn");
        private static readonly int ColorUnder = Shader.PropertyToID("_ColorUnder");
        private static readonly int Crop = Shader.PropertyToID("_Crop");
        private static readonly int Flash = Shader.PropertyToID("_Flash");
        private static readonly int IsFlee = Animator.StringToHash("IsFlee");

        [Header("HP条配色")]
        public Color hpColorUnder;

        public Color hpColorOn;
        public Color hpColorHit;

        [Header("对话气泡载入数")]
        public int numberDialog;

        public bool isDialog;

        [Header("暂存未使用的Sprite")]
        public List<Sprite> spriteUI;

        [HideInInspector] public List<SpriteRenderer> buttons;

        public List<Vector2> playerUIPos;

        [Header("选择的按钮")]
        public SelectedButton selectedButton;

        [Header("目前的UI层级")]
        public SelectedLayer selectedLayer;

        [Header("选择的名称编号")]
        public int nameLayerIndex;

        [Header("选择的选项")]
        public int optionLayerIndex;

        [FormerlySerializedAs("actSave")] [Header("暂存ACT选项以便调用")]
        public List<string> optionsSave;

        [Header("怪物清单")]
        public List<EnemiesController> enemiesControllers;

        [Header("首次进入回合的时候播放自定义的回合文本")]
        public bool firstIn;

        public int firstInDiy = -1;
        private DialogBubbleBehaviour _dialog;
        private GameObject _enemiesHpLine;
        private int _hpFood;
        private Tween _hpFoodTween;
        private SpriteRenderer _hpSpr;
        private bool _isEndBattle;

        private ItemScroller _itemScroller;

        private TextMeshPro _nameUI, _hpUI, _textUI, _textUIBack;

        private int _saveTurn = -1;
        private string _saveTurnText = "";

        private TargetController _target;
        private TypeWritter _typeWritter;

        private void Start()
        {
            GetComponent();
            TurnTextLoad(true);
            _enemiesHpLine.SetActive(false);
            UITextUpdate();
            _hpFood = MainControl.Instance.playerControl.hp;
            EnterPlayerTurn();
        }

        private void Update()
        {
            if (GameUtilityService.IsGamePausedOrSetting() || _isEndBattle)
            {
                return;
            }

            if (MainControl.Instance.playerControl.isDebug)
            {
                NameUIUpdate();
            }

            if (TurnController.Instance.isMyTurn)
            {
                PlayerTurn();
            }

            UpdateDialog();
        }

        private void UpdateDialog()
        {
            if (MainControl.Instance.selectUIController.enemiesControllers == null)
            {
                return;
            }
            _dialog.gameObject.SetActive(isDialog);


            var canEndBattle = MainControl.Instance.selectUIController.enemiesControllers.All(enemiesController =>
                enemiesController.Enemy.state is not (EnemyState.Default or EnemyState.CanSpace));

            if (!canEndBattle)
            {
                if (!isDialog)
                {
                    return;
                }

                if ((_dialog.typeWritter.isTyping || !InputService.GetKeyDown(KeyCode.Z)) &&
                    ((selectedButton != SelectedButton.Fight && _textUI.text != "") || numberDialog != 0))
                {
                    return;
                }

                if (numberDialog < optionsSave.Count)
                {
                    KeepDialogBubble();
                }
                else
                {
                    EnterTurnLayer();
                    TurnController.Instance.EnterEnemyTurn();
                }
            }
            else
            {
                EndBattle();
            }
        }

        private void EndBattle()
        {
            if (_isEndBattle)
            {
                return;
            }

            EnterTurnLayer();
            _isEndBattle = true;
            Timer.Register(1, ExitBattleScene);
        }

        private void ExitBattleScene()
        {
            _isEndBattle = true;
            AudioController.Instance.audioSource.DOFade(0, 0.5f);
            var (exp, gold) = GetEnemiesExpAndGold();

            StartTypeWritter(string.Format(TextProcessingService.GetFirstChildStringByPrefix(
                MainControl.Instance.LanguagePackControl.sceneTexts,
                "Won"), exp, gold));
            MainControl.Instance.playerControl.exp += exp;
            MainControl.Instance.playerControl.gold += gold;
            _typeWritter.OnClose = () =>
            {
                GameUtilityService.FadeOutAndSwitchScene(MainControl.Instance.playerControl.lastScene,
                    Color.black);
            };
        }

        private (int exp, int gold) GetEnemiesExpAndGold()
        {
            var exp = 0;
            var gold = 0;
            foreach (var enemy in enemiesControllers.Select(enemiesController => enemiesController.Enemy))
            {
                if (enemy.state is EnemyState.Dead)
                {
                    exp += enemy.exp;
                }

                if (enemy.state is EnemyState.Dead or EnemyState.Spaced)
                {
                    gold += enemy.gold;
                }
            }

            return (exp, gold);
        }

        private void EnterTurnLayer()
        {
            isDialog = false;
            _itemScroller.gameObject.SetActive(false);
            optionsSave = new List<string>();
            selectedLayer = SelectedLayer.TurnLayer;
        }

        private void GetComponent()
        {
            _target = transform.Find("Target").GetComponent<TargetController>();
            _target.gameObject.SetActive(false);
            _nameUI = transform.Find("Name UI").GetComponent<TextMeshPro>();
            _hpUI = transform.Find("HP UI").GetComponent<TextMeshPro>();
            _textUI = transform.Find("Text UI").GetComponent<TextMeshPro>();
            _textUIBack = transform.Find("Text UI Back").GetComponent<TextMeshPro>();
            _hpSpr = transform.Find("HP").GetComponent<SpriteRenderer>();
            _itemScroller = transform.Find("ItemSelect").GetComponent<ItemScroller>();
            _enemiesHpLine = transform.Find("EnemiesHpLine").gameObject;
            _dialog = GameObject.Find("DialogBubble").GetComponent<DialogBubbleBehaviour>();
            _dialog.gameObject.SetActive(false);
            _typeWritter = GetComponent<TypeWritter>();
            foreach (var t in new[] { "FIGHT", "ACT", "ITEM", "MERCY" })
            {
                buttons.Add(transform.Find(t).GetComponent<SpriteRenderer>());
            }

            SetBattleConfig(MainControl.Instance.BattleControl.BattleConfig);
        }

        private void SetBattleConfig(IBattleConfig config)
        {
            foreach (var enemy in config.enemies)
            {
                var obj = Instantiate(enemy);
                obj.name = enemy.name;
                enemiesControllers.Add(obj.transform.GetComponent<EnemiesController>());
            }

            selectedButton = EnumService.GetMinEnumValue<SelectedButton>();

            Instantiate(config.backGroundModel);
            RenderSettings.skybox = config.skyBox;

            var volume = GameObject.Find("Global Volume").GetComponent<UnityEngine.Rendering.Volume>();
            volume.profile = config.volumeProfile;
        }

        /// <summary>
        ///     UI打字 打字完成后不会强制控死文本
        /// </summary>
        private void StartTypeWritter(string text)
        {
            _typeWritter.StartTypeWritter(text, _textUI);
        }

        /// <summary>
        ///     战术互换
        /// </summary>
        private void SpriteChange()
        {
            (buttons[(int)selectedButton].sprite, spriteUI[(int)selectedButton]) = (
                spriteUI[(int)selectedButton], buttons[(int)selectedButton].sprite);
        }

        /// <summary>
        ///     selectUI=1时的设定
        ///     主要为选定怪物
        /// </summary>
        private void LayerOneSet()
        {
            while (nameLayerIndex < enemiesControllers.Count - 1 && nameLayerIndex + 1 < enemiesControllers.Count &&
                   enemiesControllers[nameLayerIndex].Enemy.state is not (EnemyState.Default or EnemyState.CanSpace) &&
                   enemiesControllers[nameLayerIndex + 1].Enemy.state is EnemyState.Default or EnemyState.CanSpace)
            {
                nameLayerIndex++;
            }

            if (InputService.GetKeyDown(KeyCode.DownArrow))
            {
                LayerOneSetKeyDown();
            }

            if (InputService.GetKeyDown(KeyCode.UpArrow))
            {
                LayerOneSetKeyUp();
            }

            MainControl.Instance.battlePlayerController.transform.position = new Vector3(
                -5.175f, -0.96f - nameLayerIndex * 0.66f,
                MainControl.Instance.battlePlayerController.transform.position.z);
        }

        private void LayerOneSetKeyUp()
        {
            var isPlayFx = false;
            while (nameLayerIndex > 0 &&
                   enemiesControllers[nameLayerIndex - 1].Enemy.state is EnemyState.Default or EnemyState.CanSpace)
            {
                nameLayerIndex--;
                if (isPlayFx)
                {
                    continue;
                }

                AudioController.Instance.PlayFx(0, MainControl.Instance.AudioControl.fxClipUI);
                isPlayFx = true;
            }
        }

        private void LayerOneSetKeyDown()
        {
            var isPlayFx = false;
            while (nameLayerIndex < enemiesControllers.Count - 1 && nameLayerIndex + 1 < enemiesControllers.Count &&
                   enemiesControllers[nameLayerIndex + 1].Enemy.state is EnemyState.Default or EnemyState.CanSpace)
            {
                nameLayerIndex++;
                if (isPlayFx)
                {
                    continue;
                }

                AudioController.Instance.PlayFx(0, MainControl.Instance.AudioControl.fxClipUI);
                isPlayFx = true;
            }
        }


        /// <summary>
        ///     进我方回合
        /// </summary>
        public void EnterPlayerTurn()
        {
            TurnController.Instance.isMyTurn = true;
            selectedLayer = SelectedLayer.ButtonLayer;
            selectedButton = EnumService.GetMinEnumValue<SelectedButton>();
            nameLayerIndex = 0;
            optionLayerIndex = 0;
            SpriteChange();
            TurnTextLoad();
            _itemScroller.gameObject.SetActive(true);

            if (MainControl.Instance.battlePlayerController)
            {
                MainControl.Instance.battlePlayerController.collideCollider.enabled = false;
            }
        }

        /// <summary>
        ///     玩家回合
        /// </summary>
        private void PlayerTurn()
        {
            switch (selectedLayer)
            {
                case SelectedLayer.ButtonLayer:
                {
                    UpdateButtonLayer();
                    break;
                }

                case SelectedLayer.NameLayer:
                {
                    UpdateNameLayer();
                    break;
                }

                case SelectedLayer.OptionLayer:
                {
                    UpdateOptionLayer();
                    break;
                }

                case SelectedLayer.NarratorLayer:
                {
                    UpdateNarratorLayer();
                    break;
                }
                case SelectedLayer.TurnLayer:
                {
                    break;
                }
                default:
                {
                    throw new ArgumentOutOfRangeException($"Unexpected selectedLayer value: {selectedLayer}");
                }
            }
        }

        private void UpdateButtonLayer()
        {
            MainControl.Instance.battlePlayerController.transform.position =
                (Vector3)playerUIPos[(int)selectedButton] + new Vector3(0, 0,
                    MainControl.Instance.battlePlayerController.transform.position.z);
            ButtonLayerInput();

            _hpUI.text = FormatWithLeadingZero(_hpFood) + " / " +
                         FormatWithLeadingZero(MainControl.Instance.playerControl.hpMax);
        }

        private void ButtonLayerInput()
        {
            if (InputService.GetKeyDown(KeyCode.LeftArrow))
            {
                AudioController.Instance.PlayFx(0, MainControl.Instance.AudioControl.fxClipUI);
                if (selectedButton >= EnumService.GetMinEnumValue<SelectedButton>())
                {
                    SpriteChange();
                    if (selectedButton == EnumService.GetMinEnumValue<SelectedButton>())
                    {
                        selectedButton = EnumService.GetMaxEnumValue<SelectedButton>();
                    }
                    else
                    {
                        selectedButton -= 1;
                    }

                    SpriteChange();
                }
            }
            else if (InputService.GetKeyDown(KeyCode.RightArrow))
            {
                AudioController.Instance.PlayFx(0, MainControl.Instance.AudioControl.fxClipUI);
                if (selectedButton <= EnumService.GetMaxEnumValue<SelectedButton>())
                {
                    SpriteChange();
                    if (selectedButton == EnumService.GetMaxEnumValue<SelectedButton>())
                    {
                        selectedButton = EnumService.GetMinEnumValue<SelectedButton>();
                    }
                    else
                    {
                        selectedButton += 1;
                    }

                    SpriteChange();
                }
            }

            ButtonLayerConfirm();
        }

        private void ButtonLayerConfirm()
        {
            if (!InputService.GetKeyDown(KeyCode.Z))
            {
                return;
            }

            selectedLayer = SelectedLayer.NameLayer;
            optionLayerIndex = 0;
            AudioController.Instance.PlayFx(1, MainControl.Instance.AudioControl.fxClipUI);
            _typeWritter.TypeStop();
            _textUI.text = "";

            if (selectedButton != SelectedButton.Item)
            {
                SetEnemiesName();
            }
            else
            {
                MainControl.Instance.playerControl.items =
                    ListManipulationService.CheckAllDataNamesInItemList(MainControl.Instance.playerControl
                        .items);
                MainControl.Instance.playerControl.items =
                    ListManipulationService.MoveNullOrEmptyToEnd(MainControl.Instance.playerControl.items);

                _textUIBack.rectTransform.anchoredPosition = new Vector2(-5, -3.3f);
                _textUIBack.alignment = TextAlignmentOptions.TopRight;
                _hpSpr.material.SetFloat(IsFlashing, 1);
                _hpSpr.material.SetColor(ColorFlash, hpColorOn);

                _hpFood = MainControl.Instance.playerControl.hp;
            }

            if (string.IsNullOrEmpty(MainControl.Instance.playerControl.items[0]) &&
                selectedButton == SelectedButton.Item)
            {
                selectedLayer = SelectedLayer.ButtonLayer;
            }
        }

        private void SetEnemiesName()
        {
            foreach (var enemy in enemiesControllers)
            {
                if (enemy.Enemy.state is not (EnemyState.Default or EnemyState.CanSpace))
                {
                    _textUI.text += "\n";
                    continue;
                }

                var save = TextProcessingService.GetFirstChildStringByPrefix(
                    MainControl.Instance.BattleControl.enemiesNameSave, enemy.gameObject.name);
                if (enemy.Enemy.state == EnemyState.CanSpace)
                {
                    _textUI.text += "<color=yellow>";
                }

                _textUI.text += $"{UITextPrefix}{save}\n</color>";
            }
        }

        private void UpdateNameLayer()
        {
            if (NameLayerCancel())
            {
                return;
            }

            NameLayerConfirmCommon();

            switch (selectedButton)
            {
                case SelectedButton.Fight:
                {
                    NameLayerConfirmFight();
                    break;
                }

                case SelectedButton.Act:
                {
                    NameLayerConfirmAct();
                    break;
                }

                case SelectedButton.Item:
                {
                    NameLayerConfirmItem();
                    break;
                }

                case SelectedButton.Mercy:
                {
                    NameLayerConfirmMercy();
                    break;
                }
                default:
                {
                    throw new ArgumentOutOfRangeException($"Unexpected selectedButton value: {selectedButton}");
                }
            }
        }

        /// <summary>
        ///     MERCY：选择敌人
        /// </summary>
        private void NameLayerConfirmMercy()
        {
            LayerOneSet();
            if (!InputService.GetKeyDown(KeyCode.Z))
            {
                return;
            }

            var save = TextProcessingService.BatchGetFirstChildStringByPrefix(
                MainControl.Instance.BattleControl.mercySave,
                enemiesControllers[nameLayerIndex].name + "\\");
            TextProcessingService.SplitStringToListWithDelimiter(save, optionsSave);

            _textUI.text = NameLayerSetMercyText(0);
            _textUI.text += NameLayerSetMercyText(1);
            _textUI.text += NameLayerSetMercyText(2);
        }

        private string NameLayerSetMercyText(int index)
        {
            var result = new StringBuilder();
            if (enemiesControllers[nameLayerIndex].Enemy.state == EnemyState.CanSpace &&
                index < enemiesControllers[nameLayerIndex].Enemy.MercyTypes.Length &&
                enemiesControllers[nameLayerIndex].Enemy.MercyTypes[index] == MercyType.Mercy)
            {
                result.Append("<color=yellow>");
            }

            index *= 2;
            if (optionsSave.Count <= index)
            {
                return null;
            }

            result.Append($"{UITextPrefix}{optionsSave[index]}</color>\n");
            return result.ToString();
        }

        /// <summary>
        ///     ITEM：跳2
        /// </summary>
        private void NameLayerConfirmItem()
        {
            _itemScroller.Open(ListManipulationService.FindFirstNullOrEmptyIndex(MainControl.Instance
                .playerControl
                .items), 0);
            selectedLayer = SelectedLayer.OptionLayer;

            var item = DataHandlerService.GetItemFormDataName(
                MainControl.Instance.playerControl.items[nameLayerIndex]);
            if (item is FoodItem or ParentFoodItem)
            {
                UITextUpdate(UITextMode.Food, item.Data.Value);
            }
            else
            {
                UITextUpdate(UITextMode.Food);
            }
        }

        /// <summary>
        ///     ACT：选择敌人
        /// </summary>
        private void NameLayerConfirmAct()
        {
            LayerOneSet();
            if (!InputService.GetKeyDown(KeyCode.Z))
            {
                return;
            }

            var save = TextProcessingService.BatchGetFirstChildStringByPrefix(
                MainControl.Instance.BattleControl.actSave,
                enemiesControllers[nameLayerIndex].name + "\\");
            TextProcessingService.SplitStringToListWithDelimiter(save, optionsSave);
            SetActTexts();

            for (var i = 0; i < optionsSave.Count; i++)
            {
                optionsSave[i] += ';';
            }

            optionsSave = DataHandlerService.ChangeItemData(optionsSave, false,
                new List<string>
                {
                    enemiesControllers[nameLayerIndex].name,
                    enemiesControllers[nameLayerIndex].atk.ToString(),
                    enemiesControllers[nameLayerIndex].def.ToString()
                });

            for (var i = 0; i < optionsSave.Count; i++)
            {
                optionsSave[i] = optionsSave[i][..(optionsSave[i].Length - 1)];
            }

            _textUIBack.rectTransform.anchoredPosition = new Vector2(10.75f, -3.3f);
            _textUIBack.alignment = TextAlignmentOptions.TopLeft;
        }

        private void SetActTexts()
        {
            var options = enemiesControllers[nameLayerIndex].OnOptions;
            var enemyCount = enemiesControllers.Count;

            if (options == null || options.Length == 0)
            {
                return;
            }

            const string noLPack = "No L-Pack!";
            const string noLanguagePack = "No Language Pack!";
            EnsureActSaveSize(2, noLPack);
            _textUI.text = new StringBuilder().Append(UITextPrefix).Append(optionsSave[0]).ToString();
            _textUIBack.text = "";

            for (var i = 0; i < options.Length; i++)
            {
                var index = i * 2;
                EnsureActSaveSize(index + 2, noLPack);

                if (optionsSave.Count > i * enemyCount)
                {
                    if (i % 2 == 1)
                    {
                        _textUIBack.text += $"* {optionsSave[index]}\n";
                    }
                    else
                    {
                        _textUI.text += $"{UITextPrefix}{optionsSave[index]}\n";
                    }
                }
                else
                {
                    if (i % 2 == 1)
                    {
                        _textUIBack.text += $"* {noLPack}\n";
                    }
                    else
                    {
                        _textUI.text += $"{UITextPrefix}{noLPack}\n";
                    }

                    Debug.LogError(noLanguagePack);
                }
            }
        }

        private void EnsureActSaveSize(int size, string defaultValue)
        {
            while (optionsSave.Count < size)
            {
                optionsSave.Add(defaultValue);
            }
        }


        /// <summary>
        ///     FIGHT：选择敌人
        /// </summary>
        private void NameLayerConfirmFight()
        {
            _enemiesHpLine.SetActive(true);
            LayerOneSet();
            if (!InputService.GetKeyDown(KeyCode.Z))
            {
                return;
            }

            _enemiesHpLine.SetActive(false);
            _target.gameObject.SetActive(true);
            _target.select = nameLayerIndex;

            var move = _target.transform.Find("Move");
            move.transform.position = new Vector3(
                enemiesControllers[nameLayerIndex].transform.position.x,
                move.transform.position.y);

            _target.hitMonster = enemiesControllers[nameLayerIndex];
            MainControl.Instance.battlePlayerController.transform.position =
                (Vector3)(Vector2.one * 10000) + new Vector3(0, 0,
                    MainControl.Instance.battlePlayerController.transform.position.z);
        }

        private void NameLayerConfirmCommon()
        {
            if (!InputService.GetKeyDown(KeyCode.Z) || selectedButton == SelectedButton.Item)
            {
                return;
            }

            if (selectedButton != SelectedButton.Fight)
            {
                selectedLayer = SelectedLayer.OptionLayer;
            }
            else
            {
                selectedLayer = SelectedLayer.NarratorLayer;
                SpriteChange();
            }

            optionLayerIndex = 0;
            _textUI.text = "";
            AudioController.Instance.PlayFx(1, MainControl.Instance.AudioControl.fxClipUI);
        }

        private bool NameLayerCancel()
        {
            if (!InputService.GetKeyDown(KeyCode.X))
            {
                return false;
            }

            selectedLayer = SelectedLayer.ButtonLayer;
            nameLayerIndex = 0;
            if (!firstIn)
            {
                TurnTextLoad();
            }
            else
            {
                TurnTextLoad(true, firstInDiy);
            }

            _enemiesHpLine.SetActive(false);
            return true;
        }

        private void UpdateOptionLayer()
        {
            switch (selectedButton)
            {
                case SelectedButton.Fight:
                {
                    break;
                }
                case SelectedButton.Act:
                {
                    ActOptionLayer();
                    break;
                }

                case SelectedButton.Item:
                {
                    ItemOptionLayer(ref nameLayerIndex, ref optionLayerIndex);
                    break;
                }

                case SelectedButton.Mercy:
                {
                    MercyOptionLayer();
                    break;
                }
                default:
                {
                    throw new ArgumentOutOfRangeException($"Unexpected selectedButton value: {selectedButton}");
                }
            }
        }

        private void MercyOptionLayer()
        {
            MainControl.Instance.battlePlayerController.transform.position = new Vector3(-5.175f,
                -0.96f - optionLayerIndex * 0.66f,
                MainControl.Instance.battlePlayerController.transform.position.z);
            if (InputService.GetKeyDown(KeyCode.X))
            {
                selectedLayer = SelectedLayer.NameLayer;
                optionLayerIndex = 0;
                _textUI.text = "";
                SetEnemiesName();
            }
            else if (InputService.GetKeyDown(KeyCode.Z))
            {
                selectedLayer = SelectedLayer.NarratorLayer;

                SpriteChange();
                _itemScroller.Close();

                if (ExecuteMercy())
                {
                    return;
                }

                MainControl.Instance.battlePlayerController.transform.position =
                    (Vector3)(Vector2.one * 10000) + new Vector3(0, 0,
                        MainControl.Instance.battlePlayerController.transform.position.z);

                if (optionsSave[2 * (optionLayerIndex + 1) - 1] != "Null")
                {
                    StartTypeWritter(optionsSave[2 * (optionLayerIndex + 1) - 1]);
                }
                else
                {
                    _textUI.text = "";
                    MainControl.Instance.battlePlayerController.transform.position =
                        (Vector3)MainControl.Instance.battlePlayerController.sceneDrift + new Vector3(0,
                            0, MainControl.Instance.battlePlayerController.transform.position.z);
                    TryOpenDialogBubble();
                }
            }

            if (InputService.GetKeyDown(KeyCode.UpArrow) && optionLayerIndex - 1 >= 0)
            {
                AudioController.Instance.PlayFx(0, MainControl.Instance.AudioControl.fxClipUI);
                optionLayerIndex--;
            }
            else if (InputService.GetKeyDown(KeyCode.DownArrow) &&
                     optionLayerIndex + 1 <= optionsSave.Count / 2 - 1)
            {
                AudioController.Instance.PlayFx(0, MainControl.Instance.AudioControl.fxClipUI);
                optionLayerIndex++;
            }
        }

        private bool ExecuteMercy()
        {
            var enemiesController = enemiesControllers[nameLayerIndex];
            var enemy = enemiesController.Enemy;
            var enemyMercyType = enemy.MercyTypes[optionLayerIndex];
            switch (enemyMercyType)
            {
                case MercyType.Null:
                {
                    break;
                }
                case MercyType.Mercy:
                {
                    if (Mercy(enemy, enemiesController))
                    {
                        return true;
                    }
                    break;
                }
                case MercyType.Flee:
                {
                    if (Flee())
                    {
                        return true;
                    }
                    break;
                }
                case MercyType.ActLike:
                {
                    var realIndex = enemy.MercyTypes.TakeWhile((_, index) => index != optionLayerIndex)
                        .Count(mercyType => mercyType == MercyType.ActLike);
                    var options = enemy.GetActLikeOptions();
                    options[realIndex]?.Invoke();
                    break;
                }
                default:
                {
                    throw new ArgumentOutOfRangeException($"Unexpected enemyMercyType value: {enemyMercyType}");
                }
            }

            return false;
        }

        private bool Mercy(IEnemy enemy, EnemiesController enemiesController)
        {
            if (enemy.state == EnemyState.CanSpace)
            {
                //TODO: 怪物饶恕特效
                enemy.state = EnemyState.Spaced;
                enemiesController.spriteSplitController.spriteRenderer.color = Color.gray;

                foreach (Transform child in enemiesController.spriteSplitController.transform)
                {
                    child.gameObject.SetActive(false);
                }

                AudioController.Instance.PlayFx(5, MainControl.Instance.AudioControl.fxClipBattle);


                if (enemiesControllers.All(item => item.Enemy.state is EnemyState.Spaced or EnemyState.Dead))
                {
                    MainControl.Instance.battlePlayerController.transform.position =
                        (Vector3)(Vector2.one * 10000) + new Vector3(0, 0,
                            MainControl.Instance.battlePlayerController.transform.position.z);
                    ExitBattleScene();
                    return true;
                }
            }

            return false;
        }

        private bool Flee()
        {
            var isFlee = MathUtilityService.WeightedRandom(0.5f);

            if (!isFlee)
            {
                return false;
            }

            MainControl.Instance.battlePlayerController.animator.SetBool(IsFlee, true);
            MainControl.Instance.battlePlayerController.transform.DOMoveX(
                    MainControl.Instance.battlePlayerController.transform.position.x - 5, 3.75f)
                .SetEase(Ease.Linear);

            var (exp, gold) = GetEnemiesExpAndGold();
            if (exp == 0 && gold == 0)
            {
                _textUI.text = TextProcessingService.GetFirstChildStringByPrefix(
                    MainControl.Instance.LanguagePackControl.sceneTexts, "Flee");
            }
            else
            {
                _textUI.text = string.Format(TextProcessingService.GetFirstChildStringByPrefix(
                    MainControl.Instance.LanguagePackControl.sceneTexts, "FleeWithSpoil"), exp, gold);
            }

            MainControl.Instance.playerControl.exp += exp;
            MainControl.Instance.playerControl.gold += gold;

            GameUtilityService.FadeOutAndSwitchScene(MainControl.Instance.playerControl.lastScene,
                Color.black, null, true, 2);
            return true;

        }

        private void ActOptionLayer()
        {
            ActOptionLayerSelect();

            if (InputService.GetKeyDown(KeyCode.X))
            {
                selectedLayer = SelectedLayer.NameLayer;
                optionLayerIndex = 0;
                _textUI.text = "";
                _textUIBack.text = "";
                SetEnemiesName();
            }
            else if (InputService.GetKeyDown(KeyCode.Z))
            {
                ActOptionLayerOptions();

                _textUIBack.text = "";
                selectedLayer = SelectedLayer.NarratorLayer;
                MainControl.Instance.battlePlayerController.transform.position =
                    (Vector3)(Vector2.one * 10000) + new Vector3(0, 0,
                        MainControl.Instance.battlePlayerController.transform.position.z);
                StartTypeWritter(optionsSave[2 * (optionLayerIndex + 1) - 1]);
                SpriteChange();
                _itemScroller.Close();
            }
        }

        /// <summary>
        ///     ACT触发选项
        /// </summary>
        private void ActOptionLayerOptions()
        {
            var options = enemiesControllers[nameLayerIndex].OnOptions;
            options?[optionLayerIndex]();
        }

        private void ActOptionLayerSelect()
        {
            var count = ActOptionLayerGetCount();

            if (InputService.GetKeyDown(KeyCode.UpArrow) && optionLayerIndex - 2 >= 0)
            {
                AudioController.Instance.PlayFx(0, MainControl.Instance.AudioControl.fxClipUI);
                optionLayerIndex -= 2;
            }
            else if (InputService.GetKeyDown(KeyCode.DownArrow) &&
                     optionLayerIndex + 2 <= count)
            {
                AudioController.Instance.PlayFx(0, MainControl.Instance.AudioControl.fxClipUI);
                optionLayerIndex += 2;
            }

            if (InputService.GetKeyDown(KeyCode.LeftArrow) &&
                optionLayerIndex - 1 >= 0)
            {
                AudioController.Instance.PlayFx(0, MainControl.Instance.AudioControl.fxClipUI);
                optionLayerIndex--;
            }
            else if (InputService.GetKeyDown(KeyCode.RightArrow) &&
                     optionLayerIndex + 1 <= count)
            {
                AudioController.Instance.PlayFx(0, MainControl.Instance.AudioControl.fxClipUI);
                optionLayerIndex++;
            }

            float playerPosX, playerPosY;
            if ((optionLayerIndex - 1) % 2 == 0)
            {
                playerPosX = 0.25f;
            }
            else
            {
                playerPosX = -5.175f;
            }

            if (optionLayerIndex < 2)
            {
                playerPosY = -0.96f - 0 * 0.66f;
            }
            else
            {
                playerPosY = -0.96f - 1 * 0.66f;
            }

            MainControl.Instance.battlePlayerController.transform.position = new Vector3(playerPosX,
                playerPosY, MainControl.Instance.battlePlayerController.transform.position.z);
        }

        private int ActOptionLayerGetCount()
        {
            var count = optionsSave.Count / 2 - 1;
            var options = enemiesControllers[nameLayerIndex].OnOptions;
            if (options != null)
            {
                if (count > options.Length - 1)
                {
                    count = options.Length - 1;
                }
            }
            else
            {
                throw new ArgumentNullException($"No {enemiesControllers[nameLayerIndex].OnOptions}!");
            }

            return count;
        }


        private void UpdateNarratorLayer()
        {
            firstIn = false;

            if (selectedButton == SelectedButton.Fight && !_target.gameObject.activeSelf)
            {
                if (isDialog)
                {
                    return;
                }

                _textUI.text = "";
                MainControl.Instance.battlePlayerController.transform.position =
                    (Vector3)MainControl.Instance.battlePlayerController.sceneDrift + new Vector3(0, 0,
                        MainControl.Instance.battlePlayerController.transform.position.z);
                TryOpenDialogBubble();
            }
            else if (InputService.GetKeyDown(KeyCode.Z))
            {
                ContinueNarratorLayer();
            }
        }

        private void ContinueNarratorLayer()
        {
            MainControl.Instance.battlePlayerController.collideCollider.enabled = true;
            if (isDialog)
            {
                return;
            }

            if (selectedButton != SelectedButton.Fight && _textUI.text == "")
            {
                OpenDialogBubble(
                    MainControl.Instance.BattleControl.turnDialogAsset[TurnController.Instance.turn]);
                MainControl.Instance.battlePlayerController.transform.position =
                    (Vector3)MainControl.Instance.battlePlayerController.sceneDrift + new Vector3(0, 0,
                        MainControl.Instance.battlePlayerController.transform.position.z);
                return;
            }

            if (selectedButton == SelectedButton.Fight || _typeWritter.isTyping ||
                !InputService.GetKeyDown(KeyCode.Z))
            {
                return;
            }

            _textUI.text = "";
            MainControl.Instance.battlePlayerController.transform.position =
                (Vector3)MainControl.Instance.battlePlayerController.sceneDrift + new Vector3(0,
                    0, MainControl.Instance.battlePlayerController.transform.position.z);

            TryOpenDialogBubble();
        }

        private void TryOpenDialogBubble()
        {
            if (TurnController.Instance.turn < MainControl.Instance.BattleControl.turnDialogAsset.Count)
            {
                OpenDialogBubble(
                    MainControl.Instance.BattleControl.turnDialogAsset[TurnController.Instance.turn]);
            }
            else
            {
                EnterTurnLayer();
                TurnController.Instance.EnterEnemyTurn();
            }
        }


        private void ItemOptionLayer(ref int globalItemIndex, ref int visibleItemIndex)
        {
            if (InputService.GetKeyDown(KeyCode.X))
            {
                selectedLayer = SelectedLayer.ButtonLayer;
                globalItemIndex = 0;
                if (!firstIn)
                {
                    TurnTextLoad();
                }
                else
                {
                    TurnTextLoad(true, firstInDiy);
                }

                _itemScroller.Close();

                _textUIBack.rectTransform.anchoredPosition = new Vector2(10.75f, -3.3f);
                _textUIBack.alignment = TextAlignmentOptions.TopLeft;
                _textUIBack.text = "";

                UITextUpdate(UITextMode.Food);
                return;
            }

            if (InputService.GetKeyDown(KeyCode.Z))
            {
                selectedLayer = SelectedLayer.NarratorLayer;
                MainControl.Instance.battlePlayerController.transform.position =
                    (Vector3)(Vector2.one * 10000) + new Vector3(0, 0,
                        MainControl.Instance.battlePlayerController.transform.position.z);
                var dataName = MainControl.Instance.playerControl.items[globalItemIndex + 1];

                TypeWritterTagProcessor.SetItemDataName(dataName);
                _typeWritter.StartTypeWritter(
                    DataHandlerService.ItemDataNameGetLanguagePackUseText(dataName), _textUI);
                var item = DataHandlerService.GetItemFormDataName(dataName);
                item.OnUse(globalItemIndex + 1);

                SpriteChange();
                _itemScroller.Close();

                _textUIBack.rectTransform.anchoredPosition = new Vector2(10.75f, -3.3f);
                _textUIBack.alignment = TextAlignmentOptions.TopLeft;
                _textUIBack.text = "";

                UITextUpdate(UITextMode.Food);
                return;
            }

            var myItemMax = ListManipulationService.FindFirstNullOrEmptyIndex(
                MainControl.Instance.playerControl.items);

            var (itemLine0, itemDataText0) =
                GenerateItemDisplayText(globalItemIndex - visibleItemIndex);

            var (itemLine1, itemDataText1) = myItemMax > 1
                ? GenerateItemDisplayText(globalItemIndex - visibleItemIndex + 1)
                : ("", "");

            var (itemLine2, itemDataText2) = myItemMax > 2
                ? GenerateItemDisplayText(globalItemIndex - visibleItemIndex + 2)
                : ("", "");

            MainControl.Instance.battlePlayerController.transform.position = new Vector3(-5.175f,
                -0.96f - visibleItemIndex * 0.66f,
                MainControl.Instance.battlePlayerController.transform.position.z);

            _textUI.text = itemLine0 + itemLine1 + itemLine2;

            _textUIBack.text = itemDataText0 + "\n" + itemDataText1 + "\n" + itemDataText2;

            var updateHandleItemInput =
                _itemScroller.UpdateHandleItemInput(globalItemIndex, visibleItemIndex, myItemMax,
                    CommonItemNavigationLogic);
            globalItemIndex = updateHandleItemInput.globalItemIndex;
            visibleItemIndex = updateHandleItemInput.visibleItemIndex;

            _hpUI.text = FormatWithLeadingZero(_hpFood) + " / " +
                         FormatWithLeadingZero(MainControl.Instance.playerControl.hpMax);
        }


        private void CommonItemNavigationLogic(int globalItemIndex)
        {
            AudioController.Instance.PlayFx(0, MainControl.Instance.AudioControl.fxClipUI);
            var dataName = MainControl.Instance.playerControl.items[globalItemIndex];
            var item = DataHandlerService.GetItemFormDataName(dataName);
            if (item is FoodItem or ParentFoodItem)
            {
                UITextUpdate(UITextMode.Food, item.Data.Value);
            }
            else
            {
                UITextUpdate(UITextMode.Food);
            }
        }

        private void OpenDialogBubble(string textAsset)
        {
            optionsSave = DataHandlerService.LoadItemData(textAsset);
            optionsSave = DataHandlerService.ChangeItemData(optionsSave, true, new List<string>());
            isDialog = true;
            numberDialog = 0;
        }

        private void KeepDialogBubble()
        {
            //TODO: 对话应当有多种情况，如固定对话或者随机对话
            //TODO: 针对多战斗写对话
            //TODO: 死亡怪物应不会说话
            var save = new List<string>();
            TextProcessingService.SplitStringToListWithDelimiter(optionsSave[numberDialog], save);

            var size = save[0];
            var position = save[1];
            var character = save[2];
            var direction = save[3];
            var arrowY = save[4];
            var text = save[5];

            var enemyController = enemiesControllers.FirstOrDefault(t => character == t.name);
            if (enemyController)
            {
                _dialog.transform.SetParent(enemyController.transform);
            }
            else
            {
                Debug.LogError("enemyController is empty!");
            }

            _dialog.size = TextProcessingService.StringVector2ToRealVector2(size, _dialog.size);
            _dialog.position = TextProcessingService.StringVector2ToRealVector2(position, _dialog.position);

            if (TryParseDirection(direction, out var resultDirection))
            {
                _dialog.isBackRight = resultDirection;
            }

            if (!float.TryParse(arrowY, out _dialog.backY))
            {
                Debug.LogError($"Failed to parse arrowY: {arrowY}");
            }

            _dialog.typeWritter.StartTypeWritter(text, _dialog.tmp);
            numberDialog++;
            _dialog.tmp.text = "";
            _dialog.PositionChange();
        }

        private static bool TryParseDirection(string direction, out bool result)
        {
            if (string.Equals(direction, "right", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(direction, "true", StringComparison.OrdinalIgnoreCase))
            {
                result = true;
                return true;
            }

            if (string.Equals(direction, "left", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(direction, "false", StringComparison.OrdinalIgnoreCase))
            {
                result = false;
                return true;
            }

            Debug.LogError($"Invalid direction value: {direction}");
            result = false;
            return false;
        }

        private void TurnTextLoad(bool useTurn = false, int turn = 0)
        {
            if (TurnController.Instance.turn != _saveTurn || _saveTurnText == "")
            {
                List<string> load;
                _saveTurn = TurnController.Instance.turn;
                if (useTurn)
                {
                    load = TurnTextLoad(MainControl.Instance.BattleControl.turnTextSave, turn);
                    firstIn = false;
                }
                else
                {
                    load = TurnTextLoad(MainControl.Instance.BattleControl.turnTextSave, _saveTurn);
                }

                _saveTurnText = load != null && load.Count != 0
                    ? load[Random.Range(0, load.Count)]
                    : "* No Language Pack.";
            }

            StartTypeWritter(_saveTurnText);
        }

        private static List<string> TurnTextLoad(List<string> turnTextSave, int turn)
        {
            var turnTextSaveChanged =
                (from t in turnTextSave
                    where t[..turn.ToString().Length] == turn.ToString()
                    select t[(turn.ToString().Length + 1)..]).ToList();
            var saves = new List<string>();
            TextProcessingService.SplitStringToListWithDelimiter(turnTextSaveChanged, saves);
            return saves;
        }

        /// <summary>
        ///     更新UI文字与血条
        /// </summary>
        public void UITextUpdate(UITextMode uiTextMode = 0, int foodValue = 0)
        {
            _hpSpr.transform.localScale = new Vector3(0.525f * MainControl.Instance.playerControl.hpMax, 8.5f);
            _hpSpr.material.SetColor(ColorUnder, hpColorUnder);
            _hpSpr.material.SetColor(ColorOn, hpColorOn);

            switch (uiTextMode)
            {
                case UITextMode.Hit:
                {
                    _hpSpr.material.DOKill();

                    _hpSpr.material.SetFloat(IsFlashing, 0);
                    _hpSpr.material.SetColor(ColorFlash, hpColorHit);
                    _hpSpr.material.SetFloat(Crop,
                        (float)MainControl.Instance.playerControl.hp / MainControl.Instance.playerControl.hpMax);
                    _hpSpr.material
                        .DOFloat(
                            (float)MainControl.Instance.playerControl.hp / MainControl.Instance.playerControl.hpMax,
                            "_Flash", 0.5f).SetEase(Ease.OutCirc);

                    break;
                }

                case UITextMode.Food:
                {
                    _hpSpr.material.DOKill();

                    _hpSpr.material.SetFloat(IsFlashing, 1);
                    _hpSpr.material.SetFloat(Crop,
                        (float)MainControl.Instance.playerControl.hp / MainControl.Instance.playerControl.hpMax);
                    float addNumber = MainControl.Instance.playerControl.hp + foodValue;
                    if (addNumber > MainControl.Instance.playerControl.hpMax)
                    {
                        addNumber = MainControl.Instance.playerControl.hpMax;
                    }

                    _hpSpr.material.DOFloat(addNumber / MainControl.Instance.playerControl.hpMax, "_Flash", 0.5f)
                        .SetEase(Ease.OutCirc);
                    break;
                }

                case UITextMode.None:
                default:
                {
                    _hpSpr.material.SetFloat(Crop,
                        (float)MainControl.Instance.playerControl.hp / MainControl.Instance.playerControl.hpMax);
                    _hpSpr.material.SetFloat(Flash,
                        (float)MainControl.Instance.playerControl.hp / MainControl.Instance.playerControl.hpMax);
                    break;
                }
            }

            _hpUI.transform.localPosition =
                new Vector3(9.85f + 0.0265f * (MainControl.Instance.playerControl.hpMax - 20), -5.825f);
            NameUIUpdate();

            if (uiTextMode != UITextMode.Food)
            {
                _hpUI.text = FormatWithLeadingZero(MainControl.Instance.playerControl.hp) + " / " +
                             FormatWithLeadingZero(MainControl.Instance.playerControl.hpMax);
            }
            else
            {
                _hpFoodTween.Kill();
                var addNumber = MainControl.Instance.playerControl.hp + foodValue;
                if (addNumber > MainControl.Instance.playerControl.hpMax)
                {
                    addNumber = MainControl.Instance.playerControl.hpMax;
                }

                _hpFoodTween = DOTween.To(() => _hpFood, x => _hpFood = x, addNumber, 0.5f);
            }
        }

        private void NameUIUpdate()
        {
            _nameUI.text = MainControl.Instance.playerControl.playerName +
                           " lv<indent=29.5>" +
                           MainControl.Instance.playerControl.lv;
        }

        /// <summary>
        ///     将数字格式化为两位数（前导零）显示，例如将 1 显示为 01。
        /// </summary>
        private static string FormatWithLeadingZero(int i)
        {
            return i.ToString("D2");
        }


        private static (string text, string data) GenerateItemDisplayText(int layerIndex)
        {
            var dataName = MainControl.Instance.playerControl.items[layerIndex];

            var text = UITextPrefix +
                       DataHandlerService.ItemDataNameGetLanguagePackName(dataName) + "\n";

            var item = DataHandlerService.GetItemFormDataName(dataName);
            var data = GenerateItemStatText(item);

            return (text, data);
        }

        private static string GenerateItemStatText(GameItem item)
        {
            var result = item switch
            {
                FoodItem or ParentFoodItem => "HP " + (item.Data.Value > 0 ? "+" : ""),
                WeaponItem => "ATK ",
                ArmorItem => "DEF ",
                _ => "Value "
            };
            return result + item.Data.Value;
        }
    }
}
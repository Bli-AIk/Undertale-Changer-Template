using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DG.Tweening;
using Plugins.Timer.Source;
using UCT.Audio;
using UCT.Battle.BattleConfigs;
using UCT.Core;
using UCT.Overworld;
using UCT.Overworld.FiniteStateMachine;
using UCT.Service;
using UCT.Settings;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace UCT.EventSystem
{
    /// <summary>
    ///     事件系统控制器，包含所有可以调用的函数。
    /// </summary>
    public class EventController : MonoBehaviour
    {
        private const string Global = "Global";
        private const string Scene = "Scene";
        private const string Position = "Position";
        private const string Special = "Special";
        private const string Audio = "Audio";


        private static TypeWritter _overworldTypeWritter;
        private static readonly int Open = Animator.StringToHash("Open");

        public static Dictionary<MethodNameData, IMethodWrapper> MethodDictionary { get; } = new()
        {
            {
                new MethodNameData("string:StartOverworldTypeWritter", Global),
                new MethodWrapper(args =>
                    StartOverworldTypeWritter((string)args[0], (bool)args[1], (string)args[2]))
            },
            {
                new MethodNameData("null:OpenSaveBox", Global),
                new MethodWrapper(args =>
                    OpenSaveBox((bool)args[0], (string)args[1]))
            },
            {
                new MethodNameData("bool:MainCameraIsFollow", Global),
                new MethodWrapper(args =>
                    MainCameraIsFollow((bool)args[0], (bool)args[1], (string)args[2]))
            },
            {
                new MethodNameData("bool:PlayerCanMove", Global),
                new MethodWrapper(args =>
                    PlayerCanMove((bool)args[0], (bool)args[1], (string)args[2]))
            },
            {
                new MethodNameData("IBattleConfig:EnterBattleScene", Global),
                new MethodWrapper(args => EnterBattleScene(GetIBattleConfig((string)args[0]), (bool)args[1], (string)args[2]))
            },
            {
                new MethodNameData("float:WaitingForSecond", Global),
                new MethodWrapper(args =>
                    WaitingForSecond((float)args[0], (bool)args[1], (string)args[2]))
            },
            {
                new MethodNameData("float:LockPlayerForSecond", Global),
                new MethodWrapper(args =>
                    LockPlayerForSecond((float)args[0], (bool)args[1], (string)args[2]))
            },
            {
                new MethodNameData("scene:SwitchOverworldScene", Scene),
                new MethodWrapper(args =>
                    SwitchOverworldScene((string)args[0], (bool)args[1], (Vector2)args[2], (bool)args[3],
                        (string)args[4]))
            },
            {
                new MethodNameData("Vector2:TeleportPlayer", Position),
                new MethodWrapper(args =>
                    TeleportPlayer((Vector2)args[0], (bool)args[1], (string)args[2]))
            },
            {
                new MethodNameData("Vector2Ease:MoveMainCamera", Position),
                new MethodWrapper(args =>
                    MoveMainCamera((Vector2)args[0], (float)args[1], (Ease)args[2], (bool)args[3], (string)args[4]))
            },
            {
                new MethodNameData("Vector2Ease:MoveMainCameraRelativeToPlayer", Position),
                new MethodWrapper(args =>
                    MoveMainCameraRelativeToPlayer((Vector2)args[0], (float)args[1], (Ease)args[2], (bool)args[3],
                        (string)args[4]))
            },
            {
                new MethodNameData("float:MakePlayerTranslucent", Position),
                new MethodWrapper(args =>
                    MakePlayerTranslucent((float)args[0], (bool)args[1], (string)args[2]))
            },
            {
                new MethodNameData("Vector2Ease:MovePlayerRelative", Position),
                new MethodWrapper(args =>
                    MovePlayerRelative((Vector2)args[0], (float)args[1], (Ease)args[2], (bool)args[3], (string)args[4]))
            },
            {
                new MethodNameData("float:SwingMainCamera", Position),
                new MethodWrapper(args =>
                    SwingMainCamera((float)args[0], (bool)args[1], (string)args[2]))
            },
            {
                new MethodNameData("float:MakePlayerSpin", Special),
                new MethodWrapper(args =>
                    MakePlayerSpin((float)args[0], (bool)args[1], (string)args[2]))
            },
            {
                new MethodNameData("null:GetPooledObjectAtPlayer", Special),
                new MethodWrapper(args =>
                    GetPooledObjectAtPlayer((bool)args[0], (string)args[1]))
            },
            {
                new MethodNameData("float:BannedBGM", Audio),
                new MethodWrapper(args =>
                    BannedBGM((float)args[0], (bool)args[1], (string)args[2]))
            },
            {
                new MethodNameData("int:PlayUIFx", Audio),
                new MethodWrapper(args =>
                    PlayUIFx((int)args[0], (bool)args[1], (string)args[2]))
            }
        };

        public static EventTable eventTable { get; private set; }
        public static FactTable factTable { get; private set; }
        private static RuleTable ruleTable { get; set; }
        public static EventTable globalEventTable { get; private set; }
        public static FactTable globalFactTable { get; private set; }
        private static RuleTable globalRuleTable { get; set; }

        private void Start()
        {
            LoadTables();
        }

        private void Update()
        {
            if (MainControl.Instance.sceneState != MainControl.SceneState.Overworld)
            {
                return;
            }

            UpdateEvent(globalEventTable);
            UpdateEvent(eventTable);
        }

        private static IBattleConfig GetIBattleConfig(string className)
        {
            var assembly = Assembly.Load("Assembly-CSharp");
            var type = assembly.GetTypes()
                .FirstOrDefault(t => t.Name == className && typeof(IBattleConfig).IsAssignableFrom(t));

            if (type == null)
            {
                throw new ArgumentNullException($"No class inherited from IBattleConfig found: {className}");
            }

            return Activator.CreateInstance(type) as IBattleConfig;
        }

        public static bool LoadTables(bool force = false)
        {
            globalEventTable = Resources.Load<EventTable>("Tables/EventTable");
            globalFactTable = Resources.Load<FactTable>("Tables/FactTable");
            globalRuleTable = Resources.Load<RuleTable>("Tables/RuleTable");
            if (!force && MainControl.Instance.sceneState != MainControl.SceneState.Overworld)
            {
                eventTable = globalEventTable;
                factTable = globalFactTable;
                ruleTable = globalRuleTable;
                return false;
            }

            var sceneName = SceneManager.GetActiveScene().name;
            eventTable = Resources.Load<EventTable>($"Tables/{sceneName}/EventTable");
            factTable = Resources.Load<FactTable>($"Tables/{sceneName}/FactTable");
            ruleTable = Resources.Load<RuleTable>($"Tables/{sceneName}/RuleTable");
            return true;
        }

        private static void UpdateEvent(EventTable inputEventTable)
        {
            for (var i = 0; i < inputEventTable.events.Count; i++)
            {
                var eventEntry = inputEventTable.events[i];
                if (eventEntry.isTriggering)
                {
                    eventEntry = Detection(eventEntry);
                }

                inputEventTable.events[i] = eventEntry;
            }
        }

        private static EventEntry Detection(EventEntry eventEntry)
        {
            var rules = ruleTable.rules.Concat(globalRuleTable.rules).ToList();

            for (var j = 0; j < rules.Count; j++)
            {
                var rule = rules[j];

                eventEntry = DetectionRule(eventEntry, rule, out var isTriggered);

                rules[j] = rule;

                if (isTriggered)
                {
                    break;
                }
            }

            return eventEntry;
        }

        public static EventEntry DetectionRule(EventEntry eventEntry,
            RuleEntry rule,
            out bool isTriggered,
            bool force = false)
        {
            isTriggered = false;
            var triggeredByCount = rule.triggeredBy.Count;

            if (force)
            {
                triggeredByCount = 1;
            }

            for (var triggeredByIndex = 0; triggeredByIndex < triggeredByCount; triggeredByIndex++)
            {
                if (!rule.ruleCriterion.GetResult() && rule.useRuleCriterion)
                {
                    continue;
                }

                if (!force && eventEntry.name != rule.triggeredBy[triggeredByIndex])
                {
                    continue;
                }

                eventEntry.isTriggering = false;
                isTriggered = true;

                foreach (var newTrigger in rule.triggers)
                {
                    SetTriggering(newTrigger);
                }

                InvokeRuleMethod(rule);
                SetFact(rule);

                break;
            }

            return eventEntry;
        }

        private static void SetFact(RuleEntry rule)
        {
            for (var index = 0; index < rule.factModifications.Count; index++)
            {
                var item = rule.factModifications[index];
                var number = item.fact.value;
                var newNum = item.number;
                switch (item.operation)
                {
                    case FactModification.Operation.Change:
                        number = newNum;
                        break;
                    case FactModification.Operation.Add:
                        number += newNum;
                        break;
                    case FactModification.Operation.Subtract:
                        number -= newNum;
                        break;
                    case FactModification.Operation.Multiply:
                        number *= newNum;
                        break;
                    case FactModification.Operation.Divide:
                        if (newNum != 0)
                        {
                            number /= newNum;
                        }
                        else
                        {
                            Debug.LogError("不可除以0.");
                        }

                        break;
                    default:
                        throw new ArgumentOutOfRangeException($"Unexpected operation value: {item.operation}");
                }

                item.fact.value = number;
                rule.factModifications[index] = item;
                var isGlobalFactModification = rule.isGlobalFactModifications[index];

                if (!isGlobalFactModification)
                {
                    factTable = SetFactEntry(factTable, item, number);
                }
                else
                {
                    globalFactTable = SetFactEntry(globalFactTable, item, number);
                }
            }
        }

        private static void InvokeRuleMethod(RuleEntry rule)
        {
            for (var k = 0; k < rule.methodNames.Count; k++)
            {
                if (rule.firstStringParams.Count <= k)
                {
                    rule.firstStringParams.AddRange(Enumerable.Repeat<string>(null,
                        k + 1 - rule.firstStringParams.Count));
                }

                if (rule.secondStringParams.Count <= k)
                {
                    rule.secondStringParams.AddRange(Enumerable.Repeat<string>(null,
                        k + 1 - rule.secondStringParams.Count));
                }

                if (rule.thirdStringParams.Count <= k)
                {
                    rule.thirdStringParams.AddRange(Enumerable.Repeat<string>(null,
                        k + 1 - rule.thirdStringParams.Count));
                }

                if (rule.useMethodEvents.Count <= k)
                {
                    rule.useMethodEvents.AddRange(Enumerable.Repeat(false, k + 1 - rule.useMethodEvents.Count));
                }

                if (rule.methodEvents.Count <= k)
                {
                    rule.methodEvents.AddRange(Enumerable.Repeat<string>(null, k + 1 - rule.methodEvents.Count));
                }  
                
                if (rule.objectParams.Count <= k)
                {
                    rule.objectParams.AddRange(Enumerable.Repeat<Object>(null, k + 1 - rule.objectParams.Count));
                }
                
                InvokeMethodByName(rule.methodNames[k], rule.firstStringParams[k], rule.secondStringParams[k],
                    rule.thirdStringParams[k], rule.useMethodEvents[k], rule.methodEvents[k], rule.objectParams[k]);
            }
        }

        private static FactTable SetFactEntry(FactTable inputFactTable, FactModification item, int number)
        {
            for (var l = 0; l < inputFactTable.facts.Count; l++)
            {
                var fact = inputFactTable.facts[l];
                if (fact.name == item.fact.name)
                {
                    fact.value = number;
                }

                inputFactTable.facts[l] = fact;
            }

            return inputFactTable;
        }

        private static void SetTriggering(string newTrigger, bool value = true)
        {
            UpdateEventTrigger(eventTable, newTrigger, value);
            UpdateEventTrigger(globalEventTable, newTrigger, value);
        }

        private static void UpdateEventTrigger(EventTable table, string triggerName, bool value)
        {
            for (var index = 0; index < table.events.Count; index++)
            {
                if (table.events[index].name != triggerName)
                {
                    continue;
                }

                var eventItem = table.events[index];
                eventItem.isTriggering = value;
                table.events[index] = eventItem;
            }
        }

        /// <summary>
        ///     在Overworld场景中启动对话打字机。
        /// </summary>
        /// <param name="dataName">对话数据名称</param>
        /// <param name="useEvent">是否联动event</param>
        /// <param name="eventName">联动的event名称</param>
        private static void StartOverworldTypeWritter(string dataName, bool useEvent, string eventName)
        {
            MainControl.Instance.playerControl.canMove = false;
            SettingsStorage.Pause = true;
            TalkBoxController.Instance.SetHead(false);
            if (TalkBoxController.Instance.boxDrawer.localPosition.z < 0)
            {
                TalkBoxController.Instance.boxDrawer.localPosition = new Vector3(
                    TalkBoxController.Instance.boxDrawer.localPosition.x,
                    TalkBoxController.Instance.boxDrawer.localPosition.y,
                    BackpackBehaviour.BoxZAxisVisible);
            }

            var mainCamera = Camera.main;
            if (mainCamera)
            {
                TalkBoxController.Instance.isUp =
                    MainControl.OverworldPlayerBehaviour.transform.position.y <
                    mainCamera.transform.position.y - 1.25f;
            }

            if (!_overworldTypeWritter)
            {
                _overworldTypeWritter = BackpackBehaviour.Instance.typeWritter;
            }

            _overworldTypeWritter.overworldSpriteChanger.spriteExpressionCollection = null;
            _overworldTypeWritter.overworldSpriteChanger.UpdateSpriteDisplay();

            _overworldTypeWritter.StartTypeWritter(
                TextProcessingService.GetFirstChildStringByPrefix(
                    MainControl.Instance.LanguagePackControl.sceneTexts,
                    dataName), BackpackBehaviour.Instance.talkText);

            _overworldTypeWritter.OnClose = () =>
            {
                BackpackBehaviour.Instance.talkText.text = "";
                TalkBoxController.Instance.boxDrawer.localPosition = new Vector3(
                    TalkBoxController.Instance.boxDrawer.localPosition.x,
                    TalkBoxController.Instance.boxDrawer.localPosition.y, BackpackBehaviour.BoxZAxisInvisible);
                Timer.Register(0.1f, () =>
                {
                    MainControl.Instance.playerControl.canMove = true;
                    SettingsStorage.Pause = false;
                    _overworldTypeWritter.OnClose = null;
                    if (useEvent)
                    {
                        SetTriggering(eventName);
                    }
                });
            };
        }

        /// <summary>
        ///     在保持BGM的同时切换场景
        /// </summary>
        private static void SwitchOverworldScene(string sceneName,
            bool isBgmMuted,
            Vector2 newPos,
            bool useEvent,
            string eventName)
        {
            Action action = null;
            action += () => TeleportPlayer(newPos, false, "");
            if (useEvent)
            {
                action += () => SetTriggering(eventName);
            }

            GameUtilityService.FadeOutAndSwitchScene(sceneName, Color.black, action, isBgmMuted);
        }

        /// <summary>
        ///     传送玩家
        /// </summary>
        private static void TeleportPlayer(Vector2 newPosition, bool useEvent, string eventName)
        {
            if (!MainControl.OverworldPlayerBehaviour)
            {
                return;
            }

            MainControl.OverworldPlayerBehaviour.transform.position = newPosition;
            MainControl.Instance.playerControl.playerLastPos = newPosition;

            if (useEvent)
            {
                SetTriggering(eventName);
            }
        }

        private static void OpenSaveBox(bool useEvent, string eventName)
        {
            SaveBoxController.Instance.OpenSaveBox();

            if (useEvent)
            {
                SetTriggering(eventName);
            }
        }

        private static void MoveMainCamera(Vector2 newPosition,
            float duration,
            Ease ease,
            bool useEvent,
            string eventName)
        {
            if (duration < 0)
            {
                UnityEngine.Debug.LogError($"{duration} 应当大于0.");
            }

            var mainCamera = CameraFollowPlayer.Instance;
            mainCamera.isFollow = false;
            if (mainCamera)
            {
                mainCamera.transform
                    .DOMove(new Vector3(newPosition.x, newPosition.y, mainCamera.transform.position.z), duration)
                    .SetEase(ease)
                    .OnKill(() =>
                    {
                        if (useEvent)
                        {
                            SetTriggering(eventName);
                        }
                    });
            }
        }

        private static void MoveMainCameraRelativeToPlayer(Vector2 newPosition,
            float duration,
            Ease ease,
            bool useEvent,
            string eventName)
        {
            var newPos = MainControl.OverworldPlayerBehaviour.transform.position + (Vector3)newPosition;
            if (CameraFollowPlayer.Instance.isLimit)
            {
                newPos = CameraFollowPlayer.Instance.GetLimitedPosition(newPos);
            }

            MoveMainCamera(newPos, duration, ease, useEvent, eventName);
        }

        private static void MainCameraIsFollow(bool isFollow, bool useEvent, string eventName)
        {
            CameraFollowPlayer.Instance.isFollow = isFollow;
            if (useEvent)
            {
                SetTriggering(eventName);
            }
        }

        private static void PlayerCanMove(bool canMove, bool useEvent, string eventName)
        {
            MainControl.Instance.playerControl.canMove = canMove;
            if (useEvent)
            {
                SetTriggering(eventName);
            }
        }

        //TODO:多战斗补全
        private static void EnterBattleScene(IBattleConfig battleConfig, bool useEvent, string eventName)
        {
            MainControl.Instance.BattleControl.BattleConfig = battleConfig;
            SettingsController.Instance.Animator.SetBool(Open, true);
            if (useEvent)
            {
                SetTriggering(eventName);
            }
        }

        private static void MakePlayerTranslucent(float duration, bool useEvent, string eventName)
        {
            MainControl.OverworldPlayerBehaviour.spriteRenderer.color = ColorEx.HalfAlpha;

            Timer.Register(duration, () =>
            {
                MainControl.OverworldPlayerBehaviour.spriteRenderer.color = Color.white;
                if (useEvent)
                {
                    SetTriggering(eventName);
                }
            });
        }

        private static void MakePlayerSpin(float duration, bool useEvent, string eventName)
        {
            MainControl.Instance.playerControl.canMove = false;
            SettingsStorage.Pause = true;
            MainControl.OverworldPlayerBehaviour.TransitionToStateIfNeeded(StateType.Spin);


            Timer.Register(duration, () =>
            {
                MainControl.Instance.playerControl.canMove = true;
                SettingsStorage.Pause = false;
                MainControl.OverworldPlayerBehaviour.TransitionToStateIfNeeded(StateType.Idle);
                if (useEvent)
                {
                    SetTriggering(eventName);
                }
            });
        }

        private static void MovePlayerRelative(Vector2 newPosition,
            float duration,
            Ease ease,
            bool useEvent,
            string eventName)
        {
            MovePlayerAbsolute(
                MainControl.OverworldPlayerBehaviour.transform.position + (Vector3)newPosition, duration,
                ease, useEvent, eventName);
        }

        private static void MovePlayerAbsolute(Vector2 newPosition,
            float duration,
            Ease ease,
            bool useEvent,
            string eventName)
        {
            if (duration <= 0)
            {
                MainControl.OverworldPlayerBehaviour.transform.position = newPosition;
                return;
            }

            MainControl.Instance.playerControl.canMove = false;

            var colliders = MainControl.OverworldPlayerBehaviour.GetComponentsInChildren<Collider2D>();
            foreach (var collider in colliders)
            {
                collider.enabled = false;
            }

            MainControl.OverworldPlayerBehaviour.transform
                .DOMove(newPosition, duration)
                .SetEase(ease)
                .OnKill(() =>
                {
                    foreach (var collider in colliders)
                    {
                        collider.enabled = true;
                    }

                    MainControl.Instance.playerControl.canMove = true;
                    if (useEvent)
                    {
                        SetTriggering(eventName);
                    }
                });
        }

        private static void GetPooledObjectAtPlayer(bool useEvent, string eventName)
        {
            var obj = GameObject.Find("ObjectPool").GetComponent<ObjectPool>().GetFromPool<Transform>();
            obj.transform.position = MainControl.OverworldPlayerBehaviour.transform.position;
            if (useEvent)
            {
                SetTriggering(eventName);
            }
        }

        private static void WaitingForSecond(float duration, bool useEvent, string eventName)
        {
            Timer.Register(duration, () =>
            {
                if (useEvent)
                {
                    SetTriggering(eventName);
                }
            });
        }

        private static void LockPlayerForSecond(float duration, bool useEvent, string eventName)
        {
            MainControl.Instance.playerControl.canMove = false;
            Timer.Register(duration, () =>
            {
                MainControl.Instance.playerControl.canMove = true;
                if (useEvent)
                {
                    SetTriggering(eventName);
                }
            });
        }

        private static void PlayUIFx(int index, bool useEvent, string eventName)
        {
            AudioController.Instance.PlayFx(index, MainControl.Instance.AudioControl.fxClipUI);
            if (useEvent)
            {
                SetTriggering(eventName);
            }
        }

        private static void BannedBGM(float duration, bool useEvent, string eventName)
        {
            DOTween.To(() => AudioController.Instance.audioSource.volume,
                    x => AudioController.Instance.audioSource.volume = x, 0, duration)
                .SetEase(Ease.Linear)
                .OnKill(() =>
                {
                    if (useEvent)
                    {
                        SetTriggering(eventName);
                    }
                });
        }

        private static void SwingMainCamera(float duration, bool useEvent, string eventName)
        {
            const float dir = 0.25f;
            CameraFollowPlayer.Instance.transform
                .DOMove(CameraFollowPlayer.Instance.transform.position - new Vector3(dir, 0, 0), duration / 8)
                .SetEase(Ease.Linear)
                .OnKill(() => CameraFollowPlayer.Instance.transform
                    .DOMove(CameraFollowPlayer.Instance.transform.position + new Vector3(dir * 2, 0, 0), duration / 4)
                    .SetEase(Ease.Linear).SetLoops(3, LoopType.Yoyo)
                    .OnKill(() => CameraFollowPlayer.Instance.transform
                        .DOMove(CameraFollowPlayer.Instance.transform.position - new Vector3(dir, 0, 0), duration / 8)
                        .SetEase(Ease.Linear)
                        .OnKill(() =>
                        {
                            if (useEvent)
                            {
                                SetTriggering(eventName);
                            }
                        })
                    )
                );
        }

        // TODO:把下面未完成的函数实现。
        //  带△的项是不优先考虑的项
        //  △中断/停止对话
        //  SFM物体移动到指定位置
        //  SFM物体切换行走/奔跑
        //  SFM物体切换特定动画
        //  玩家掉血
        //  玩家回血
        //  从玩家背包添加物品 △在物品系统重做后添加
        //  从玩家背包移除物品 △在物品系统重做后添加
        //  播放特定音效
        //  播放或暂停背景音乐
        //  播放背景音乐
        //  暂停背景音乐
        //  切换背景音乐
        //  移动摄像机
        //  △修改SFM物体的data
        //  △创建对象
        //  △触发特殊视觉效果


        /// <summary>
        ///     调用_methodDictionary字典中注册的方法
        /// </summary>
        /// <param name="methodName">方法名</param>
        /// <param name="parameter1">（可能的）传递的第一个参数</param>
        /// <param name="parameter2">（可能的）传递的第二个参数</param>
        /// <param name="parameter3">（可能的）传递的第三个参数</param>
        /// <param name="useEvent">是否联动event</param>
        /// <param name="eventName">联动的event名称</param>
        /// <param name="unityObj">（可能的）传递的Obj参数</param>
        private static void InvokeMethodByName(string methodName,
            string parameter1,
            string parameter2,
            string parameter3,
            bool useEvent,
            string eventName,
            Object unityObj)

        {
            var tag = methodName[..methodName.IndexOf(':')];
            var method = MethodDictionary.FirstOrDefault(kv => kv.Key.MethodName == methodName).Value;

            if (method != null)
            {
                switch (tag)
                {
                    case "Vector2Ease":
                        method.Invoke(TextProcessingService.StringVector2ToRealVector2(parameter1),
                            float.Parse(parameter2),
                            (Ease)int.Parse(parameter3), useEvent, eventName);
                        break;

                    case "bool":
                        method.Invoke(bool.Parse(parameter1), useEvent, eventName);
                        break;

                    case "Vector2":
                        method.Invoke(TextProcessingService.StringVector2ToRealVector2(parameter1), useEvent,
                            eventName);
                        break;

                    case "int":
                        method.Invoke(int.Parse(parameter1), useEvent, eventName);
                        break;

                    case "float":
                        method.Invoke(float.Parse(parameter1), useEvent, eventName);
                        break;

                    case "null":
                        method.Invoke(useEvent, eventName);
                        break;

                    case "scene":
                        method.Invoke(parameter1, bool.Parse(parameter2),
                            TextProcessingService.StringVector2ToRealVector2(parameter3), useEvent, eventName);
                        break;

                    default:
                        method.Invoke(parameter1, useEvent, eventName);
                        break;
                }
            }
            else
            {
                Console.WriteLine($"方法 {methodName} 未找到！");
            }
        }
    }

    [Serializable]
    public struct MethodNameData : IEquatable<MethodNameData>
    {
        public MethodNameData(string methodName, string methodType)
        {
            MethodName = methodName;
            MethodType = methodType;
        }

        public string MethodType { get; }
        public string MethodName { get; }

        public bool Equals(MethodNameData other)
        {
            return MethodType == other.MethodType && MethodName == other.MethodName;
        }

        public override bool Equals(object obj)
        {
            return obj is MethodNameData other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(MethodType, MethodName);
        }
    }
}
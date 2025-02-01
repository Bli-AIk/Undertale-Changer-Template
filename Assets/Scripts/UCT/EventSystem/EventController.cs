using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UCT.Global.Core;
using UCT.Global.Settings;
using UCT.Global.UI;
using UCT.Overworld;
using UCT.Service;
using UnityEngine;

namespace UCT.EventSystem
{
    /// <summary>
    ///     事件系统控制器，包含所有可以调用的函数。
    /// </summary>
    public class EventController : MonoBehaviour
    {
        public static readonly Dictionary<string, IMethodWrapper> MethodDictionary = new()
        {
            {
                "string:StartOverworldTypeWritter",
                new MethodWrapper(args =>
                    StartOverworldTypeWritter((string)args[0], (bool)args[1], (string)args[2]))
            },
            {
                "string:SwitchSceneWithBgm",
                new MethodWrapper(args =>
                    SwitchSceneWithBgm((string)args[0], (bool)args[1], (string)args[2]))
            },
            {
                "string:SwitchSceneWithoutBgm",
                new MethodWrapper(args =>
                    SwitchSceneWithoutBgm((string)args[0], (bool)args[1], (string)args[2]))
            },
            {
                "Vector2:TeleportPlayer",
                new MethodWrapper(args =>
                    TeleportPlayer((Vector2)args[0], (bool)args[1], (string)args[2]))
            },
            {
                "null:OpenSaveBox",
                new MethodWrapper(args =>
                    OpenSaveBox((bool)args[0], (string)args[1]))
            },
            {
                "Vector2Ease:MoveMainCamera",
                new MethodWrapper(args =>
                    MoveMainCamera((Vector2)args[0], (float)args[1], (Ease)args[2], (bool)args[3], (string)args[4]))
            },
            {
            "null:EnterBattleScene",
            new MethodWrapper(args =>
                EnterBattleScene((bool)args[0], (string)args[1]))
        },
        };


        private static TypeWritter _overworldTypeWritter;
        public static EventTable eventTable;
        public static FactTable factTable;
        public static RuleTable ruleTable;
        private static readonly int Open = Animator.StringToHash("Open");

        private void Awake()
        {
            eventTable = Resources.Load<EventTable>("Tables/EventTable");
            factTable = Resources.Load<FactTable>("Tables/FactTable");
            ruleTable = Resources.Load<RuleTable>("Tables/RuleTable");
        }

        private void Update()
        {
            for (var i = 0; i < eventTable.events.Count; i++)
            {
                var eventEntry = eventTable.events[i];
                if (eventEntry.isTriggering)
                    eventEntry = Detection(eventEntry);
                eventTable.events[i] = eventEntry;
            }
        }

        private static EventEntry Detection(EventEntry eventEntry)
        {
            // ReSharper disable once ForCanBeConvertedToForeach
            for (var j = 0; j < ruleTable.rules.Count; j++)
            {
                var rule = ruleTable.rules[j];

                eventEntry = DetectionRule(eventEntry, rule);
                ruleTable.rules[j] = rule;
            }

            return eventEntry;
        }

        public static EventEntry DetectionRule(EventEntry eventEntry, RuleEntry rule, bool force = false)
        {
            var triggeredByCount = rule.triggeredBy.Count;

            if (force) triggeredByCount = 1;

            for (var triggeredByIndex = 0; triggeredByIndex < triggeredByCount; triggeredByIndex++)
            {
                if (!rule.ruleCriterion.GetResult() && rule.useRuleCriterion) continue;

                if (!force && eventEntry.name != rule.triggeredBy[triggeredByIndex]) continue;


                eventEntry.isTriggering = false;

                foreach (var newTrigger in rule.triggers) SetTriggering(newTrigger);

                for (var k = 0; k < rule.methodNames.Count; k++)
                {
                    if (rule.firstStringParams.Count <= k)
                        rule.firstStringParams.AddRange(Enumerable.Repeat<string>(null,
                            k + 1 - rule.firstStringParams.Count));

                    if (rule.secondStringParams.Count <= k)
                        rule.secondStringParams.AddRange(Enumerable.Repeat<string>(null,
                            k + 1 - rule.secondStringParams.Count));

                    if (rule.thirdStringParams.Count <= k)
                        rule.thirdStringParams.AddRange(Enumerable.Repeat<string>(null,
                            k + 1 - rule.thirdStringParams.Count));

                    if (rule.useMethodEvents.Count <= k)
                        rule.useMethodEvents.AddRange(Enumerable.Repeat(false, k + 1 - rule.useMethodEvents.Count));

                    if (rule.methodEvents.Count <= k)
                        rule.methodEvents.AddRange(Enumerable.Repeat<string>(null, k + 1 - rule.methodEvents.Count));

                    InvokeMethodByName(rule.methodNames[k], rule.firstStringParams[k], rule.secondStringParams[k],
                        rule.thirdStringParams[k], rule.useMethodEvents[k], rule.methodEvents[k]);
                }


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
                                number /= newNum;
                            else
                                Other.Debug.LogError("不可除以0.");
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    UnityEngine.Debug.Log(number);
                    item.fact.value = number;
                    rule.factModifications[index] = item;
                    // ReSharper disable once ForCanBeConvertedToForeach
                    for (var l = 0; l < factTable.facts.Count; l++)
                    {
                        var fact = factTable.facts[l];
                        if (fact.name == item.fact.name) fact.value = number;
                        factTable.facts[l] = fact;
                    }
                }
            }

            return eventEntry;
        }

        private static void SetTriggering(string newTrigger, bool value = true)
        {
            for (var index = 0; index < eventTable.events.Count; index++)
            {
                if (eventTable.events[index].name != newTrigger) continue;
                var eventItem = eventTable.events[index];
                eventItem.isTriggering = value;
                eventTable.events[index] = eventItem;
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
            SettingsStorage.pause = true;

            TalkBoxController.Instance.Change(true, true);
            if (TalkBoxController.Instance.boxDrawer.localPosition.z < 0)
                TalkBoxController.Instance.boxDrawer.localPosition = new Vector3(
                    TalkBoxController.Instance.boxDrawer.localPosition.x,
                    TalkBoxController.Instance.boxDrawer.localPosition.y,
                    BackpackBehaviour.BoxZAxisVisible);

            var mainCamera = Camera.main;
            if (mainCamera)
                TalkBoxController.Instance.isUp =
                    MainControl.overworldPlayerBehaviour.transform.position.y <
                    mainCamera.transform.position.y - 1.25f;

            if (!_overworldTypeWritter)
                _overworldTypeWritter = BackpackBehaviour.Instance.typeWritter;

            _overworldTypeWritter.TypeOpen(
                TextProcessingService.GetFirstChildStringByPrefix(
                    MainControl.Instance.overworldControl.sceneTextsSave,
                    dataName),
                false, 0, 1, BackpackBehaviour.Instance.talkText);

            _overworldTypeWritter.OnClose = () =>
            {
                BackpackBehaviour.Instance.talkText.text = "";
                TalkBoxController.Instance.boxDrawer.localPosition = new Vector3(
                    TalkBoxController.Instance.boxDrawer.localPosition.x,
                    TalkBoxController.Instance.boxDrawer.localPosition.y, BackpackBehaviour.BoxZAxisInvisible);
                MainControl.Instance.playerControl.canMove = true;
                SettingsStorage.pause = false;
                if (useEvent)
                    SetTriggering(eventName);
            };
        }

        /// <summary>
        ///     在保持BGM的同时切换场景
        /// </summary>
        /// <param name="sceneName">场景名</param>
        /// <param name="useEvent">是否联动event</param>
        /// <param name="eventName">联动的event名称</param>
        private static void SwitchSceneWithBgm(string sceneName, bool useEvent, string eventName)
        {
            Action action = null;
            if (useEvent)
                action += () => SetTriggering(eventName);

            GameUtilityService.FadeOutAndSwitchScene(sceneName, Color.black, action);
        }


        /// <summary>
        ///     在渐出BGM的同时切换场景
        /// </summary>
        /// <param name="sceneName">场景名</param>
        /// <param name="useEvent">是否联动event</param>
        /// <param name="eventName">联动的event名称</param>
        private static void SwitchSceneWithoutBgm(string sceneName, bool useEvent, string eventName)
        {
            Action action = null;
            if (useEvent)
                action += () => SetTriggering(eventName);

            GameUtilityService.FadeOutAndSwitchScene(sceneName, Color.black, action, true);
            //TODO: 给场景的BGM加一个渐入
        }

        /// <summary>
        ///     传送玩家
        /// </summary>
        private static void TeleportPlayer(Vector2 newPosition, bool useEvent, string eventName)
        {
            if (!MainControl.overworldPlayerBehaviour) return;
            MainControl.overworldPlayerBehaviour.transform.position = newPosition;
            MainControl.Instance.playerControl.playerLastPos = newPosition;

            if (useEvent)
                SetTriggering(eventName);
        }

        private static void OpenSaveBox(bool useEvent, string eventName)
        {
            SaveBoxController.Instance.OpenSaveBox();

            if (useEvent)
                SetTriggering(eventName);
        }

        private static void MoveMainCamera(Vector2 newPosition, float duration, Ease ease, bool useEvent,
            string eventName)
        {
            MainControl.Instance.playerControl.canMove = false;
            
            if (duration < 0)
                UnityEngine.Debug.LogError($"{duration} 应当大于0.");

            var mainCamera = CameraFollowPlayer.Instance;
            mainCamera.isFollow = false;
            if (mainCamera)
                mainCamera.transform
                    .DOMove(new Vector3(newPosition.x, newPosition.y, mainCamera.transform.position.z), duration)
                    .SetEase(ease)
                    .OnKill(() =>
                    {
                        if (useEvent)
                            SetTriggering(eventName);
                    });
        }

        //TODO:多战斗补全
        private static void EnterBattleScene(bool useEvent, string eventName)
        {
            SettingsController.Instance.Animator.SetBool(Open, true);
            if (useEvent)
                SetTriggering(eventName);
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
        private static void InvokeMethodByName(string methodName, string parameter1, string parameter2,
            string parameter3, bool useEvent, string eventName)

        {
            var tag = methodName[..methodName.IndexOf(':')];

            if (MethodDictionary.TryGetValue(methodName, out var method))
                switch (tag)
                {
                    case "Vector2Ease":
                        method.Invoke(TextProcessingService.StringVector2ToRealVector2(parameter1),
                            float.Parse(parameter2),
                            (Ease)int.Parse(parameter3), useEvent, eventName);
                        break;

                    case "string,string":
                        method.Invoke(parameter1, parameter2, useEvent, eventName);
                        break;

                    case "Vector2":
                        method.Invoke(TextProcessingService.StringVector2ToRealVector2(parameter1), useEvent,
                            eventName);
                        break;

                    case "null":
                        method.Invoke(useEvent, eventName);
                        break;

                    // ReSharper disable once RedundantCaseLabel
                    case "string":
                    default:
                        method.Invoke(parameter1, useEvent, eventName);
                        break;
                }
            else
                Console.WriteLine($"方法 {methodName} 未找到！");
        }
    }
}
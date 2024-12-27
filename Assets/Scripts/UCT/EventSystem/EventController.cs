using System;
using System.Collections.Generic;
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
        public static readonly Dictionary<string, Action<string>> MethodDictionary = new()
        {
            { "StartOverworldTypeWritter", StartOverworldTypeWritter }
        };

        private static TypeWritter _overworldTypeWritter;
        public EventTable eventTable;
        public FactTable factTable;
        public RuleTable ruleTable;

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
                Other.Debug.Log(eventEntry.isTriggering);
                if (eventEntry.isTriggering) 
                    eventEntry = DetectionRule(eventEntry, i);
                eventTable.events[i] = eventEntry;
            }
        }

        private EventEntry DetectionRule(EventEntry eventEntry, int i)
        {
            // ReSharper disable once ForCanBeConvertedToForeach
            for (var j = 0; j < ruleTable.rules.Count; j++)
            {
                var rule = ruleTable.rules[j];

                if (eventEntry.name != rule.triggeredBy.name || !rule.ruleCriterion.GetResult()) continue;

                eventEntry.isTriggering = false;
                rule.triggers.isTriggering = true;

                for (var k = 0; k < rule.methodNames.Count; k++)
                    InvokeMethodByName(rule.methodNames[k], rule.methodStrings[k]);


                for (var k = 0; k < rule.factModifications.Count; k++)
                {
                    var item = rule.factModifications[k];
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

                    item.fact.value = number;

                    // ReSharper disable once ForCanBeConvertedToForeach
                    for (var l = 0; l < factTable.facts.Count; l++)
                    {
                        var fact = factTable.facts[l];
                        if (fact.name == item.fact.name) fact.value = number;
                        factTable.facts[l] = fact;
                    }
                }

                ruleTable.rules[j] = rule;
            }

            return eventEntry;
        }

        /// <summary>
        ///     在Overworld场景中启动对话打字机。
        /// </summary>
        private static void StartOverworldTypeWritter(string dataName)
        {
            // 禁止玩家移动并暂停游戏
            MainControl.Instance.playerControl.canMove = false;
            SettingsStorage.pause = true;

            // 改变对话框位置
            TalkBoxPositionChanger.Instance.Change(true, true);
            if (TalkBoxPositionChanger.Instance.boxDrawer.localPosition.z < 0)
            {
                TalkBoxPositionChanger.Instance.boxDrawer.localPosition = new Vector3(
                    TalkBoxPositionChanger.Instance.boxDrawer.localPosition.x,
                    TalkBoxPositionChanger.Instance.boxDrawer.localPosition.y,
                    BackpackBehaviour.BoxZAxisVisible);
            }

            // 根据玩家位置决定对话框显示位置
            if (Camera.main)
            {
                TalkBoxPositionChanger.Instance.isUp =
                    MainControl.Instance.overworldPlayerBehaviour.transform.position.y <
                    Camera.main.transform.position.y - 1.25f;
            }

            // 初始化打字机并执行对话
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
                TalkBoxPositionChanger.Instance.boxDrawer.localPosition = new Vector3(
                    TalkBoxPositionChanger.Instance.boxDrawer.localPosition.x,
                    TalkBoxPositionChanger.Instance.boxDrawer.localPosition.y, BackpackBehaviour.BoxZAxisInvisible);
                MainControl.Instance.playerControl.canMove = true;
                SettingsStorage.pause = false;
            };
        }

        /// <summary>
        ///     调用_methodDictionary字典中注册的方法
        /// </summary>
        /// <param name="methodName">方法名</param>
        /// <param name="parameter">传递的参数</param>
        private static void InvokeMethodByName(string methodName, string parameter)
        {
            if (MethodDictionary.TryGetValue(methodName, out var method))
                method.Invoke(parameter);
            else
                Console.WriteLine($"方法 {methodName} 未找到！");
        }

        // TODO:把下面未完成的函数实现。
        //  开始对话 
        //  中断/停止对话
        //  SFM物体移动到指定位置
        //  SFM物体切换行走/奔跑
        //  SFM物体切换特定动画
        //  玩家掉血
        //  玩家回血
        //  从玩家背包添加物品
        //  从玩家背包移除物品
        //  播放特定音效
        //  播放背景音乐
        //  暂停背景音乐
        //  切换背景音乐
        //  切换场景
        //  移动摄像机
        //  修改SFM物体的data
        //  创建对象
        //  触发特殊视觉效果
    }
}
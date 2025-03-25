using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using UnityEngine;

namespace UCT.Service
{
    public static class EnemiesXmlDialogParser
    {
        public enum DialogType
        {
            Fixed,
            Random
        }

        public enum MessageMode
        {
            Confirm,
            Delay
        }

        /// <summary>
        ///     获取 Dialog 的基本信息
        /// </summary>
        public static Dialog GetDialogInfo(string xmlContent)
        {
            var doc = new XmlDocument();
            doc.LoadXml(xmlContent);

            XmlNode dialogNode = doc.DocumentElement;
            if (dialogNode is not { Name: "Dialog" })
            {
                throw new ArgumentNullException($"{xmlContent}不包含<Dialog>！");
            }

            var result = new Dialog
            {
                Name = dialogNode.Attributes?["name"]?.Value
            };
            if (Enum.TryParse(typeof(DialogType), dialogNode.Attributes?["type"]?.Value, out var type))
            {
                result.Type = (DialogType)type;
            }

            if (int.TryParse(dialogNode.Attributes?["turn"]?.Value, out var turn))
            {
                result.Turn = turn;
            }

            return result;
        }

        /// <summary>
        ///     获取指定 Dialog 下的所有 Message
        /// </summary>
        public static List<Message> GetMessagesInDialog(string xmlContent, string dialogName)
        {
            var doc = new XmlDocument();
            doc.LoadXml(xmlContent);

            var dialogNode = doc.SelectSingleNode($"/Dialog[@name='{dialogName}']");
            if (dialogNode == null)
            {
                Debug.LogError($"Dialog '{dialogName}' not found.");
                throw new ArgumentNullException($"{dialogName}不包含<Message>！");
            }

            var messageNodes = dialogNode.SelectNodes("Message");
            var result = new List<Message>();

            if (messageNodes == null)
            {
                return result;
            }

            foreach (XmlNode messageNode in messageNodes)
            {
                var message = new Message
                {
                    Name = messageNode.Attributes?["name"]?.Value ?? string.Empty,
                    Target = messageNode.Attributes?["target"]?.Value?
                        .Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(s => s.Trim())
                        .ToArray()
                };

                if (Enum.TryParse(typeof(MessageMode), messageNode.Attributes?["mode"]?.Value, out var mode))
                {
                    message.Mode = (MessageMode)mode;
                }

                if (float.TryParse(messageNode.Attributes?["autoDelay"]?.Value, out var autoDelay))
                {
                    message.AutoDelay = autoDelay;
                }

                result.Add(message);
            }

            return result;
        }


        /// <summary>
        ///     获取特定 Message 的信息
        /// </summary>
        public static Message GetMessageInfo(string xmlContent, string dialogName, string messageName)
        {
            var doc = new XmlDocument();
            doc.LoadXml(xmlContent);

            var dialogNode = doc.SelectSingleNode($"/Dialog[@name='{dialogName}']");
            if (dialogNode == null)
            {
                Debug.LogError($"Dialog '{dialogName}' not found.");
                throw new ArgumentNullException($"{dialogName}不包含<Message>！");
            }

            var messageNode = dialogNode.SelectSingleNode($"Message[@name='{messageName}']");
            if (messageNode == null)
            {
                Debug.LogError($"Message '{messageName}' not found in Dialog '{dialogName}'.");
                throw new ArgumentNullException($"{dialogName}不包含<Message>！");
            }

            var result = new Message();

            if (Enum.TryParse(typeof(MessageMode), dialogNode.Attributes?["mode"]?.Value, out var type))
            {
                result.Mode = (MessageMode)type;
            }

            result.Target = messageNode.Attributes?["target"]?.Value?
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .ToArray();
            if (float.TryParse(dialogNode.Attributes?["autoDelay"]?.Value, out var autoDelay))
            {
                result.AutoDelay = autoDelay;
            }

            return result;
        }


        /// <summary>
        ///     获取特定 Message 下的所有 Bubble 信息
        /// </summary>
        public static List<Bubble> GetBubbles(string xmlContent, string messageName)
        {
            var doc = new XmlDocument();
            doc.LoadXml(xmlContent);

            var messageNodes = doc.GetElementsByTagName("Message");
            var result = new List<Bubble>();
            foreach (XmlNode messageNode in messageNodes)
            {
                if (messageNode.Attributes?["name"]?.Value != messageName)
                {
                    continue;
                }

                var bubbleNodes = messageNode.SelectNodes("Bubble");

                Console.WriteLine($"Bubbles in Message '{messageName}':");
                if (bubbleNodes == null)
                {
                    throw new ArgumentNullException($"{messageName}不包含<Bubble>！");
                }

                foreach (XmlNode bubbleNode in bubbleNodes)
                {
                    var bubble = new Bubble
                    {
                        Character = bubbleNode.Attributes?["character"]?.Value
                    };

                    float.TryParse(bubbleNode.Attributes?["sizeX"]?.Value, out var sizeX);
                    float.TryParse(bubbleNode.Attributes?["sizeY"]?.Value, out var sizeY);
                    bubble.Size = new Vector2(sizeX, sizeY);
                    float.TryParse(bubbleNode.Attributes?["offsetX"]?.Value, out var offsetX);
                    float.TryParse(bubbleNode.Attributes?["offsetY"]?.Value, out var offsetY);
                    bubble.Offset = new Vector2(offsetX, offsetY);
                    float.TryParse(bubbleNode.Attributes?["arrowOffset"]?.Value, out var arrowOffset);
                    bubble.ArrowOffset = arrowOffset;

                    bubble.Direction = bubbleNode.Attributes?["direction"]?.Value switch
                    {
                        "right" => true,
                        "left" => false,
                        "true" => true,
                        "false" => false,
                        _ => false
                    };

                    bubble.Text = bubbleNode.InnerText.Trim();
                    result.Add(bubble);
                }

                break;
            }

            return result;
        }

        public struct Dialog
        {
            public string Name;
            public DialogType Type;
            public int Turn;
        }

        public struct Message : IEquatable<Message>
        {
            public string Name;
            public MessageMode Mode;
            public string[] Target;
            public float AutoDelay;
            public bool IsDelaying;

            public bool Equals(Message other)
            {
                return Name == other.Name && Mode == other.Mode && Equals(Target, other.Target) && AutoDelay.Equals(other.AutoDelay) && IsDelaying == other.IsDelaying;
            }

            public override bool Equals(object obj)
            {
                return obj is Message other && Equals(other);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(Name, (int)Mode, Target, AutoDelay, IsDelaying);
            }
        }

        public struct Bubble
        {
            public Vector2 Size;
            public Vector2 Offset;
            public string Character;
            public float ArrowOffset;
            public bool Direction;
            public string Text;
        }
    }
}
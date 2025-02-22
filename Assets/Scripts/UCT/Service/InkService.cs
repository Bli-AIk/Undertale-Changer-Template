using System;
using System.IO;
using Ink;
using Ink.Runtime;
using UnityEngine;

namespace UCT.Service
{
    /// <summary>
    ///     UCT对Ink语言的相关函数封装。
    ///     关于Ink，请参阅：https://www.inklestudios.com/ink/
    /// </summary>
    public static class InkService
    {
        private static Story _story;

        /// <summary>
        ///     从指定路径读取ink文件生成的json文件中包含的Story。
        ///     如果没有json文件或json文件旧于ink文件，会生成新的json文件。
        /// </summary>
        public static Story ReadInkJsonFileFromLocalPath(string path)
        {
            var inkFilePath = $"{path}.ink";
            var jsonFilePath = $"{path}.json";
            if (!File.Exists(inkFilePath))
            {
                throw new ArgumentNullException(inkFilePath);
            }

            string jsonContent;
            if (!File.Exists(jsonFilePath) ||
                File.GetLastWriteTime(jsonFilePath) < File.GetLastWriteTime(inkFilePath))
            {
                var inkContent = File.ReadAllText(inkFilePath);

                var compiler = new Compiler(inkContent);
                var newJson = compiler.Compile().ToJson();
                File.WriteAllText(jsonFilePath, newJson);
                jsonContent = newJson;
            }
            else
            {
                jsonContent = File.ReadAllText(jsonFilePath);
            }
            return new Story(jsonContent);
        }

        public static Story ReadInkJsonFileFromResources(string path)
        {
            var file = Resources.Load<TextAsset>(path);
            return new Story(file.text);
        }
    }
}
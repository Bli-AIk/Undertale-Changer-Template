using System;
using System.IO;
using Plugins.Timer.Source;
using UCT.EventSystem;
using UCT.Global.Core;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Editor.Handler
{
    [InitializeOnLoad]
    public static class OverworldSceneHandler
    {
        static OverworldSceneHandler()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                CheckSceneState();
            }
        }

        private static void CheckSceneState()
        {
            var mainControl = MainControl.Instance;
            if (!mainControl)
            {
                UCT.Other.Debug.LogWarning("OverworldSceneHandler 在 MainControlSummon 物体上未找到脚本。");
                return;
            }

            if (mainControl.sceneState != MainControl.SceneState.Overworld)
            {
                return;
            }

            var sceneName = SceneManager.GetActiveScene().name;
            CreateLanguagePackFiles($"{Application.dataPath}/Resources/TextAssets/LanguagePacks", sceneName);
            CreateLanguagePackFiles($"{Application.dataPath}/LanguagePacks", sceneName);
            EnsureScriptableObjects("Assets/Resources/Tables", sceneName);
        }

        public static void CreateLanguagePackFiles(string rootPath, string sceneName)
        {
            if (string.IsNullOrEmpty(rootPath))
            {
                UCT.Other.Debug.LogError("场景初始化失败：rootPath参数不能为空");
                return;
            }

            if (string.IsNullOrEmpty(sceneName))
            {
                UCT.Other.Debug.LogError("场景初始化失败：name参数不能为空");
                UCT.Other.Debug.Log("你可能在尝试创建一个Overworld场景并运行，但尚未保存此场景。");
                UCT.Other.Debug.Log("请先保存此场景，并为其命名，这个名称不能和其他场景相同。");
                UCT.Other.Debug.Log("如果可以，把此场景添加进Scenes In Build，然后再次运行场景。场景将会自动进行初始化。");
                return;
            }

            if (!Directory.Exists(rootPath))
            {
                UCT.Other.Debug.LogError($"场景初始化失败：{rootPath} 路径不存在");
                return;
            }

            if (sceneName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            {
                UCT.Other.Debug.LogError("场景初始化失败：场景名称包含非法文件名字符");
                return;
            }

            foreach (var subDir in Directory.GetDirectories(rootPath))
            {
                try
                {
                    var overworldPath = Path.Combine(subDir, "Overworld");
                    if (!Directory.Exists(overworldPath))
                    {
                        continue;
                    }

                    var filePath = Path.Combine(overworldPath, $"{sceneName}.txt");
                    if (File.Exists(filePath))
                    {
                        continue;
                    }

                    File.Create(filePath).Close();
                    UCT.Other.Debug.Log($"已生成{sceneName}场景的语言包文件: {filePath}");
                }
                catch (Exception ex)
                {
                    UCT.Other.Debug.LogError($"处理目录 {subDir} 时出错: {ex.Message}");
                }
            }
        }


        public static void EnsureScriptableObjects(string path, string sceneName)
        {
            var folderPath = Path.Combine(path, sceneName);
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            var isFact = EnsureScriptableObject<FactTable>(folderPath, "FactTable");
            var isEvent = EnsureScriptableObject<EventTable>(folderPath, "EventTable");
            var isRule = EnsureScriptableObject<RuleTable>(folderPath, "RuleTable");
            if (!isFact && !isEvent && !isRule)
            {
                return;
            }

            UCT.Other.Debug.Log($"已生成{sceneName}场景的事件系统文件！");
            UCT.Other.Debug.LogWarning("重新加载此场景后即可正常运行！");
            //对于存储在SceneManager的场景，可以自动重载，但此外的只能手动处理。
            Timer.Register(1,()=>SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex));
        }

        private static bool EnsureScriptableObject<T>(string folderPath, string fileName) where T : ScriptableObject
        {
            var assetPath = Path.Combine(folderPath, fileName + ".asset").Replace("\\", "/");
            if (File.Exists(assetPath))
            {
                return false;
            }

            var instance = ScriptableObject.CreateInstance<T>();
            var uniquePath = AssetDatabase.GenerateUniqueAssetPath(assetPath);
            AssetDatabase.CreateAsset(instance, uniquePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return true;
        }
    }
}
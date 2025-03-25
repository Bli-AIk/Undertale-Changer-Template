using TMPro;
using UnityEditor;
using UnityEngine;

namespace Editor.Default
{
    public class SetTmpExtraPadding : EditorWindow
    {
        [MenuItem("Tools/UCT/Set TMP ExtraPadding")]
        private static void SetExtraPadding()
        {
            var tmpObjects = FindObjectsOfType<TextMeshPro>();
            var tmpUIObjects = FindObjectsOfType<TextMeshProUGUI>();

            var count = 0;

            foreach (var tmp in tmpObjects)
            {
                if (!tmp)
                {
                    continue;
                }

                Undo.RecordObject(tmp, "Set ExtraPadding");
                tmp.extraPadding = true;
                EditorUtility.SetDirty(tmp);
                count++;
                UCT.Debug.Log($"设置 {tmp.name} 的 extraPadding 为 true", tmp.gameObject);

            }

            foreach (var tmpUI in tmpUIObjects)
            {
                if (!tmpUI)
                {
                    continue;
                }

                Undo.RecordObject(tmpUI, "Set ExtraPadding");
                tmpUI.extraPadding = true;
                EditorUtility.SetDirty(tmpUI);
                count++;
                UCT.Debug.Log($"设置了{tmpUI.name} 的 extraPadding 为 true", tmpUI.gameObject);
            }

            UCT.Debug.Log($"已将 {count} 个 TextMeshPro 组件的 ExtraPadding 设置为 True");
        }
    }
}
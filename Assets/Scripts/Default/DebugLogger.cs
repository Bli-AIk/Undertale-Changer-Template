using System.Diagnostics;
using Debug = UnityEngine.Debug;

namespace Log
{
    public static class DebugLogger
    {
        public enum type
        { nor, war, err };

        [Conditional(conditionString: "ENABLE_DEBUG_LOG")]
        public static void Log(object content, type type = 0, string color = "#FFFFFF")
        {
            string text = ("<color=" + color + ">" + content + "</color>");
            switch (type)
            {
                case type.war:
                    Debug.LogWarning(text);
                    break;

                case type.err:
                    Debug.LogError(text);
                    break;

                default:
                    Debug.Log(text);
                    break;
            }
        }
    }
}
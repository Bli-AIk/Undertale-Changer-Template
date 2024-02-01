using System.Diagnostics;
using Debug = UnityEngine.Debug;

namespace Log
{
    public static class DebugLogger
    {
        public enum Type
        { nor, war, err };

        [Conditional(conditionString: "ENABLE_DEBUG_LOG")]
        public static void Log(object content, Type type = 0, string color = "#FFFFFF")
        {
            string text = ("<color=" + color + ">" + content + "</color>");
            switch (type)
            {
                case Type.war:
                    Debug.LogWarning(text);
                    break;

                case Type.err:
                    Debug.LogError(text);
                    break;

                default:
                    Debug.Log(text);
                    break;
            }
        }
        [Conditional(conditionString: "ENABLE_DEBUG_LOG")]
        public static void Log(object content, UnityEngine.Object context, Type type = 0, string color = "#FFFFFF")
        {
            string text = ("<color=" + color + ">" + content + "</color>");
            switch (type)
            {
                case Type.war:
                    Debug.LogWarning(text, context);
                    break;

                case Type.err:
                    Debug.LogError(text, context);
                    break;

                default:
                    Debug.Log(text, context);
                    break;
            }
        }
    }
}
using Ink.Runtime;

namespace UCT.Global.Core
{
    /// <summary>
    ///     使用ink语言，控制对话中的选项。
    /// </summary>
    
    public class TypeWritterSelectController
    {
        public Story Story { get; private set; }
        public bool IsSelecting;
        public int GlobalItemIndex;
        public int VisibleItemIndex;
        public void SetStory(Story story)
        {
            Story = story;
            Story.onError += (msg, type) =>
            {
                if (type == Ink.ErrorType.Warning)
                {
                    Other.Debug.LogWarning(msg);
                }
                else
                {
                    Other.Debug.LogError(msg);
                }
            };
        }

        public string GetStoryDialogue()
        {
            var text = "";
            while (Story.canContinue)
            {
                var partText = Story.Continue();
                if (partText[^1] == '\n' || partText[^1] == '\r')
                {
                    partText = partText[..^1];
                }

                text += partText;
            }

            return text;
        }
    }
}
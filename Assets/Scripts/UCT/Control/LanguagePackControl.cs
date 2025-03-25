using System.Collections.Generic;
using UnityEngine;

namespace UCT.Control
{
    /// <summary>
    ///     存储所有物品信息。（准备废除）
    /// </summary>
    [CreateAssetMenu(fileName = "LanguagePackControl", menuName = "UCT-Disposable/LanguagePackControl")]
    public class LanguagePackControl : ScriptableObject
    {
        public List<string> itemTexts;
        public List<string> dataTexts;
        public List<string> sceneTexts;
        public List<string> settingTexts;
    }
}
using System;
using System.Collections.Generic;

namespace UCT.Global.Settings
{
    /// <summary>
    ///     设置页面选项类
    /// </summary>
    public class SettingsOption
    {
        private object _value;

        public Dictionary<int, string> GetSpDataWithIndex;

        public SettingsOption(object value)
        {
            _value = value;
        }

        public string DataName { get; set; }
        public string[] DescriptionDataName { get; set; }
        public OptionType Type { get; set; }
        public Action OnSelected { get; set; }


        public Func<object> SelectionBasedChangedValueGetter { get; set; }
        public Action<object> SelectionBasedChangedValueSetter { get; set; }
        public float SelectionBasedChangedUnit { get; set; }
        public float SelectionBasedChangedUnitWhenGetC { get; set; }
        public float SelectionBasedChangedMax { get; set; }
        public float SelectionBasedChangedMin { get; set; }
        public OptionDisplayMode OptionDisplayMode { get; set; }

        /// <summary>
        /// 进入新层级时的选项索引。
        /// 如果该索引是负数，那么会视为倒数第X位。
        /// </summary>
        public int NewSelectedOption = 0;

        public void SetValue(object newValue)
        {
            _value = newValue;
        }

        public object GetValue()
        {
            return _value;
        }
    }

    public enum OptionType
    {
        SelectionToggle,
        SelectionBasedFalse,
        SelectionBasedTrue,
        EnterLayer,
        EnterScene,
        Back,
        ConfigurableKeyFalse,
        ConfigurableKeyTrue,
        KeyBindingsReset,
        SwitchPage,
        LanguagePackage,
    }

    public enum OptionDisplayMode
    {
        Default,
        Percentage,
        Resolution,
        DataName
    }
}
using System;

namespace UCT.Global.Settings
{
    /// <summary>
    ///     设置页面选项类
    /// </summary>
    public class SettingsOption
    {
        public string DataName { get; set; }
        public string[] DescriptionDataName { get; set; }
        public OptionType Type { get; set; }
        public Action OnSelected { get; set; }
        private object _value;

        public SettingsOption(object value)
        {
            _value = value;
        }



        public Func<object> SelectionBasedChangedValueGetter { get; set; }
        public Action<object> SelectionBasedChangedValueSetter { get; set; }
        public float SelectionBasedChangedUnit { get; set; }
        public float SelectionBasedChangedMax { get; set; }
        public float SelectionBasedChangedMin { get; set; }
        public OptionDisplayMode OptionDisplayMode { get; set; }
        
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
        Back
    }

    public enum OptionDisplayMode
    {
        Default,
        Percentage,
        Resolution
    }
}
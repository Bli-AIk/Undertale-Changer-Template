using System;
using System.Collections.Generic;

namespace UCT.Settings
{
    /// <summary>
    ///     设置页面选项类
    /// </summary>
    public class SettingsOption
    {
        /// <summary>
        ///     该选项类的实际值。
        /// </summary>
        private object _value;

        /// <summary>
        ///     设置页面的选项类，可配置初始值。
        /// </summary>
        /// <param name="value">初始值</param>
        public SettingsOption(object value)
        {
            _value = value;
        }

        /// <summary>
        ///     当该选项的int值与词典内的int值相同时，会在选项值文本上显示对应的dataName。
        /// </summary>
        public Dictionary<int, string> GetSpDataNameWithIndex { get; set; }

        /// <summary>
        ///     进入新层级时的选项索引。
        ///     如果该索引是负数，那么会视为倒数第X位。
        /// </summary>
        public int NewSelectedOption { get; set; }

        /// <summary>
        ///     该选项自身的语言包数据名称。
        /// </summary>
        public string DataName { get; set; }

        /// <summary>
        ///     该选项描述文本的语言包数据名称。
        ///     若此选项的实际值为int，会尝试将其作为数组的索引。
        /// </summary>
        public string[] DescriptionDataName { get; set; }

        /// <summary>
        ///     该选项的选项类型。
        /// </summary>
        public OptionType Type { get; set; }

        /// <summary>
        ///     该选项在被选择后执行的方法。
        /// </summary>
        public Action OnSelected { get; set; }

        //  以下的函数是为SelectionBased类型选项所提供的。
        /// <summary>
        ///     设定SelectionBased类型选项的实际值会联动更改的外部值。
        /// </summary>
        public Action<object> SelectionBasedChangedValueSetter { get; set; }

        /// <summary>
        ///     获取SelectionBased类型选项的实际值联动的的外部值。
        /// </summary>
        public Func<object> SelectionBasedChangedValueGetter { get; set; }

        /// <summary>
        ///     SelectionBased类型选项值在常规情况下单次修改的单位。
        /// </summary>
        public float SelectionBasedChangedUnit { get; set; }

        /// <summary>
        ///     SelectionBased类型选项值在按住C键时单次修改的单位。
        /// </summary>
        public float SelectionBasedChangedUnitWhenGetC { get; set; }

        /// <summary>
        ///     SelectionBased类型选项值的最大值。
        /// </summary>
        public float SelectionBasedChangedMax { get; set; }

        /// <summary>
        ///     SelectionBased类型选项值的最小值。
        /// </summary>
        public float SelectionBasedChangedMin { get; set; }

        /// <summary>
        ///     该选项的展示方式。
        /// </summary>
        public OptionDisplayMode OptionDisplayMode { get; set; }

        /// <summary>
        ///     设定该选项的实际值。
        /// </summary>
        /// <param name="newValue">新的值</param>
        public void SetValue(object newValue)
        {
            _value = newValue;
        }

        /// <summary>
        ///     获取该选项的实际值。
        /// </summary>
        public object GetValue()
        {
            return _value;
        }
    }

    /// <summary>
    ///     用于设置选项的选项类型，定义了选项的显示方式与具体功能。
    /// </summary>
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
        LanguagePackage
    }

    /// <summary>
    ///     用于设置选项的显示方式。
    /// </summary>
    public enum OptionDisplayMode
    {
        Default,
        Percentage,
        Resolution,
        DataName
    }
}
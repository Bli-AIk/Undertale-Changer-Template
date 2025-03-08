using System;
using UCT.Global.Audio;
using UCT.Global.Core;
using UCT.Service;

// ReSharper disable UnusedMemberInSuper.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedType.Global

namespace UCT.Control
{
    /// <summary>
    ///     表示物品数据结构
    /// </summary>
    public readonly struct ItemData : IEquatable<ItemData>
    {
        /// <summary>
        ///     物品名称
        /// </summary>
        public readonly string DataName;

        /// <summary>
        ///     物品数值
        /// </summary>
        public readonly int Value;

        /// <summary>
        ///     初始化 <see cref="ItemData" /> 结构的新实例
        /// </summary>
        /// <param name="dataName">物品名称</param>
        /// <param name="value">物品数值</param>
        public ItemData(string dataName, int value)
        {
            DataName = dataName;
            Value = value;
        }

        /// <summary>
        ///     判断当前实例是否与指定的 <see cref="ItemData" /> 实例相等
        /// </summary>
        /// <param name="other">要比较的 ItemData 实例</param>
        /// <returns>如果相等返回 true，否则返回 false</returns>
        public bool Equals(ItemData other)
        {
            return DataName == other.DataName && Value == other.Value;
        }

        /// <summary>
        ///     判断当前实例是否与指定对象相等
        /// </summary>
        /// <param name="obj">要比较的对象</param>
        /// <returns>如果相等返回 true，否则返回 false</returns>
        public override bool Equals(object obj)
        {
            return obj is ItemData other && Equals(other);
        }

        /// <summary>
        ///     返回当前实例的哈希代码
        /// </summary>
        /// <returns>哈希代码</returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(DataName, Value);
        }
    }

    /// <summary>
    ///     物品的抽象基类
    /// </summary>
    public abstract class GameItem
    {
        private readonly Action<int> _onCheckAction;
        private readonly Action<int> _onDropAction;
        protected Action<int> OnUseAction;

        /// <summary>
        ///     初始化 <see cref="GameItem" /> 类的新实例
        /// </summary>
        /// <param name="data">物品数据</param>
        /// <param name="onUse">使用物品事件</param>
        /// <param name="onCheck">查看物品事件</param>
        /// <param name="onDrop">丢弃物品事件</param>
        protected GameItem(ItemData data, Action<int> onUse, Action<int> onCheck, Action<int> onDrop)
        {
            Data = data;
            OnUseAction = onUse;
            _onCheckAction = onCheck;
            _onDropAction = onDrop;
        }

        /// <summary>
        ///     获取物品数据
        /// </summary>
        public ItemData Data { get; }

        /// <summary>
        ///     使用了物品
        /// </summary>
        /// <param name="index">物品索引</param>
        public void OnUse(int index)
        {
            OnUseAction?.Invoke(index);
        }

        /// <summary>
        ///     查看了物品
        /// </summary>
        /// <param name="index">物品索引</param>
        public void OnCheck(int index)
        {
            _onCheckAction?.Invoke(index);
        }

        /// <summary>
        ///     丢弃了物品
        /// </summary>
        /// <param name="index">物品索引</param>
        public void OnDrop(int index)
        {
            _onDropAction?.Invoke(index);
        }
    }

    /// <summary>
    ///     食物类物品
    /// </summary>
    public class FoodItem : GameItem
    {
        /// <summary>
        ///     初始化 <see cref="FoodItem" /> 类的新实例
        /// </summary>
        /// <param name="data">物品数据</param>
        /// <param name="onUse">使用物品事件</param>
        /// <param name="onCheck">查看物品事件</param>
        /// <param name="onDrop">丢弃物品事件</param>
        public FoodItem(ItemData data, Action<int> onUse, Action<int> onCheck, Action<int> onDrop)
            : base(data, onUse, onCheck, onDrop)
        {
            OnUseAction += ConsumeFood;
        }

        /// <summary>
        ///     消耗食物，恢复玩家生命并移除物品
        /// </summary>
        /// <param name="index">物品索引</param>
        private void ConsumeFood(int index)
        {
            MainControl.Instance.playerControl.hp += Data.Value;
            if (MainControl.Instance.playerControl.hp > MainControl.Instance.playerControl.hpMax)
            {
                MainControl.Instance.playerControl.hp = MainControl.Instance.playerControl.hpMax;
            }

            MainControl.Instance.playerControl.items[index] = null;
            AudioController.Instance.PlayFx(2, MainControl.Instance.AudioControl.fxClipUI);
        }
    }

    /// <summary>
    ///     父级食物类物品，使用后会生成子物品
    /// </summary>
    public class ParentFoodItem : GameItem
    {
        private readonly GameItem _child;

        /// <summary>
        ///     初始化 <see cref="ParentFoodItem" /> 类的新实例
        /// </summary>
        /// <param name="data">物品数据</param>
        /// <param name="onUse">使用物品事件</param>
        /// <param name="onCheck">查看物品事件</param>
        /// <param name="onDrop">丢弃物品事件</param>
        /// <param name="child">子物品实例</param>
        public ParentFoodItem(ItemData data, Action<int> onUse, Action<int> onCheck, Action<int> onDrop, GameItem child)
            : base(data, onUse, onCheck, onDrop)
        {
            _child = child;
            OnUseAction += ConsumeFoodAndSpawnChild;
        }

        /// <summary>
        ///     消耗食物并生成子物品
        /// </summary>
        /// <param name="index">物品索引</param>
        private void ConsumeFoodAndSpawnChild(int index)
        {
            MainControl.Instance.playerControl.hp += Data.Value;
            if (MainControl.Instance.playerControl.hp > MainControl.Instance.playerControl.hpMax)
            {
                MainControl.Instance.playerControl.hp = MainControl.Instance.playerControl.hpMax;
            }

            MainControl.Instance.playerControl.items[index] = _child.Data.DataName;
            AudioController.Instance.PlayFx(2, MainControl.Instance.AudioControl.fxClipUI);
        }
    }

    /// <summary>
    ///     GameItem 生成器（具体类型为 GameItemBuilder&lt;GameItemBuilder&gt;）
    /// </summary>
    public class GameItemBuilder : GameItemBuilder<GameItemBuilder> { }

    /// <summary>
    ///     用于构建食物物品实例的生成器
    /// </summary>
    public class FoodItemBuilder : GameItemBuilder<FoodItemBuilder>
    {
        /// <summary>
        ///     构建 FoodItem 实例
        /// </summary>
        /// <returns>FoodItem 实例</returns>
        public override GameItem Build()
        {
            return new FoodItem(Data, OnUse, OnCheck, OnDrop);
        }
    }

    /// <summary>
    ///     用于构建父级食物物品实例的生成器
    /// </summary>
    public class ParentFoodItemBuilder : GameItemBuilder<ParentFoodItemBuilder>
    {
        private readonly GameItem _child;

        /// <summary>
        ///     初始化 <see cref="ParentFoodItemBuilder" /> 类的新实例
        /// </summary>
        /// <param name="child">子物品实例</param>
        public ParentFoodItemBuilder(GameItem child)
        {
            _child = child;
        }

        /// <summary>
        ///     构建 ParentFoodItem 实例
        /// </summary>
        /// <returns>ParentFoodItem 实例</returns>
        public override GameItem Build()
        {
            return new ParentFoodItem(Data, OnUse, OnCheck, OnDrop, _child);
        }
    }

    /// <summary>
    ///     用于构建 GameItem 实例的生成器基类
    /// </summary>
    /// <typeparam name="T">生成器类型</typeparam>
    public class GameItemBuilder<T> where T : GameItemBuilder<T>
    {
        protected ItemData Data;
        protected Action<int> OnUse, OnCheck, OnDrop;

        /// <summary>
        ///     设置物品数据
        /// </summary>
        /// <param name="dataName">物品名称</param>
        /// <param name="value">物品数值</param>
        /// <returns>当前生成器实例</returns>
        public T SetData(string dataName, int value)
        {
            Data = new ItemData(dataName, value);
            return (T)this;
        }

        /// <summary>
        ///     设置使用物品事件
        /// </summary>
        /// <param name="action">事件委托</param>
        /// <returns>当前生成器实例</returns>
        public T SetOnUse(Action<int> action)
        {
            OnUse += action;
            return (T)this;
        }

        /// <summary>
        ///     设置检查物品事件
        /// </summary>
        /// <param name="action">事件委托</param>
        /// <returns>当前生成器实例</returns>
        public T SetOnCheck(Action<int> action)
        {
            OnCheck += action;
            return (T)this;
        }

        /// <summary>
        ///     设置丢弃物品事件
        /// </summary>
        /// <param name="action">事件委托</param>
        /// <returns>当前生成器实例</returns>
        public T SetOnDrop(Action<int> action)
        {
            OnDrop += action;
            return (T)this;
        }

        /// <summary>
        ///     构建 GameItem 实例
        /// </summary>
        /// <returns>构建后的 GameItem 实例</returns>
        public virtual GameItem Build()
        {
            return new GameItemImpl(Data, OnUse, OnCheck, OnDrop);
        }

        private sealed class GameItemImpl : GameItem
        {
            /// <summary>
            ///     初始化内部 GameItemImpl 实例
            /// </summary>
            /// <param name="data">物品数据</param>
            /// <param name="onUse">使用物品事件</param>
            /// <param name="onCheck">查看物品事件</param>
            /// <param name="onDrop">丢弃物品事件</param>
            public GameItemImpl(ItemData data, Action<int> onUse, Action<int> onCheck, Action<int> onDrop)
                : base(data, onUse, onCheck, onDrop) { }
        }
    }

    /// <summary>
    ///     装备类物品
    /// </summary>
    public class EquipmentItem : GameItem
    {
        private readonly Action<int> _onEquipAction;
        private readonly Action<int> _onRemoveAction;
        private readonly Action<int> _onSwitchAction;
        private readonly Action<int> _onUpdateAction;

        /// <summary>
        ///     初始化 <see cref="EquipmentItem" /> 类的新实例
        /// </summary>
        /// <param name="data">物品数据</param>
        /// <param name="onUse">使用物品事件</param>
        /// <param name="onCheck">查看物品事件</param>
        /// <param name="onDrop">丢弃物品事件</param>
        /// <param name="onSwitch">切换物品事件</param>
        /// <param name="onEquip">装备物品事件</param>
        /// <param name="onRemove">卸下物品事件</param>
        /// <param name="onUpdate">更新物品事件</param>
        protected internal EquipmentItem(ItemData data,
            Action<int> onUse,
            Action<int> onCheck,
            Action<int> onDrop,
            Action<int> onSwitch,
            Action<int> onEquip,
            Action<int> onRemove,
            Action<int> onUpdate)
            : base(data, onUse, onCheck, onDrop)
        {
            _onSwitchAction = onSwitch;
            _onEquipAction = onEquip;
            _onRemoveAction = onRemove;
            _onUpdateAction = onUpdate;
        }

        /// <summary>
        ///     切换装备状态
        /// </summary>
        /// <param name="index">物品索引</param>
        public void OnSwitch(int index)
        {
            _onSwitchAction?.Invoke(index);
        }

        /// <summary>
        ///     装备物品
        /// </summary>
        /// <param name="index">物品索引</param>
        protected void OnEquip(int index)
        {
            _onEquipAction?.Invoke(index);
        }

        /// <summary>
        ///     卸下物品
        /// </summary>
        /// <param name="index">物品索引</param>
        public void OnRemove(int index)
        {
            _onRemoveAction?.Invoke(index);
        }

        /// <summary>
        ///     更新物品状态
        /// </summary>
        /// <param name="index">物品索引</param>
        public void OnUpdate(int index)
        {
            _onUpdateAction?.Invoke(index);
        }
    }

    /// <summary>
    ///     装备物品生成器
    /// </summary>
    public class EquipmentItemBuilder : EquipmentItemBuilder<EquipmentItemBuilder> { }

    /// <summary>
    ///     用于构建装备物品实例的生成器基类
    /// </summary>
    /// <typeparam name="T">生成器类型</typeparam>
    public class EquipmentItemBuilder<T> : GameItemBuilder<T> where T : EquipmentItemBuilder<T>
    {
        protected Action<int> OnSwitch, OnEquip, OnRemove, OnUpdate;

        /// <summary>
        ///     设置切换装备事件
        /// </summary>
        /// <param name="action">事件委托</param>
        /// <returns>当前生成器实例</returns>
        public T SetOnSwitch(Action<int> action)
        {
            OnSwitch += action;
            return (T)this;
        }

        /// <summary>
        ///     设置装备物品事件
        /// </summary>
        /// <param name="action">事件委托</param>
        /// <returns>当前生成器实例</returns>
        public T SetOnEquip(Action<int> action)
        {
            OnEquip += action;
            return (T)this;
        }

        /// <summary>
        ///     设置卸下物品事件
        /// </summary>
        /// <param name="action">事件委托</param>
        /// <returns>当前生成器实例</returns>
        public T SetOnRemove(Action<int> action)
        {
            OnRemove += action;
            return (T)this;
        }

        /// <summary>
        ///     设置更新物品事件
        /// </summary>
        /// <param name="action">事件委托</param>
        /// <returns>当前生成器实例</returns>
        public T SetOnUpdate(Action<int> action)
        {
            OnUpdate += action;
            return (T)this;
        }

        /// <summary>
        ///     构建装备物品实例
        /// </summary>
        /// <returns>构建后的装备物品实例</returns>
        public override GameItem Build()
        {
            return new EquipmentItem(Data, OnUse, OnCheck, OnDrop, OnSwitch, OnEquip, OnRemove, OnUpdate);
        }
    }

    /// <summary>
    ///     武器类物品
    /// </summary>
    public class WeaponItem : EquipmentItem
    {
        private readonly Action<int> _onAttackAction;
        private readonly Action<int> _onHitAction;
        private readonly Action<int> _onMissAction;

        /// <summary>
        ///     初始化 <see cref="WeaponItem" /> 类的新实例
        /// </summary>
        /// <param name="data">物品数据</param>
        /// <param name="onUse">使用物品事件</param>
        /// <param name="onCheck">查看物品事件</param>
        /// <param name="onDrop">丢弃物品事件</param>
        /// <param name="onSwitch">切换物品事件</param>
        /// <param name="onEquip">装备物品事件</param>
        /// <param name="onRemove">卸下物品事件</param>
        /// <param name="onUpdate">更新物品事件</param>
        /// <param name="onAttack">武器攻击事件</param>
        /// <param name="onMiss">武器未击中事件</param>
        /// <param name="onHit">武器击中事件</param>
        protected internal WeaponItem(ItemData data,
            Action<int> onUse,
            Action<int> onCheck,
            Action<int> onDrop,
            Action<int> onSwitch,
            Action<int> onEquip,
            Action<int> onRemove,
            Action<int> onUpdate,
            Action<int> onAttack,
            Action<int> onMiss,
            Action<int> onHit)
            : base(data, onUse, onCheck, onDrop, onSwitch, onEquip, onRemove, onUpdate)
        {
            _onAttackAction = onAttack;
            _onMissAction = onMiss;
            _onHitAction = onHit;

            OnUseAction += EquipWeapon;
        }

        /// <summary>
        ///     装备武器，执行切换和装备操作，并处理旧武器的卸下
        /// </summary>
        /// <param name="index">物品索引</param>
        private void EquipWeapon(int index)
        {
            OnSwitch(index);
            OnEquip(index);

            (MainControl.Instance.playerControl.wearWeapon, MainControl.Instance.playerControl.items[index]) =
                (MainControl.Instance.playerControl.items[index], MainControl.Instance.playerControl.wearWeapon);
            AudioController.Instance.PlayFx(3, MainControl.Instance.AudioControl.fxClipUI);

            if (DataHandlerService.GetItemFormDataName(MainControl.Instance.playerControl.items[index]) is not
                EquipmentItem unEquippedItem)
            {
                return;
            }

            unEquippedItem.OnSwitch(index);
            unEquippedItem.OnRemove(index);
        }

        /// <summary>
        ///     武器攻击敌人时触发（无论是否击中）
        /// </summary>
        /// <param name="index">物品索引</param>
        public void OnAttack(int index)
        {
            _onAttackAction?.Invoke(index);
        }

        /// <summary>
        ///     武器未击中敌人时触发
        /// </summary>
        /// <param name="index">物品索引</param>
        public void OnMiss(int index)
        {
            _onMissAction?.Invoke(index);
        }

        /// <summary>
        ///     武器击中敌人时触发
        /// </summary>
        /// <param name="index">物品索引</param>
        public void OnHit(int index)
        {
            _onHitAction?.Invoke(index);
        }
    }

    /// <summary>
    ///     护甲类物品
    /// </summary>
    public class ArmorItem : EquipmentItem
    {
        private readonly Action<int> _onDamageTakenAction;

        /// <summary>
        ///     初始化 <see cref="ArmorItem" /> 类的新实例
        /// </summary>
        /// <param name="data">物品数据</param>
        /// <param name="onUse">使用物品事件</param>
        /// <param name="onCheck">查看物品事件</param>
        /// <param name="onDrop">丢弃物品事件</param>
        /// <param name="onSwitch">切换物品事件</param>
        /// <param name="onEquip">装备物品事件</param>
        /// <param name="onRemove">卸下物品事件</param>
        /// <param name="onUpdate">更新物品事件</param>
        /// <param name="onDamageTaken">受到伤害事件</param>
        protected internal ArmorItem(ItemData data,
            Action<int> onUse,
            Action<int> onCheck,
            Action<int> onDrop,
            Action<int> onSwitch,
            Action<int> onEquip,
            Action<int> onRemove,
            Action<int> onUpdate,
            Action<int> onDamageTaken)
            : base(data, onUse, onCheck, onDrop, onSwitch, onEquip, onRemove, onUpdate)
        {
            _onDamageTakenAction = onDamageTaken;

            OnUseAction += EquipArmor;
        }

        /// <summary>
        ///     装备护甲，执行切换和装备操作，并处理旧护甲的卸下
        /// </summary>
        /// <param name="index">物品索引</param>
        private void EquipArmor(int index)
        {
            OnSwitch(index);
            OnEquip(index);
            (MainControl.Instance.playerControl.wearArmor, MainControl.Instance.playerControl.items[index]) = (
                MainControl.Instance.playerControl.items[index], MainControl.Instance.playerControl.wearArmor);
            AudioController.Instance.PlayFx(3, MainControl.Instance.AudioControl.fxClipUI);

            if (DataHandlerService.GetItemFormDataName(MainControl.Instance.playerControl.items[index]) is not
                EquipmentItem unEquippedItem)
            {
                return;
            }

            unEquippedItem.OnSwitch(index);
            unEquippedItem.OnRemove(index);
        }

        /// <summary>
        ///     当受到伤害时触发
        /// </summary>
        /// <param name="index">物品索引</param>
        public void OnDamageTaken(int index)
        {
            _onDamageTakenAction?.Invoke(index);
        }
    }

    /// <summary>
    ///     武器物品生成器
    /// </summary>
    public class WeaponItemBuilder : EquipmentItemBuilder<WeaponItemBuilder>
    {
        private Action<int> _onAttack, _onMiss, _onHit;

        /// <summary>
        ///     设置武器攻击事件
        /// </summary>
        /// <param name="action">事件委托</param>
        /// <returns>当前生成器实例</returns>
        public WeaponItemBuilder SetOnAttack(Action<int> action)
        {
            _onAttack += action;
            return this;
        }

        /// <summary>
        ///     设置武器未击中事件
        /// </summary>
        /// <param name="action">事件委托</param>
        /// <returns>当前生成器实例</returns>
        public WeaponItemBuilder SetOnMiss(Action<int> action)
        {
            _onMiss += action;
            return this;
        }

        /// <summary>
        ///     设置武器击中事件
        /// </summary>
        /// <param name="action">事件委托</param>
        /// <returns>当前生成器实例</returns>
        public WeaponItemBuilder SetOnHit(Action<int> action)
        {
            _onHit += action;
            return this;
        }

        /// <summary>
        ///     构建 WeaponItem 实例
        /// </summary>
        /// <returns>构建后的 WeaponItem 实例</returns>
        public override GameItem Build()
        {
            return new WeaponItem(Data, OnUse, OnCheck, OnDrop, OnSwitch, OnEquip, OnRemove, OnUpdate,
                _onAttack, _onMiss, _onHit);
        }
    }

    /// <summary>
    ///     护甲物品生成器
    /// </summary>
    public class ArmorItemBuilder : EquipmentItemBuilder<ArmorItemBuilder>
    {
        private Action<int> _onDamageTaken;

        /// <summary>
        ///     设置受到伤害事件
        /// </summary>
        /// <param name="action">事件委托</param>
        /// <returns>当前生成器实例</returns>
        public ArmorItemBuilder SetOnDamageTaken(Action<int> action)
        {
            _onDamageTaken += action;
            return this;
        }

        /// <summary>
        ///     构建 ArmorItem 实例
        /// </summary>
        /// <returns>构建后的 ArmorItem 实例</returns>
        public override GameItem Build()
        {
            return new ArmorItem(Data, OnUse, OnCheck, OnDrop, OnSwitch, OnEquip, OnRemove, OnUpdate,
                _onDamageTaken);
        }
    }
}
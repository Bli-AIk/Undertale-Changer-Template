using System;
using System.Collections.Generic;
using UCT.Global.Audio;
using UCT.Global.Core;

namespace UCT.Control
{
    public readonly struct ItemData : IEquatable<ItemData>
    {
        public readonly string DataName;
        public readonly int Value;

        public ItemData(string dataName, int value)
        {
            DataName = dataName;
            Value = value;
        }

        public bool Equals(ItemData other)
        {
            return DataName == other.DataName && Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            return obj is ItemData other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(DataName, Value);
        }
    }

    public abstract class GameItem
    {
        private readonly Action<int> _onCheckAction;
        private readonly Action<int> _onDropAction;
        protected Action<int> OnUseAction;

        protected GameItem(ItemData data, Action<int> onUse, Action<int> onCheck, Action<int> onDrop)
        {
            Data = data;
            OnUseAction = onUse;
            _onCheckAction = onCheck;
            _onDropAction = onDrop;
        }

        public ItemData Data { get; }

        public void OnUse(int index)
        {
            OnUseAction?.Invoke(index);
        }

        public void OnCheck(int index)
        {
            _onCheckAction?.Invoke(index);
        }

        public void OnDrop(int index)
        {
            _onDropAction?.Invoke(index);
        }
    }

    public class FoodItem : GameItem
    {
        public FoodItem(ItemData data, Action<int> onUse, Action<int> onCheck, Action<int> onDrop) : base(data, onUse,
            onCheck, onDrop)
        {
            OnUseAction += ConsumeFood;
        }

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

    public class ParentFoodItem : GameItem
    {
        private readonly GameItem _child;

        public ParentFoodItem(ItemData data, Action<int> onUse, Action<int> onCheck, Action<int> onDrop, GameItem child)
            : base(data, onUse,
                onCheck, onDrop)
        {
            _child = child;
            OnUseAction += ConsumeFoodAndSpawnChild;
        }

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

    public class GameItemBuilder : GameItemBuilder<GameItemBuilder> { }

    public class FoodItemBuilder : GameItemBuilder<FoodItemBuilder>
    {
        public override GameItem Build()
        {
            return new FoodItem(Data, OnUse, OnCheck, OnDrop);
        }
    }

    public class ParentFoodItemBuilder : GameItemBuilder<ParentFoodItemBuilder>
    {
        private readonly GameItem _child;

        public ParentFoodItemBuilder(GameItem child)
        {
            _child = child;
        }

        public override GameItem Build()
        {
            return new ParentFoodItem(Data, OnUse, OnCheck, OnDrop, _child);
        }
    }

    public class GameItemBuilder<T> where T : GameItemBuilder<T>
    {
        protected ItemData Data;
        protected Action<int> OnUse, OnCheck, OnDrop;

        public virtual T SetData(string dataName, int value)
        {
            Data = new ItemData(dataName, value);
            return (T)this;
        }

        public T SetOnUse(Action<int> action)
        {
            OnUse += action;
            return (T)this;
        }

        public T SetOnCheck(Action<int> action)
        {
            OnCheck += action;
            return (T)this;
        }

        public T SetOnDrop(Action<int> action)
        {
            OnDrop += action;
            return (T)this;
        }

        public virtual GameItem Build()
        {
            return new GameItemImpl(Data, OnUse, OnCheck, OnDrop);
        }

        private sealed class GameItemImpl : GameItem
        {
            public GameItemImpl(ItemData data, Action<int> onUse, Action<int> onCheck, Action<int> onDrop)
                : base(data, onUse, onCheck, onDrop) { }
        }
    }

    public class EquipmentItem : GameItem
    {
        private readonly Action<int> _onEquipAction;
        private readonly Action<int> _onRemoveAction;
        private readonly Action<int> _onSwitchAction;
        private readonly Action<int> _onUpdateAction;

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

        public void OnSwitch(int index)
        {
            _onSwitchAction?.Invoke(index);
        }

        public void OnEquip(int index)
        {
            _onEquipAction?.Invoke(index);
        }

        public void OnRemove(int index)
        {
            _onRemoveAction?.Invoke(index);
        }

        public void OnUpdate(int index)
        {
            _onUpdateAction?.Invoke(index);
        }
    }

    public class EquipmentItemBuilder : EquipmentItemBuilder<EquipmentItemBuilder> { }

    public class EquipmentItemBuilder<T> : GameItemBuilder<T> where T : EquipmentItemBuilder<T>
    {
        protected Action<int> OnSwitch, OnEquip, OnRemove, OnUpdate;

        public T SetOnSwitch(Action<int> action)
        {
            OnSwitch += action;
            return (T)this;
        }

        public T SetOnEquip(Action<int> action)
        {
            OnEquip += action;
            return (T)this;
        }

        public T SetOnRemove(Action<int> action)
        {
            OnRemove += action;
            return (T)this;
        }

        public T SetOnUpdate(Action<int> action)
        {
            OnUpdate += action;
            return (T)this;
        }

        public override GameItem Build()
        {
            return new EquipmentItem(Data, OnUse, OnCheck, OnDrop, OnSwitch, OnEquip, OnRemove, OnUpdate);
        }
    }

    public class WeaponItem : EquipmentItem
    {
        private readonly Action<int> _onAttackAction;
        private readonly Action<int> _onHitAction;
        private readonly Action<int> _onHitMissAction;
        private readonly Action<int> _onMissAction;

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
            Action<int> onHit,
            Action<int> onHitMiss)
            : base(data, onUse, onCheck, onDrop, onSwitch, onEquip, onRemove, onUpdate)
        {
            _onAttackAction = onAttack;
            _onMissAction = onMiss;
            _onHitAction = onHit;
            _onHitMissAction = onHitMiss;

            OnUseAction += EquipWeapon;
        }

        private static void EquipWeapon(int index)
        {
            (MainControl.Instance.playerControl.wearWeapon, MainControl.Instance.playerControl.items[index]) = (
                MainControl.Instance.playerControl.items[index], MainControl.Instance.playerControl.wearWeapon);
            AudioController.Instance.PlayFx(3, MainControl.Instance.AudioControl.fxClipUI);
        }


        public void OnAttack(int index)
        {
            _onAttackAction?.Invoke(index);
        }

        public void OnMiss(int index)
        {
            _onMissAction?.Invoke(index);
        }

        public void OnHit(int index)
        {
            _onHitAction?.Invoke(index);
        }

        public void OnHitMiss(int index)
        {
            _onHitMissAction?.Invoke(index);
        }
    }

    public class ArmorItem : EquipmentItem
    {
        private readonly Action<int> _onDamageTakenAction;

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

        private static void EquipArmor(int index)
        {
            (MainControl.Instance.playerControl.wearArmor, MainControl.Instance.playerControl.items[index]) = (
                MainControl.Instance.playerControl.items[index], MainControl.Instance.playerControl.wearArmor);
            AudioController.Instance.PlayFx(3, MainControl.Instance.AudioControl.fxClipUI);
        }

        public void OnDamageTaken(int index)
        {
            _onDamageTakenAction?.Invoke(index);
        }
    }

    public class WeaponItemBuilder : EquipmentItemBuilder<WeaponItemBuilder>
    {
        private Action<int> _onAttack, _onMiss, _onHit, _onHitMiss;

        public WeaponItemBuilder SetOnAttack(Action<int> action)
        {
            _onAttack += action;
            return this;
        }

        public WeaponItemBuilder SetOnMiss(Action<int> action)
        {
            _onMiss += action;
            return this;
        }

        public WeaponItemBuilder SetOnHit(Action<int> action)
        {
            _onHit += action;
            return this;
        }

        public WeaponItemBuilder SetOnHitMiss(Action<int> action)
        {
            _onHitMiss += action;
            return this;
        }

        public override GameItem Build()
        {
            return new WeaponItem(Data, OnUse, OnCheck, OnDrop, OnSwitch, OnEquip, OnRemove, OnUpdate,
                _onAttack, _onMiss, _onHit, _onHitMiss);
        }
    }


    public class ArmorItemBuilder : EquipmentItemBuilder<ArmorItemBuilder>
    {
        private Action<int> _onDamageTaken;

        public ArmorItemBuilder SetOnDamageTaken(Action<int> action)
        {
            _onDamageTaken += action;
            return this;
        }

        public override GameItem Build()
        {
            return new ArmorItem(Data, OnUse, OnCheck, OnDrop, OnSwitch, OnEquip, OnRemove, OnUpdate,
                _onDamageTaken);
        }
    }

    public class ItemController
    {
        public Dictionary<string, GameItem> ItemDictionary { get; } = new();

        public void InitializeItems()
        {
            var serving = new FoodItemBuilder()
                .SetData("Serving", 5)
                .Build();
            AddItem(serving);

            AddItem(new ParentFoodItemBuilder(serving)
                .SetData("TwoServings", 10)
                .Build());


            AddItem(new WeaponItemBuilder()
                .SetData("TKnife", 1)
                .Build());

            AddItem(new WeaponItemBuilder()
                .SetData("PSword", 999)
                .Build());

            AddItem(new ArmorItemBuilder()
                .SetData("TPS", 123)
                .Build());

            AddItem(new ArmorItemBuilder()
                .SetData("WearableSth", 456)
                .Build());
        }

        private void AddItem(GameItem item)
        {
            ItemDictionary.Add(item.Data.DataName, item);
        }
    }
}
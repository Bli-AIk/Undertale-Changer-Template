using System.Collections.Generic;

namespace UCT.Control
{
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
                .SetData("TKnife", 20)
                .Build());

            AddItem(new WeaponItemBuilder()
                .SetData("PSword", 999)
                .SetOnSwitch(_ => Debug.Log("切换了PSword"))
                .SetOnEquip(_ => Debug.Log("装备了PSword"))
                .SetOnRemove(_ => Debug.Log("卸下了PSword"))
                .SetOnAttack(_ => Debug.Log("用PSword攻击了"))
                .SetOnHit(_ => Debug.Log("用PSword打中了"))
                .SetOnMiss(_ => Debug.Log("用PSword没打中"))
                .Build());

            AddItem(new ArmorItemBuilder()
                .SetData("TPS", 123)
                .SetOnDamageTaken(_ => Debug.Log("击中了TPS"))
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
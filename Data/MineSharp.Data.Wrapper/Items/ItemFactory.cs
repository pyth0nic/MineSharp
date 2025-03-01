﻿using MineSharp.Core.Types;

namespace MineSharp.Data.Items {
    public class ItemFactory {

        public static Item CreateItem(Type type, byte count, int? damage, fNbt.NbtCompound? metadata) {

            if (!type.IsAssignableTo(typeof(Item)))
                throw new ArgumentException();

            object?[] parameters = new object?[] {
                count, damage, metadata
            };

            return (Item)Activator.CreateInstance(type, parameters)!;
        }

        public static Item CreateItem(int id, byte count, int? damage, fNbt.NbtCompound? metadata) {
            var type = ItemPalette.GetItemTypeById(id);
            return CreateItem(type, count, damage, metadata);
        }

    }
}

﻿using System.Globalization;
using System.Text;

namespace MineSharp.Data.Generator.Items {
    internal class ItemGenerator : Generator {
        public ItemGenerator(MinecraftDataHelper wrapper, string version) : base(wrapper, version) {
        }

        public string[] GetUsings() {
            return new[] { "MineSharp.Core.Types", "fNbt" };
        }

        public override void WriteCode(CodeGenerator codeGenerator) {

            var itemData = Wrapper.LoadJson<ItemJsonInfo[]>(Version, "items");

            NumberFormatInfo nfi = new NumberFormatInfo();
            nfi.NumberDecimalSeparator = ".";

            codeGenerator.CommentBlock($"Generated Item Data for Minecraft Version {Version}");

            foreach (var ns in GetUsings())
                codeGenerator.WriteLine($"using {ns};");

            codeGenerator.Begin("namespace MineSharp.Data.Items");

            codeGenerator.Begin("public static class ItemPalette");

            codeGenerator.Begin("public static Type GetItemTypeById(int id) => id switch");
            foreach (var item in itemData)
                codeGenerator.WriteLine($"{item.Id} => typeof({Wrapper.GetCSharpName(item.Name)}Item),");
            codeGenerator.WriteLine($"_ => throw new ArgumentException($\"Item with id {{id}} not found!\")");
            codeGenerator.Finish(semicolon: true);
            codeGenerator.Finish();

            foreach (var item in itemData) {

                codeGenerator.Begin($"public class {Wrapper.GetCSharpName(item.Name)}Item : Item");
                codeGenerator.WriteBlock(
$@"public const int ItemId = { item.Id };
		public const string ItemName = "" { item.Name }"";
		public const string ItemDisplayName = ""{ item.DisplayName }"";

        public const byte ItemStackSize = { item.StackSize };
        public static readonly int? ItemMaxDurability = { (item.MaxDurability.ToString() ?? "null") };
        public static readonly string[]? ItemEnchantCategories = { WriteStringArray(item.EnchantCategories) };
		public static readonly string[]? ItemRepairWith = { WriteStringArray(item.RepairWith) };


        public { Wrapper.GetCSharpName(item.Name) }Item () : base (ItemId, ItemDisplayName, ItemName, ItemStackSize, ItemMaxDurability, ItemEnchantCategories, ItemRepairWith) {{}}
		public {Wrapper.GetCSharpName(item.Name) }Item (byte count, int? damage, fNbt.NbtCompound? metadata) : base(count, damage, metadata, ItemId, ItemDisplayName, ItemName, ItemStackSize, ItemMaxDurability, ItemEnchantCategories, ItemRepairWith) {{}}");
                codeGenerator.Finish();

            }

            codeGenerator.Begin("public enum ItemType");
            foreach (var item in itemData)
                codeGenerator.WriteLine($"{Wrapper.GetCSharpName(item.Name)} = {item.Id},");
            codeGenerator.Finish();
            codeGenerator.Finish();
        }

        string WriteStringArray(string[]? arr) {
            if (arr == null) return ("null");
            else {
                StringBuilder sb = new StringBuilder();
                sb.Append("new string[] {");
                sb.Append(string.Join(", ", arr.Select(x => '"' + x + '"')));
                sb.Append("}");
                return sb.ToString();

            }
        }
    }
}

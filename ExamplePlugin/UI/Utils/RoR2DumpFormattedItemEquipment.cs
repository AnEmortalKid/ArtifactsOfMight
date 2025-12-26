using System;
using System.Text;
using RoR2;

namespace ArtifactsOfMight.UI.Utils
{
    /// <summary>
    /// Taken from https://gist.githubusercontent.com/DestroyedClone/e7219428e5ce6022ab3fc2b80a6c12ca/raw/688c84154b3b39d109e1970aba5b559fe8c11594/RoR2DumpFormattedItemEquipment.cs
    /// </summary>
    public static class ItemCatalogDumper
    {
        public static void ParseAndLog()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine($"\n## Items\n");
            stringBuilder.AppendLine("| Index | Code Name | Real Name | Tier | Tags | DLC |");
            stringBuilder.AppendLine("|-------|-----------|-----------|------|------|-----|");

            foreach (var item in ItemCatalog.allItemDefs)
            {
                stringBuilder.Append($"| {item.itemIndex} | {item.name} | {Language.GetString(item.nameToken)} ");

                string tierString = "No Tier";
                switch (item.tier)
                {
                    case ItemTier.Boss:
                        tierString = "Boss";
                        break;

                    case ItemTier.Lunar:
                        tierString = "Lunar";
                        break;

                    case ItemTier.Tier1:
                        tierString = "White";
                        break;

                    case ItemTier.Tier2:
                        tierString = "Green";
                        break;

                    case ItemTier.Tier3:
                        tierString = "Red";
                        break;

                    case ItemTier.VoidBoss:
                        tierString = "Void Boss";
                        break;

                    case ItemTier.VoidTier1:
                        tierString = "Void White";
                        break;

                    case ItemTier.VoidTier2:
                        tierString = "Void Green";
                        break;

                    case ItemTier.VoidTier3:
                        tierString = "Void Red";
                        break;
                }
                stringBuilder.Append($"| {tierString} ");
                stringBuilder.Append("| ");
                for (int j = 0; j < item.tags.Length; j++)
                {
                    var tag = item.tags[j];

                    if (j == 0)
                    {
                        if (item.tags.Length > 1)
                            stringBuilder.Append($"{tag}, ");
                        else
                            stringBuilder.Append($"{tag}");
                    }
                    else if (j < item.tags.Length - 1)
                        stringBuilder.Append($"{tag}, ");
                    else
                        stringBuilder.Append($"{tag}");
                }
                if (item.tags.Length == 0)
                    stringBuilder.Append("No Tag");
                if (item.requiredExpansion)
                {
                    stringBuilder.Append($"| {Language.GetString(item.requiredExpansion.nameToken)}");
                }
                else
                {
                    stringBuilder.Append($"| No DLC");
                }

                stringBuilder.Append($"|{Environment.NewLine}");
            }

            //Debug.LogWarning(stringBuilder);

            //stringBuilder.Clear();
            stringBuilder.AppendLine($"\n\n## Equipments\n\n");

            stringBuilder.AppendLine("| Index | Code Name | Real Name | Tier | DLC |");
            stringBuilder.AppendLine("|-------|-----------|-----------|------|-----|");

            foreach (var item in EquipmentCatalog.equipmentDefs)
            {
                stringBuilder.Append($"| {item.equipmentIndex} | {item.name} | {Language.GetString(item.nameToken)} ");
                var tier = item.isLunar ? "Lunar" : "Normal";
                stringBuilder.Append($"| {tier}");

                if (item.requiredExpansion)
                {
                    stringBuilder.Append($"| {Language.GetString(item.requiredExpansion.nameToken)}");
                }
                else
                {
                    stringBuilder.Append($"| No DLC");
                }
                stringBuilder.Append($"|{Environment.NewLine}");
            }

            Log.Warning(stringBuilder);
        }
    }
}

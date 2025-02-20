using SDG.Unturned;
using System.Linq;
using UnityEngine;

namespace Ocelot.BlueCrystalCooking.functions
{
    public static class BarricadeFunctions
    {
        public static void BarricadeDamaged(Transform barricadeTransform, ushort pendingTotalDamage)
        {
            if (!barricadeTransform) return;
            var bData = BlueCrystalCookingPlugin.Instance.GetBarricadeDataAtPosition(barricadeTransform.position);
            if (bData == null)
                return;

            if (bData.barricade.health > pendingTotalDamage) return;
            if (BlueCrystalCookingPlugin.Instance.Configuration.Instance.BarrelObjectId == bData.barricade.id)
            {
                BlueCrystalCookingPlugin.Instance.PlacedBarrelsTransformsIngredients.Remove(barricadeTransform);
            }
            else if (BlueCrystalCookingPlugin.Instance.Configuration.Instance.BlueCrystalTrayId == bData.barricade.id)
            {
                foreach(var tray in BlueCrystalCookingPlugin.Instance.FreezingTrays.ToList())
                {
                    if (!tray.Transform)
                        return;

                    if (tray.Transform == barricadeTransform)
                    {
                        BlueCrystalCookingPlugin.Instance.FreezingTrays.Remove(tray);
                    }
                }
            }
        }
    }
}

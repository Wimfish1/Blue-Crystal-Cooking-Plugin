using SDG.Unturned;
using System.Linq;
using UnityEngine;

namespace Ocelot.BlueCrystalCooking.functions
{
    public static class BarricadeFunctions
    {
        public static void BarricadeDamaged(Transform barricadeTransform, ushort pendingTotalDamage)
        {
            if (barricadeTransform)
            {
                BarricadeData bData = BlueCrystalCookingPlugin.Instance.getBarricadeDataAtPosition(barricadeTransform.position);
                if (bData == null)
                    return;

                if (bData.barricade.health <= pendingTotalDamage)
                {
                    if (BlueCrystalCookingPlugin.Instance.Configuration.Instance.BarrelObjectId == bData.barricade.id)
                    {
                        BlueCrystalCookingPlugin.Instance.placedBarrelsTransformsIngredients.Remove(barricadeTransform);
                    }
                    else if (BlueCrystalCookingPlugin.Instance.Configuration.Instance.BlueCrystalTrayId == bData.barricade.id)
                    {
                        foreach(var tray in BlueCrystalCookingPlugin.Instance.freezingTrays.ToList())
                        {
                            if (tray.transform == null)
                                return;

                            if (tray.transform == barricadeTransform)
                            {
                                BlueCrystalCookingPlugin.Instance.freezingTrays.Remove(tray);
                            }
                        }
                    }
                }
            }
        }
    }
}

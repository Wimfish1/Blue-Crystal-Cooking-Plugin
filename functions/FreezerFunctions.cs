using SDG.Unturned;
using System.Linq;
using UnityEngine;

namespace Ocelot.BlueCrystalCooking.functions
{
    public static class FreezerFunctions
    {
        public static void BarricadeDeployed(Barricade barricade, ItemBarricadeAsset asset, Transform hit, ref Vector3 point, ref float angleX, ref float angleY, ref float angleZ, ref ulong owner, ref ulong group, ref bool shouldAllow)
        {
            var pos = point;
            if (barricade.id != BlueCrystalCookingPlugin.Instance.Configuration.Instance.LiquidTrayId) return;
            if (!Physics.Raycast(pos, Vector3.down, out RaycastHit raycastHit, 10, RayMasks.BARRICADE)) return;
            if (!Physics.Raycast(pos, Vector3.up, out RaycastHit raycastHitUp, 10, RayMasks.BARRICADE)) return;
            if (raycastHit.transform != raycastHitUp.transform) return;
            BarricadeManager.tryGetInfo(raycastHit.transform, out var x, out var y, out var plant, out var index, out var region, out var drop);
            if (drop.asset.id != BlueCrystalCookingPlugin.Instance.Configuration.Instance.FreezerId) return;
            var ownerTray = owner;
            var groupTray = group;
            var angleXTray = angleX;
            var angleYTray = angleY;
            var angleZTray = angleZ;
            BlueCrystalCookingPlugin.Instance.Wait(0.5f, () =>
            {
                Transform tray = BlueCrystalCookingPlugin.Instance.GetPlacedObjectTransform(pos);
                BlueCrystalCookingPlugin.Instance.FreezingTrays.Add(new FreezingTrayObject(tray, pos, ownerTray, groupTray, angleXTray, angleYTray, angleZTray, 0));
            });
        }
        public static void Update()
        {
            foreach (var tray in BlueCrystalCookingPlugin.Instance.FreezingTrays.ToList())
            { 
                if (BlueCrystalCookingPlugin.Instance.Configuration.Instance.FreezerNeedsPower)
                {
                    foreach (var generator in PowerTool.checkGenerators(tray.Pos, PowerTool.MAX_POWER_RANGE, ushort.MaxValue))
                    {
                        if (generator.fuel <= 0 || !generator.isPowered || !(generator.wirerange >=
                                                                             (tray.Pos - generator.transform.position)
                                                                             .magnitude)) continue;
                        tray.FreezingSeconds += 1;
                        if (tray.FreezingSeconds < BlueCrystalCookingPlugin.Instance.Configuration.Instance
                                .BlueCrystalTrayFreezingTimeSecs) continue;
                        if (!BarricadeManager.tryGetInfo(tray.Transform, out var x, out var y, out var plant,
                                out var index, out var region)) continue;
                        BarricadeManager.destroyBarricade(region, x, y, plant, index);
                        BarricadeManager.dropBarricade(new Barricade(BlueCrystalCookingPlugin.Instance.Configuration.Instance.FrozenTrayId), null, tray.Pos, tray.AngleX, tray.AngleY, tray.AngleZ, tray.Owner, tray.Group);
                        if (BlueCrystalCookingPlugin.Instance.Configuration.Instance.EnableBlueCrystalFreezeEffect)
                        {
                            EffectManager.sendEffect(BlueCrystalCookingPlugin.Instance.Configuration.Instance.BlueCrystalFreezeEffectId, 10, tray.Pos);
                        }
                        BlueCrystalCookingPlugin.Instance.FreezingTrays.Remove(tray);
                    }
                } else
                {
                    tray.FreezingSeconds += 1;
                    if (tray.FreezingSeconds < BlueCrystalCookingPlugin.Instance.Configuration.Instance
                            .BlueCrystalTrayFreezingTimeSecs) continue;
                    if (!BarricadeManager.tryGetInfo(tray.Transform, out var x, out var y, out var plant, out var index,
                            out var region)) continue;
                    BarricadeManager.destroyBarricade(region, x, y, plant, index);
                    BarricadeManager.dropBarricade(new Barricade(BlueCrystalCookingPlugin.Instance.Configuration.Instance.FrozenTrayId), null, tray.Pos, tray.AngleX, tray.AngleY, tray.AngleZ, tray.Owner, tray.Group);
                    if (BlueCrystalCookingPlugin.Instance.Configuration.Instance.EnableBlueCrystalFreezeEffect)
                    {
                        EffectManager.sendEffect(BlueCrystalCookingPlugin.Instance.Configuration.Instance.BlueCrystalFreezeEffectId, 10, tray.Pos);
                    }
                    BlueCrystalCookingPlugin.Instance.FreezingTrays.Remove(tray);
                }
            }
        }
    }
}

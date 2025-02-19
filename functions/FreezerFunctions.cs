﻿using SDG.Unturned;
using System.Linq;
using UnityEngine;

namespace Ocelot.BlueCrystalCooking.functions
{
    public static class FreezerFunctions
    {
        public static void BarricadeDeployed(Barricade barricade, ItemBarricadeAsset asset, Transform hit, ref Vector3 point, ref float angle_x, ref float angle_y, ref float angle_z, ref ulong owner, ref ulong group, ref bool shouldAllow)
        {
            Vector3 pos = point;
            if (barricade.id == BlueCrystalCookingPlugin.Instance.Configuration.Instance.LiquidTrayId)
            {
                if (Physics.Raycast(pos, Vector3.down, out RaycastHit raycastHit, 10, RayMasks.BARRICADE))
                {
                    if (Physics.Raycast(pos, Vector3.up, out RaycastHit raycastHitUp, 10, RayMasks.BARRICADE))
                    {
                        if (raycastHit.transform == raycastHitUp.transform)
                        {
                            BarricadeManager.tryGetInfo(raycastHit.transform, out byte x, out byte y, out ushort plant, out ushort index, out BarricadeRegion region, out BarricadeDrop drop);
                            if (drop.asset.id == BlueCrystalCookingPlugin.Instance.Configuration.Instance.FreezerId)
                            {
                                ulong ownerTray = owner;
                                ulong groupTray = group;
                                float angle_x_tray = angle_x;
                                float angle_y_tray = angle_y;
                                float angle_z_tray = angle_z;
                                BlueCrystalCookingPlugin.Instance.Wait(0.5f, () =>
                                {
                                    Transform tray = BlueCrystalCookingPlugin.Instance.GetPlacedObjectTransform(pos);
                                    BlueCrystalCookingPlugin.Instance.freezingTrays.Add(new FreezingTrayObject(tray, pos, ownerTray, groupTray, angle_x_tray, angle_y_tray, angle_z_tray, 0));
                                });
                            }
                        }
                    }
                }
            }
        }
        public static void Update()
        {
            foreach (var tray in BlueCrystalCookingPlugin.Instance.freezingTrays.ToList())
            { 
                if (BlueCrystalCookingPlugin.Instance.Configuration.Instance.FreezerNeedsPower)
                {
                    foreach (var Generator in PowerTool.checkGenerators(tray.pos, PowerTool.MAX_POWER_RANGE, ushort.MaxValue))
                    {
                        if (Generator.fuel > 0 && Generator.isPowered && Generator.wirerange >= (tray.pos - Generator.transform.position).magnitude)
                        {
                            tray.freezingSeconds += 1;
                            if (tray.freezingSeconds >= BlueCrystalCookingPlugin.Instance.Configuration.Instance.BlueCrystalTrayFreezingTimeSecs)
                            {
                                if (BarricadeManager.tryGetInfo(tray.transform, out byte x, out byte y, out ushort plant, out ushort index, out BarricadeRegion region))
                                {
                                    BarricadeManager.destroyBarricade(region, x, y, plant, index);
                                    BarricadeManager.dropBarricade(new Barricade(BlueCrystalCookingPlugin.Instance.Configuration.Instance.FrozenTrayId), null, tray.pos, tray.angle_x, tray.angle_y, tray.angle_z, tray.owner, tray.group);
                                    if (BlueCrystalCookingPlugin.Instance.Configuration.Instance.EnableBlueCrystalFreezeEffect)
                                    {
                                        EffectManager.sendEffect(BlueCrystalCookingPlugin.Instance.Configuration.Instance.BlueCrystalFreezeEffectId, 10, tray.pos);
                                    }
                                    BlueCrystalCookingPlugin.Instance.freezingTrays.Remove(tray);
                                }
                            }
                        }
                    }
                } else
                {
                    tray.freezingSeconds += 1;
                    if (tray.freezingSeconds >= BlueCrystalCookingPlugin.Instance.Configuration.Instance.BlueCrystalTrayFreezingTimeSecs)
                    {
                        if (BarricadeManager.tryGetInfo(tray.transform, out byte x, out byte y, out ushort plant, out ushort index, out BarricadeRegion region))
                        {
                            BarricadeManager.destroyBarricade(region, x, y, plant, index);
                            BarricadeManager.dropBarricade(new Barricade(BlueCrystalCookingPlugin.Instance.Configuration.Instance.FrozenTrayId), null, tray.pos, tray.angle_x, tray.angle_y, tray.angle_z, tray.owner, tray.group);
                            if (BlueCrystalCookingPlugin.Instance.Configuration.Instance.EnableBlueCrystalFreezeEffect)
                            {
                                EffectManager.sendEffect(BlueCrystalCookingPlugin.Instance.Configuration.Instance.BlueCrystalFreezeEffectId, 10, tray.pos);
                            }
                            BlueCrystalCookingPlugin.Instance.freezingTrays.Remove(tray);
                        }
                    }
                }
            }
        }
    }
}

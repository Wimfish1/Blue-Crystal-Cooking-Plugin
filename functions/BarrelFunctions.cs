using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using Random = System.Random;

namespace Ocelot.BlueCrystalCooking.functions
{
    public static class BarrelFunctions
    {
        public static void OnGestureChanged(UnturnedPlayer player, EPlayerGesture gesture)
        {
            if (player == null)
                return;
            if (!Physics.Raycast(player.Player.look.aim.position, player.Player.look.aim.forward,
                    out var raycastHit, 2, RayMasks.BARRICADE)) return;
            foreach (var barrel in BlueCrystalCookingPlugin.Instance.PlacedBarrelsTransformsIngredients.ToList())
            {
                if (!barrel.Key || !raycastHit.transform)
                    break;
                if (barrel.Key.position == raycastHit.transform.position)
                {
                    uint ingredientCount = 0;
                    for (int i = 0; i < BlueCrystalCookingPlugin.Instance.Configuration.Instance.DrugIngredientIds.Count; i++)
                    {
                        if (barrel.Value.Ingredients.Contains(BlueCrystalCookingPlugin.Instance.Configuration.Instance.DrugIngredientIds[i]))
                        {
                            ingredientCount += 1;
                        }
                    }
                    if (ingredientCount == BlueCrystalCookingPlugin.Instance.Configuration.Instance.DrugIngredientIds.Count)
                    {
                        if (BlueCrystalCookingPlugin.Instance.Configuration.Instance.EnableBarrelStirEffect)
                        {
                            ingredientCount = 0;
                            if (raycastHit.transform)
                            {
                                EffectManager.sendEffect(
                                    BlueCrystalCookingPlugin.Instance.Configuration.Instance.BarrelStirEffectId, 4,
                                    raycastHit.transform.position);
                            }
                        }
                        barrel.Value.Progress += BlueCrystalCookingPlugin.Instance.Configuration.Instance.StirProgressAddPercentage;
                    }
                    else
                    {
                        ingredientCount = 0;
                        ChatManager.serverSendMessage(
                            BlueCrystalCookingPlugin.Instance.Translate("not_enough_ingredients"), Color.white, null,
                            player.SteamPlayer(), EChatMode.SAY,
                            BlueCrystalCookingPlugin.Instance.Configuration.Instance.IconImageUrl, true);
                    }
                }

                if (barrel.Value.Progress < 100) continue;
                {
                    barrel.Value.Progress = 0;
                    for (var i = 0; i < BlueCrystalCookingPlugin.Instance.Configuration.Instance.DrugIngredientIds.Count; i++)
                    {
                        barrel.Value.Ingredients.Remove(BlueCrystalCookingPlugin.Instance.Configuration.Instance.DrugIngredientIds[i]);
                    }

                    if (BlueCrystalCookingPlugin.Instance.Configuration.Instance.RequireGasMask)
                    {
                        if (player.Player.clothing.hat != BlueCrystalCookingPlugin.Instance.Configuration.Instance.GasMaskId)
                        {
                            // Define the chance of blowing up (e.g., 20% chance)
                            float blowUpChance = BlueCrystalCookingPlugin.Instance.Configuration.Instance.GasChance; // 20% chance
                            Random random = new Random();
                            float randomValue = (float)random.NextDouble(); // Generates a value between 0.0 and 1.0
                    
                            if (randomValue < blowUpChance)
                            {
                                ChatManager.serverSendMessage(BlueCrystalCookingPlugin.Instance.Translate("lethal_gas"),
                                    Color.white, null, player.SteamPlayer(), EChatMode.SAY,
                                    BlueCrystalCookingPlugin.Instance.Configuration.Instance.IconImageUrl, true);
                                player.Infection = 100;
                                EffectManager.sendEffect(20, 4, raycastHit.transform.position);
                                player.Damage(90, new Vector3(0, 0, 0), EDeathCause.INFECTION, ELimb.SKULL, new CSteamID());
                            }
                        }
                    }
                    ChatManager.serverSendMessage(BlueCrystalCookingPlugin.Instance.Translate("stir_successful"),
                        Color.white, null, player.SteamPlayer(), EChatMode.SAY,
                        BlueCrystalCookingPlugin.Instance.Configuration.Instance.IconImageUrl, true);
                    BarricadeManager.dropBarricade(
                        new Barricade(BlueCrystalCookingPlugin.Instance.Configuration.Instance.BlueCrystalTrayId), null,
                        player.Position, 0, 0, 0, (ulong)player.CSteamID, (ulong)player.Player.quests.groupID);
                }
            }
        }
        
        public static void BarricadeDeployed(Barricade barricade, ItemBarricadeAsset asset, Transform hit,
            ref Vector3 point, ref float angleX, ref float angleY, ref float angleZ, ref ulong owner, ref ulong group,
            ref bool shouldAllow)
        {
            var pos = point;
            var ownerBarricade = owner;
            if (barricade.id == BlueCrystalCookingPlugin.Instance.Configuration.Instance.BarrelObjectId)
            {
                BlueCrystalCookingPlugin.Instance.Wait(0.2f, () => {
                    var ingredients = new List<ushort>();
                    BlueCrystalCookingPlugin.Instance.PlacedBarrelsTransformsIngredients.Add(
                        BlueCrystalCookingPlugin.Instance.GetPlacedObjectTransform(pos),
                        new BarrelObject(ingredients, 0));
                });
            }

            foreach (var drugObject in BlueCrystalCookingPlugin.Instance.Configuration.Instance.DrugIngredientIds)
            {
                if (drugObject != barricade.id) continue;
                if (!Physics.Raycast(pos, Vector3.down, out RaycastHit raycastHit, 10, RayMasks.BARRICADE)) continue;
                if (!BarricadeManager.tryGetInfo(raycastHit.transform, out byte x, out byte y, out ushort plant,
                        out var index, out var region, out var drop)) continue;
                if (drop.asset.id != BlueCrystalCookingPlugin.Instance.Configuration.Instance.BarrelObjectId) continue;
                foreach (var barrel in BlueCrystalCookingPlugin.Instance.PlacedBarrelsTransformsIngredients.ToList())
                {
                    if (barrel.Key != raycastHit.transform) continue;
                    ChatManager.serverSendMessage(
                        BlueCrystalCookingPlugin.Instance.Translate("ingredient_added", asset.itemName), Color.white,
                        null, UnturnedPlayer.FromCSteamID(new CSteamID(ownerBarricade)).SteamPlayer(), EChatMode.SAY,
                        BlueCrystalCookingPlugin.Instance.Configuration.Instance.IconImageUrl, true);
                    barrel.Value.Ingredients.Add(barricade.id);
                    BlueCrystalCookingPlugin.Instance.Wait(0.2f, () => {
                        BarricadeManager.tryGetInfo(BlueCrystalCookingPlugin.Instance.GetPlacedObjectTransform(pos),
                            out var xingredient, out var yingredient, out var plantingredient, out var indexingredient,
                            out var regioningredient);
                        BarricadeManager.destroyBarricade(regioningredient, xingredient, yingredient, plantingredient,
                            indexingredient);
                    });
                }
            }
        }
    }
}

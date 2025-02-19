using Ocelot.BlueCrystalCooking.functions;
using Rocket.API.Collections;
using Rocket.Core.Plugins;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Logger = Rocket.Core.Logging.Logger;

namespace Ocelot.BlueCrystalCooking
{
    public class BlueCrystalCookingPlugin : RocketPlugin<BlueCrystalCookingConfiguration>
    {
        public static BlueCrystalCookingPlugin Instance;
        private const string Version = "1.1.2";

        private int _frame = 0;
        public long timer = 0;
        public Dictionary<Transform, BarrelObject> PlacedBarrelsTransformsIngredients = new Dictionary<Transform, BarrelObject>();
        public List<DrugeffectTimeObject> DrugeffectPlayersList = new List<DrugeffectTimeObject>();
        public List<FreezingTrayObject> FreezingTrays = new List<FreezingTrayObject>();

        protected override void Load()
        {
            Instance = this;
            Logger.Log("BlueCrystalCookingPlugin v" + Version, ConsoleColor.Yellow);
            Logger.Log("Original Plugin By: Ocelot");
            Logger.Log("Edited & Maintained by: Wimfish1", ConsoleColor.Blue);

            BarricadeManager.onDeployBarricadeRequested += BarricadeDeployed;
            BarricadeManager.onSalvageBarricadeRequested += BarricadeSalvaged;
            //UnturnedPlayerEvents.OnPlayerUpdateGesture += OnPlayerUpdateGesture;
            PlayerAnimator.OnGestureChanged_Global += OnGestureChanged;
            UseableConsumeable.onConsumePerformed += ConsumeAction;
            BarricadeManager.onDamageBarricadeRequested += BarricadeDamaged;
            if (Level.isLoaded)
            {
                AddExistingBarrels(1);
            } else
            {
                Level.onLevelLoaded += AddExistingBarrels;
            }
        }

        private void BarricadeSalvaged(CSteamID steamID, byte x, byte y, ushort plant, ushort index, ref bool shouldAllow)
        {
            BarricadeManager.tryGetRegion(x, y, plant, out BarricadeRegion region);
            foreach (var drop in region.drops.ToList())
            {
                if (drop.asset.id == Configuration.Instance.BarrelObjectId)
                {
                    BarricadeData barricade = region.findBarricadeByInstanceID(drop.instanceID);
                    foreach (var item in PlacedBarrelsTransformsIngredients.ToList())
                    {
                        if (item.Key.position == barricade.point)
                        {
                            BarricadeManager.tryGetInfo(GetPlacedObjectTransform(barricade.point), out byte xBarricade, out byte yBarricade, out ushort plantBarricade, out ushort indexBarricade, out BarricadeRegion regionBarricade);
                            if (x == xBarricade && y == yBarricade && plant == plantBarricade && index == indexBarricade)
                            {
                                PlacedBarrelsTransformsIngredients.Remove(GetPlacedObjectTransform(barricade.point));
                            }
                        }
                    }
                }
            }
        }

        protected override void Unload()
        {
            BarricadeManager.onDeployBarricadeRequested -= BarricadeDeployed;
            BarricadeManager.onSalvageBarricadeRequested -= BarricadeSalvaged;
            PlayerAnimator.OnGestureChanged_Global -= OnGestureChanged;
            //UnturnedPlayerEvents.OnPlayerUpdateGesture -= OnPlayerUpdateGesture;
            UseableConsumeable.onConsumePerformed -= ConsumeAction;
            BarricadeManager.onDamageBarricadeRequested -= BarricadeDamaged;
        }
        
        private void ConsumeAction(Player instigatingPlayer, ItemConsumeableAsset consumeableAsset)
        {
            MethBagFunctions.ConsumeAction(instigatingPlayer, consumeableAsset);
        }

        private void OnGestureChanged(PlayerAnimator arg1, EPlayerGesture gesture)
        {
            if (gesture == EPlayerGesture.PUNCH_LEFT || gesture == EPlayerGesture.PUNCH_RIGHT)
            {
                BarrelFunctions.OnGestureChanged(UnturnedPlayer.FromPlayer(arg1.player), gesture);
                MethBagFunctions.OnGestureChanged(UnturnedPlayer.FromPlayer(arg1.player), gesture);
            }
                
        }

        //private void OnPlayerUpdateGesture(UnturnedPlayer player, UnturnedPlayerEvents.PlayerGesture gesture)
        //{
        //    BarrelFunctions.OnPlayerUpdateGesture(player, gesture);
        //    MethBagFunctions.OnPlayerUpdateGesture(player, gesture);
        //}
        
        private void BarricadeDeployed(Barricade barricade, ItemBarricadeAsset asset, Transform hit, ref Vector3 point, ref float angleX, ref float angleY, ref float angleZ, ref ulong owner, ref ulong group, ref bool shouldAllow)
        {
            BarrelFunctions.BarricadeDeployed(barricade, asset, hit, point: ref point, angleX: ref angleX, angleY: ref angleY, angleZ: ref angleZ, owner: ref owner, group: ref group, shouldAllow: ref shouldAllow);
            FreezerFunctions.BarricadeDeployed(barricade, asset, hit, point: ref point, angleX: ref angleX, angleY: ref angleY, angleZ: ref angleZ, owner: ref owner, group: ref group, shouldAllow: ref shouldAllow);
        }

        private void BarricadeDamaged(CSteamID instigatorSteamID, Transform barricadeTransform, ref ushort pendingTotalDamage, ref bool shouldAllow, EDamageOrigin damageOrigin)
        {
            BarricadeFunctions.BarricadeDamaged(barricadeTransform, pendingTotalDamage);
        }

        public static Dictionary<Vector3, Transform> GetAllObjects()
        {
            Dictionary<Vector3, Transform> objectsOnMap = new Dictionary<Vector3, Transform>();
            foreach (var region in BarricadeManager.regions)
            {
                foreach (var drop in region.drops)
                {
                    if (!objectsOnMap.ContainsKey(drop.model.position))
                    {
                        objectsOnMap.Add(drop.model.position, drop.model);
                    }
                }
            }
            return objectsOnMap;
        }

        public Transform GetPlacedObjectTransform(Vector3 objectPosition)
        {
            Dictionary<Vector3, Transform> objectsOnMap;
            objectsOnMap = GetAllObjects();

            foreach (var mapObject in objectsOnMap.ToList())
            {
                if (mapObject.Key == objectPosition)
                {
                    return mapObject.Value;
                }
            }
            return null; //Never happens
        }
        public BarricadeData GetBarricadeDataAtPosition(Vector3 position)
        {
            foreach (var region in BarricadeManager.regions)
            {
                foreach (var b in region.barricades)
                {
                    if (b.point == position) return b;
                }
            }
            return null;
        }

        private void AddExistingBarrels(int level)
        {
            Logger.Log("Adding map barrels to list...", ConsoleColor.Green);
            List<ushort> ingredientsStandard = new List<ushort>();
            foreach (var region in BarricadeManager.regions)
            {
                foreach (var drop in region.drops)
                {
                    if (drop.asset.id == Configuration.Instance.BarrelObjectId)
                    {
                        BarricadeData barricade = region.findBarricadeByInstanceID(drop.instanceID);
                        Transform barrelTransform = GetPlacedObjectTransform(barricade.point);
                        if (PlacedBarrelsTransformsIngredients.ContainsKey(barrelTransform))
                        {
                            Logger.Log("Duplicated entry detected, skipping object. (No need to worry)", ConsoleColor.Yellow);
                        } else
                        {
                            PlacedBarrelsTransformsIngredients.Add(barrelTransform, new BarrelObject(ingredientsStandard, 0));
                        }
                    }
                }
            }
            Logger.Log("All barrels added.", ConsoleColor.Green);
        }

        public override TranslationList DefaultTranslations => new TranslationList
        {
            {"not_enough_ingredients", "There are <color=#ff3c19>not enough ingredients</color> in the barrel to stir them into blue crystal." },
            {"ingredient_added", "You have <color=#75ff19>added {0}</color> to the barrel." },
            {"stir_successful", "You have <color=#75ff19>successfully mixed</color> the ingredients into a tray filled with <color=#1969ff>liquid blue crystal</color>." },
            {"bluecrystalbags_obtained", "You have <color=#75ff19>successfully obtained {0} bags</color> filled with <color=#1969ff>blue crystal</color>." }
        };

        private void Update()
        {
            _frame++;
            if (_frame % 5 != 0) return; // BRICHT METHODE AB WENN DER FRAME NICHT DURCH 5 TEILBAR IST
            // DO STUFF EVERY GAME FRAME E.G 60/s

            if (GetCurrentTime() - timer >= 1)
            {
                timer = GetCurrentTime();
                MethBagFunctions.Update();
                FreezerFunctions.Update();
            }
            
        }

        public static Int32 GetCurrentTime()
        {
            return (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
        }

        public void Wait(float seconds, System.Action action)
        {
            StartCoroutine(_wait(seconds, action));
        }
        IEnumerator _wait(float time, System.Action callback)
        {
            yield return new WaitForSeconds(time);
            callback();
        }
    }
}
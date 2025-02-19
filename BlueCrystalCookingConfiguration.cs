using Rocket.API;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Ocelot.BlueCrystalCooking
{
    public class BlueCrystalCookingConfiguration : IRocketPluginConfiguration
    {
        // CONFIG VARIABLES
        public string IconImageUrl;
        public ushort BarrelObjectId;
        public ushort BlueCrystalBagId;
        public bool UseDrugEffectSpeed;
        public float DrugEffectSpeedMultiplier;
        public bool UseDrugEffectJump;
        public float DrugEffectJumpMultiplier;
        public ushort BlueCrystalTrayId;
        public ushort FrozenTrayId;
        public ushort LiquidTrayId;
        public ushort FreezerId;
        public int DrugEffectDurationSecs;
        public int BlueCrystalTrayFreezingTimeSecs;
        public ushort BlueCrystalFreezeEffectId;
        public bool EnableBlueCrystalFreezeEffect;
        public ushort BarrelStirEffectId;
        public bool EnableBarrelStirEffect;
        public uint StirProgressAddPercentage;
        public int BlueCrystalBagsAmountMin;
        public int BlueCrystalBagsAmountMax;
        [XmlArrayItem(ElementName = "Id")]
        public List<ushort> drugIngredientIds;
        public bool FreezerNeedsPower;
        public bool AddItemsDirectlyToInventory;

        public void LoadDefaults()
        {
            IconImageUrl = "https://i.imgur.com/fpl2UL7.png";
            BarrelObjectId = 10102;
            drugIngredientIds = new List<ushort>() { 10103, 10104, 10105 };
            BlueCrystalBagId = 10108;
            UseDrugEffectSpeed = true;
            DrugEffectSpeedMultiplier = 2;
            UseDrugEffectJump = true;
            DrugEffectJumpMultiplier = 2;
            DrugEffectDurationSecs = 30;
            BlueCrystalTrayId = 10106;
            FrozenTrayId = 10107;
            LiquidTrayId = 10106;
            FreezerId = 10101;
            BlueCrystalTrayFreezingTimeSecs = 45;
            EnableBlueCrystalFreezeEffect = true;
            BlueCrystalFreezeEffectId = 63;
            EnableBarrelStirEffect = true;
            BarrelStirEffectId = 76;
            StirProgressAddPercentage = 10;
            BlueCrystalBagsAmountMin = 1;
            BlueCrystalBagsAmountMax = 3;
            FreezerNeedsPower = true;
            AddItemsDirectlyToInventory = true;
        }
    }
}

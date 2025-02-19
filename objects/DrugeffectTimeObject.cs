namespace Ocelot.BlueCrystalCooking
{
    public class DrugeffectTimeObject
    {
        public long time = BlueCrystalCookingPlugin.getCurrentTime();
        public string playerId;
        public DrugeffectTimeObject(string playerId)
        {
            this.playerId = playerId;
        }
    }
}

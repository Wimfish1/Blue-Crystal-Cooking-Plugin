namespace Ocelot.BlueCrystalCooking
{
    public class DrugeffectTimeObject
    {
        public long Time = BlueCrystalCookingPlugin.GetCurrentTime();
        public string PlayerId;
        public DrugeffectTimeObject(string playerId)
        {
            this.PlayerId = playerId;
        }
    }
}

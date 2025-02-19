using System.Collections.Generic;

namespace Ocelot.BlueCrystalCooking
{
    public class BarrelObject
    {
        public List<ushort> Ingredients = new List<ushort>();
        public uint Progress = 0;
        public BarrelObject(List<ushort> ingredients, uint progress)
        {
            this.Ingredients = ingredients;
            this.Progress = progress;
        }
    }
}

using UnityEngine;

namespace Ocelot.BlueCrystalCooking
{
    public class FreezingTrayObject
    {
        public Transform Transform;
        public ulong Owner;
        public ulong Group;
        public float AngleX;
        public float AngleY;
        public float AngleZ;
        public Vector3 Pos;
        public int FreezingSeconds;
        public FreezingTrayObject(Transform transform, Vector3 pos, ulong owner, ulong group, float angleX, float angleY, float angleZ, int freezingSeconds)
        {
            this.Transform = transform;
            this.Pos = pos;
            this.Owner = owner;
            this.Group = group;
            this.AngleX = angleX;
            this.AngleY = angleY;
            this.AngleZ = angleZ;
            this.FreezingSeconds = freezingSeconds;
        }
    }
}

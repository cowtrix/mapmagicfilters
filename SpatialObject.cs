using System;
using UnityEngine;

namespace MapMagic
{
    [System.Serializable]
    public class SpatialObject //TODO: to struct
    {
        public Vector2 pos;
        public float height;
        public float rotation;
        public Vector3 size;
        public int type;
        public int id; //unique num to apply random

        public float sizeScalar
        {
            get { return (Math.Abs(size.x) + Math.Abs(size.y) + Math.Abs(size.z)) / 3; }
        }

        public SpatialObject Copy() { return new SpatialObject() { pos = this.pos, height = this.height, rotation = this.rotation, size = this.size, type = this.type, id = this.id }; }
    }
}
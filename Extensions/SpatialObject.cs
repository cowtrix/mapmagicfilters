using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MapMagic
{
    [System.Serializable]
    public class SpatialObject //TODO: to struct
    {
        public List<StringTuple> Tags = new List<StringTuple>();
        public Vector3 size;
        public Quaternion rotation;

        public Vector2 pos;
        public float height;

        public float ySpin;
        
        public int type;
        public int id; //unique num to apply random

        public float sizeScalar
        {
            get { return (Math.Abs(size.x) + Math.Abs(size.y) + Math.Abs(size.z)) / 3; }
        }

        public SpatialObject Copy() { return new SpatialObject() { pos = this.pos, height = this.height, rotation = this.rotation, size = this.size, type = this.type, id = this.id, Tags = this.Tags.ToList()}; }
    }
}
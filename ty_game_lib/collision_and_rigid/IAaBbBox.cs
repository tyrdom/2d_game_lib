using System.Collections.Generic;

namespace collision_and_rigid
{
    public interface IAaBbBox
    {
        public IShape GetShape();

        public Zone Zone { get; set; }
        public List<(int, IAaBbBox)> SplitByQuads(float horizon, float vertical);
    }
}
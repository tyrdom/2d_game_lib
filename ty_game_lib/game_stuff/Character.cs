using System.Drawing;
using collision_and_rigid;

namespace game_stuff
{
    public class Character
    {
        public int Team;

        public Body Body;
        public BodySize Size;
        public TwoDPoint LastPos;
        public AngleSight Sight;
        
        
    }

    public class Body : IShape
    {
        public Round Rd;
        public Character Master;

        public Body(Round rd, Character master)
        {
            this.Rd = rd;
            Master = master;
        }

        public AabbBoxShape CovToAabbPackBox()
        {
            var zones = Rd.GetZones();
            return new AabbBoxShape(zones, this);
        }

        public int TouchByRightShootPointInAAbbBox(TwoDPoint p)
        {
            throw new System.NotImplementedException();
        }

        public bool IsTouchAnother(IShape another)
        {
            var isTouchAnother = Rd.IsTouchAnother(another);
            return isTouchAnother;
        }
    }
}
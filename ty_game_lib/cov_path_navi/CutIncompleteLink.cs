using collision_and_rigid;

namespace cov_path_navi
{
    public class CutIncompleteLink
    {
        public readonly Link Link;
        public readonly TwoDVectorLine Mirror;
        public int Belong;

        public CutIncompleteLink(Link link, TwoDVectorLine mirror)
        {
            Link = link;
            Mirror = mirror;
            Belong = -1;
        }

        public bool IsComplete()
        {
            return Link.LinkToPathNodeId >= 0 && Belong >= 0;
        }
    }
}
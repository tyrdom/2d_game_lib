using collision_and_rigid;

namespace cov_path_navi
{
    public class Link
    {
        public int LinkToPathNodeId;

        public TwoDVectorLine GoThrough;

        public Link(int linkToPathNodeId, TwoDVectorLine goThrough)
        {
            LinkToPathNodeId = linkToPathNodeId;
            GoThrough = goThrough;
        }

        public override string ToString()
        {
            return $"link to ::{LinkToPathNodeId} go through" + GoThrough.Log();
        }
    }

    public class CutIncompleteLink
    {
        public Link Link;
        public TwoDVectorLine Mirror;
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
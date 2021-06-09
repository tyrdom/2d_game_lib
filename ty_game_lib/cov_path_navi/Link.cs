using collision_and_rigid;

namespace cov_path_navi
{
    public class Link
    {
        public int LinkToPathNodeId { get; set; }

        public TwoDVectorLine GoThrough { get; }

        public Link(int linkToPathNodeId, TwoDVectorLine goThrough)
        {
            LinkToPathNodeId = linkToPathNodeId;
            GoThrough = goThrough;
        }

        public override string ToString()
        {
            return $"link to ::{LinkToPathNodeId} go through" + GoThrough;
        }
    }
}
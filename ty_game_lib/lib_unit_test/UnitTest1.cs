using System.Linq;
using NUnit.Framework;

namespace lib_unit_test
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test1()
        {
            var genAChapterMap = rogue_chapter_maker.ChapterMapTop.GenAChapterMap(7, 3);
            var selectMany = genAChapterMap.PointMaps.SelectMany(x => x.ToString());
            var s = new string(selectMany.ToArray());
            Assert.Pass(s);
        }

        [Test]
        public void Test2()
        {
            var genAChapterMap = rogue_chapter_maker.ChapterMapTop.GenAChapterMap(7, 3);

            Assert.Pass(genAChapterMap.ToString());
        }
    }
}
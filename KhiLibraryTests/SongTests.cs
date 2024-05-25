using Microsoft.VisualStudio.TestTools.UnitTesting;
using KhiLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KhiLibrary.Tests
{
    [TestClass()]
    public class SongTests
    {
        [TestMethod()]
        public void SongTest()
        {
            Assert.IsNull(new Song());
        }

        [TestMethod()]
        public void SongTest1()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void SongTest2()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void SetArtistTest()
        {
            Assert.Fail();
        }
    }
}
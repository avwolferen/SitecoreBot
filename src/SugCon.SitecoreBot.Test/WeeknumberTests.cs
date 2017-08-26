using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SugCon.SitecoreBot.Test
{
    [TestClass]
    public class WeeknumberTests
    {
        [TestMethod]
        public void FirstDayOfWeek()
        {
            var date = Helpers.DateHelpers.FirstDateOfWeek(2017, 18);
            Assert.AreEqual(1, date.Day);
        }

        [TestMethod]
        public void LastDayOfWeek()
        {
            var date = Helpers.DateHelpers.LastDateOfWeek(2017, 18);
            Assert.AreEqual(7, date.Day);
        }
    }
}

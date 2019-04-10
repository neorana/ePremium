using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Utilities;

namespace BusinessLayer.Tests
{
    [TestClass()]
    public class ProcessTimeTests
    {

        [TestMethod()]
        public void TimeIn()
        {
            //Arrange
            var timeTest = DateTime.Parse("2017-07-31 08:01:00");

            //Act
            var result = DateTime.Parse("2017-07-31 08:00:00");

            var actual = timeTest.GetRoundTime(5);

            //Assert
            Assert.AreEqual(result, actual); //haha
        }


    }
}
using System;
using System.Collections.Generic;
using System.Text;
using CCMEngine;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CCMTests
{
    [TestClass]
    public class SortedListenerTests
    {
        [TestMethod]
        public void SingleItemStored()
        {
            SortedListener l = new SortedListener(1, new List<string>(), 0);

            l.OnMetric(new ccMetric("a.cpp", "Foo::Bar", 3, 0), null);
            //l.OnMetric("Foo::Bar", 3, "a.cpp", 1, 1);

            Assert.AreEqual("Foo::Bar", l.Metrics[0].Unit);
            Assert.AreEqual(3, l.Metrics[0].CCM);
        }

        [TestMethod]
        public void CanContainMultipleMetricsWithSameCCM()
        {
            SortedListener l = new SortedListener(2, new List<string>(), 0);

            l.OnMetric(new ccMetric("a.cpp", "Foo::Bar", 3, 0), null);
            l.OnMetric(new ccMetric("b.cpp", "Foo::Bar2", 3, 0), null);

            Assert.AreEqual(2, l.Metrics.Count);

            Assert.AreEqual(3, l.Metrics[0].CCM);
            Assert.AreEqual(3, l.Metrics[1].CCM);
        }

        [TestMethod]
        public void MultiItemsAreSorted()
        {
            SortedListener l = new SortedListener(3, new List<string>(), 0);

            l.OnMetric(new ccMetric("a.cpp", "Foo::Bar3", 3, 0), null);
            l.OnMetric(new ccMetric("a.cpp", "Foo::Bar1", 1, 0), null);
            l.OnMetric(new ccMetric("a.cpp", "Foo::Bar7", 7, 0), null);

            Assert.AreEqual("Foo::Bar7", l.Metrics[0].Unit);
            Assert.AreEqual(7, l.Metrics[0].CCM);

            Assert.AreEqual("Foo::Bar3", l.Metrics[1].Unit);
            Assert.AreEqual(3, l.Metrics[1].CCM);

            Assert.AreEqual("Foo::Bar1", l.Metrics[2].Unit);
            Assert.AreEqual(1, l.Metrics[2].CCM);
        }

        [TestMethod]
        public void TestExcludes()
        {
            List<string> ignores = new List<string>();
            ignores.Add("Foo");
            ignores.Add("Bar");

            SortedListener l = new SortedListener(3, ignores, 0);

            l.OnMetric(new ccMetric("a.cpp", "Foo(int j)", 2, 0), null);
            l.OnMetric(new ccMetric("a.cpp", "Bar(double d, X& x)", 3, 0), null);

            Assert.AreEqual(0, l.Metrics.Count); // should all have been filtered away
        }

        [TestMethod]
        public void ExhaustedBufferHasCorrectItems()
        {
            SortedListener l = new SortedListener(3, new List<string>(), 0);

            l.OnMetric(new ccMetric("a.cpp", "Foo::Bar3", 3, 0), null);
            l.OnMetric(new ccMetric("a.cpp", "Foo::Bar1", 1, 0), null);
            l.OnMetric(new ccMetric("a.cpp", "Foo::Bar7", 7, 0), null);
            l.OnMetric(new ccMetric("a.cpp", "Foo::Bar9", 9, 0), null);

            Assert.AreEqual(l.Metrics.Count, 3);

            Assert.AreEqual("Foo::Bar9", l.Metrics[0].Unit);
            Assert.AreEqual(9, l.Metrics[0].CCM);

            Assert.AreEqual("Foo::Bar7", l.Metrics[1].Unit);
            Assert.AreEqual(7, l.Metrics[1].CCM);

            Assert.AreEqual("Foo::Bar3", l.Metrics[2].Unit);
            Assert.AreEqual(3, l.Metrics[2].CCM);
        }

        //TODO: Test ignore unit functionality (ignore specific methods)

        [TestMethod]
        public void TestMultipleItemsSameName()
        {
            SortedListener l = new SortedListener(10, new List<string>(), 0);

            l.OnMetric(new ccMetric("a.cpp", "Foo::Bar", 3, 0), null);
            l.OnMetric(new ccMetric("a.cpp", "Foo::Bar", 1, 0), null);
            l.OnMetric(new ccMetric("a.cpp", "Foo::Bar", 7, 0), null);

            Assert.AreEqual(l.Metrics.Count, 3);

            Assert.AreEqual("Foo::Bar", l.Metrics[0].Unit);
            Assert.AreEqual(7, l.Metrics[0].CCM);

            Assert.AreEqual("Foo::Bar", l.Metrics[1].Unit);
            Assert.AreEqual(3, l.Metrics[1].CCM);

            Assert.AreEqual("Foo::Bar", l.Metrics[2].Unit);
            Assert.AreEqual(1, l.Metrics[2].CCM);
        }

        [TestMethod]
        public void TestNoMetricsLeftAfterClear()
        {
            SortedListener l = new SortedListener(2, new List<string>(), 0);

            l.OnMetric(new ccMetric("a.cpp", "Foo::Bar", 3, 0), null);
            l.OnMetric(new ccMetric("b.cpp", "Foo::Bar2", 3, 0), null);

            Assert.AreEqual(2, l.Metrics.Count);

            l.Clear();

            Assert.AreEqual(0, l.Metrics.Count);
        }

        [TestMethod]
        public void TestIgnoresMetricBelowThreshold()
        {
            SortedListener l = new SortedListener(2, new List<string>(), 10);

            l.OnMetric(new ccMetric("a.cpp", "Foo::Bar", 3, null), null);
            l.OnMetric(new ccMetric("b.cpp", "Foo::Bar2", 10, null), null);

            Assert.AreEqual(1, l.Metrics.Count);
            Assert.AreEqual(10, l.Metrics[0].CCM);
        }

    }
}

#nullable enable
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnitTest.Fakes;

namespace UnitTest
{
    /// <summary>
    /// Enhetstester för IScriptSystem — testar FakeScriptSystem i isolation.
    /// </summary>
    [TestClass]
    public class ScriptSystemTests
    {
        [TestMethod]
        public void Tick_SingleCall_IncrementsTickCount()
        {
            var script = new FakeScriptSystem();
            script.Tick(0.016f);
            Assert.AreEqual(1, script.TickCount);
        }

        [TestMethod]
        public void Tick_MultipleCalls_CountsAll()
        {
            var script = new FakeScriptSystem();
            script.Tick(0.016f);
            script.Tick(0.016f);
            script.Tick(0.016f);
            Assert.AreEqual(3, script.TickCount);
        }

        [TestMethod]
        public void Tick_ZeroElapsed_StillCounts()
        {
            var script = new FakeScriptSystem();
            script.Tick(0f);
            Assert.AreEqual(1, script.TickCount);
        }
    }
}

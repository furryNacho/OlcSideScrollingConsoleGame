using Microsoft.VisualStudio.TestTools.UnitTesting;
using OlcSideScrollingConsoleGame.Models.Objects;
using UnitTest.Fakes;

namespace UnitTest
{
    /// <summary>
    /// Enhetstester för fiendehierarkin — verifierar att typbaserad dispatch
    /// och IsIndestructible fungerar korrekt utan spelmotor eller hårdvara.
    ///
    /// Objekt skapas via DI-konstruktorer med FakeAssets (returnerar null för
    /// alla sprites/items) — inga Aggregate.Instance-anrop, inga x86-DLL-laddningar.
    /// </summary>
    [TestClass]
    public class EnemyTypeTests
    {
        private static readonly FakeAssets Fake = new FakeAssets();

        // ─────────────────────────────────────────────
        // IsIndestructible — oförstörbarhetsflagga
        // ─────────────────────────────────────────────

        [TestMethod]
        public void DynamicCreatureEnemyIcicle_IsIndestructible_IsTrue()
        {
            var icicle = new DynamicCreatureEnemyIcicle(Fake);
            Assert.IsTrue(icicle.IsIndestructible,
                "Icicle-fienden ska vara oförstörbar — hoppspark ska inte döda den.");
        }

        [TestMethod]
        public void DynamicCreatureEnemyBoss_IsIndestructible_IsFalse()
        {
            var boss = new DynamicCreatureEnemyBoss(Fake);
            Assert.IsFalse(boss.IsIndestructible,
                "Bossen är inte oförstörbar — den ska kunna ta hoppsparks-skada.");
        }

        [TestMethod]
        public void DynamicCreatureEnemyWalrus_IsIndestructible_IsFalse()
        {
            var walrus = new DynamicCreatureEnemyWalrus(Fake);
            Assert.IsFalse(walrus.IsIndestructible);
        }

        [TestMethod]
        public void DynamicCreatureEnemyFrost_IsIndestructible_IsFalse()
        {
            var frost = new DynamicCreatureEnemyFrost(Fake);
            Assert.IsFalse(frost.IsIndestructible);
        }

        [TestMethod]
        public void DynamicCreatureEnemyPenguin_IsIndestructible_IsFalse()
        {
            var penguin = new DynamicCreatureEnemyPenguin(Fake);
            Assert.IsFalse(penguin.IsIndestructible);
        }

        // ─────────────────────────────────────────────
        // Typhierarki — är subklasser korrekt arvade?
        // ─────────────────────────────────────────────

        [TestMethod]
        public void DynamicCreatureEnemyWalrus_IsCreature()
        {
            var walrus = new DynamicCreatureEnemyWalrus(Fake);
            Assert.IsInstanceOfType(walrus, typeof(Creature));
        }

        [TestMethod]
        public void DynamicCreatureEnemyFrost_IsCreature()
        {
            var frost = new DynamicCreatureEnemyFrost(Fake);
            Assert.IsInstanceOfType(frost, typeof(Creature));
        }

        [TestMethod]
        public void DynamicCreatureEnemyIcicle_IsCreature()
        {
            var icicle = new DynamicCreatureEnemyIcicle(Fake);
            Assert.IsInstanceOfType(icicle, typeof(Creature));
        }

        [TestMethod]
        public void DynamicCreatureEnemyBoss_IsCreature()
        {
            var boss = new DynamicCreatureEnemyBoss(Fake);
            Assert.IsInstanceOfType(boss, typeof(Creature));
        }

        [TestMethod]
        public void DynamicCreatureOverlayIce_IsCreature()
        {
            var overlay = new DynamicCreatureOverlayIce(Fake);
            Assert.IsInstanceOfType(overlay, typeof(Creature));
        }

        [TestMethod]
        public void DynamicCreatureOverlayIce_IsNotDynamicCreatureOverlay()
        {
            // OverlayIce och Overlay är separata typer — garanterar att
            // typbaserad dispatch skiljer dem åt korrekt.
            var ice = new DynamicCreatureOverlayIce(Fake);
            Assert.IsNotInstanceOfType(ice, typeof(DynamicCreatureOverlay),
                "DynamicCreatureOverlayIce ska inte ärva från DynamicCreatureOverlay.");
        }
    }
}

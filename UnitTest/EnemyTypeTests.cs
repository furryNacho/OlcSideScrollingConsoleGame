using Microsoft.VisualStudio.TestTools.UnitTesting;
using OlcSideScrollingConsoleGame.Models.Objects;

namespace UnitTest
{
    /// <summary>
    /// Enhetstester för fiendehierarkin — verifierar att typbaserad dispatch
    /// och IsIndestructible fungerar korrekt utan spelmotor eller hårdvara.
    ///
    /// Objekt skapas utan argument — sprites hanteras nu av SpriteId-enumet och
    /// PixelEngineRenderContext, inte av konstruktorparametrar. Inga DLL-laddningar.
    /// </summary>
    [TestClass]
    public class EnemyTypeTests
    {

        // ─────────────────────────────────────────────
        // IsIndestructible — oförstörbarhetsflagga
        // ─────────────────────────────────────────────

        [TestMethod]
        public void DynamicCreatureEnemyIcicle_IsIndestructible_IsTrue()
        {
            var icicle = new DynamicCreatureEnemyIcicle();
            Assert.IsTrue(icicle.IsIndestructible,
                "Icicle-fienden ska vara oförstörbar — hoppspark ska inte döda den.");
        }

        [TestMethod]
        public void DynamicCreatureEnemyBoss_IsIndestructible_IsFalse()
        {
            var boss = new DynamicCreatureEnemyBoss();
            Assert.IsFalse(boss.IsIndestructible,
                "Bossen är inte oförstörbar — den ska kunna ta hoppsparks-skada.");
        }

        [TestMethod]
        public void DynamicCreatureEnemyWalrus_IsIndestructible_IsFalse()
        {
            var walrus = new DynamicCreatureEnemyWalrus();
            Assert.IsFalse(walrus.IsIndestructible);
        }

        [TestMethod]
        public void DynamicCreatureEnemyFrost_IsIndestructible_IsFalse()
        {
            var frost = new DynamicCreatureEnemyFrost();
            Assert.IsFalse(frost.IsIndestructible);
        }

        [TestMethod]
        public void DynamicCreatureEnemyPenguin_IsIndestructible_IsFalse()
        {
            var penguin = new DynamicCreatureEnemyPenguin();
            Assert.IsFalse(penguin.IsIndestructible);
        }

        // ─────────────────────────────────────────────
        // Typhierarki — är subklasser korrekt arvade?
        // ─────────────────────────────────────────────

        [TestMethod]
        public void DynamicCreatureEnemyWalrus_IsCreature()
        {
            var walrus = new DynamicCreatureEnemyWalrus();
            Assert.IsInstanceOfType(walrus, typeof(Creature));
        }

        [TestMethod]
        public void DynamicCreatureEnemyFrost_IsCreature()
        {
            var frost = new DynamicCreatureEnemyFrost();
            Assert.IsInstanceOfType(frost, typeof(Creature));
        }

        [TestMethod]
        public void DynamicCreatureEnemyIcicle_IsCreature()
        {
            var icicle = new DynamicCreatureEnemyIcicle();
            Assert.IsInstanceOfType(icicle, typeof(Creature));
        }

        [TestMethod]
        public void DynamicCreatureEnemyBoss_IsCreature()
        {
            var boss = new DynamicCreatureEnemyBoss();
            Assert.IsInstanceOfType(boss, typeof(Creature));
        }

        [TestMethod]
        public void DynamicCreatureOverlayIce_IsCreature()
        {
            var overlay = new DynamicCreatureOverlayIce();
            Assert.IsInstanceOfType(overlay, typeof(Creature));
        }

        [TestMethod]
        public void DynamicCreatureOverlayIce_IsNotDynamicCreatureOverlay()
        {
            // OverlayIce och Overlay är separata typer — garanterar att
            // typbaserad dispatch skiljer dem åt korrekt.
            var ice = new DynamicCreatureOverlayIce();
            Assert.IsNotInstanceOfType(ice, typeof(DynamicCreatureOverlay),
                "DynamicCreatureOverlayIce ska inte ärva från DynamicCreatureOverlay.");
        }
    }
}

using Microsoft.VisualStudio.TestTools.UnitTesting;
using OlcSideScrollingConsoleGame.Models.Objects;
using System.Runtime.Serialization;

namespace UnitTest
{
    /// <summary>
    /// Enhetstester för fiendehierarkin — verifierar att typbaserad dispatch
    /// och IsIndestructible fungerar korrekt utan spelmotor eller hårdvara.
    ///
    /// Objekt skapas med FormatterServices.GetUninitializedObject så att
    /// konstruktorer (och därmed Core.Aggregate.Instance / x86-DLL-beroenden)
    /// aldrig anropas. Expression-body-properties (=>) har ingen backing field
    /// och fungerar korrekt utan konstruktor; auto-properties med initialiserare
    /// ({ get; } = value) skulle kräva konstruktor och ska undvikas i dessa klasser.
    /// </summary>
    [TestClass]
    public class EnemyTypeTests
    {
        // Hjälpmetod: skapar en instans utan att anropa konstruktorn.
        private static T Uninit<T>() where T : class
            => (T)FormatterServices.GetUninitializedObject(typeof(T));

        // ─────────────────────────────────────────────
        // IsIndestructible — oförstörbarhetsflagga
        // ─────────────────────────────────────────────

        [TestMethod]
        public void DynamicCreatureEnemyIcicle_IsIndestructible_IsTrue()
        {
            var icicle = Uninit<DynamicCreatureEnemyIcicle>();
            Assert.IsTrue(icicle.IsIndestructible,
                "Icicle-fienden ska vara oförstörbar — hoppspark ska inte döda den.");
        }

        [TestMethod]
        public void DynamicCreatureEnemyBoss_IsIndestructible_IsFalse()
        {
            var boss = Uninit<DynamicCreatureEnemyBoss>();
            Assert.IsFalse(boss.IsIndestructible,
                "Bossen är inte oförstörbar — den ska kunna ta hoppsparks-skada.");
        }

        [TestMethod]
        public void DynamicCreatureEnemyWalrus_IsIndestructible_IsFalse()
        {
            var walrus = Uninit<DynamicCreatureEnemyWalrus>();
            Assert.IsFalse(walrus.IsIndestructible);
        }

        [TestMethod]
        public void DynamicCreatureEnemyFrost_IsIndestructible_IsFalse()
        {
            var frost = Uninit<DynamicCreatureEnemyFrost>();
            Assert.IsFalse(frost.IsIndestructible);
        }

        [TestMethod]
        public void DynamicCreatureEnemyPenguin_IsIndestructible_IsFalse()
        {
            var penguin = Uninit<DynamicCreatureEnemyPenguin>();
            Assert.IsFalse(penguin.IsIndestructible);
        }

        // ─────────────────────────────────────────────
        // Typhierarki — är subklasser korrekt arvade?
        // ─────────────────────────────────────────────

        [TestMethod]
        public void DynamicCreatureEnemyWalrus_IsCreature()
        {
            var walrus = Uninit<DynamicCreatureEnemyWalrus>();
            Assert.IsInstanceOfType(walrus, typeof(Creature));
        }

        [TestMethod]
        public void DynamicCreatureEnemyFrost_IsCreature()
        {
            var frost = Uninit<DynamicCreatureEnemyFrost>();
            Assert.IsInstanceOfType(frost, typeof(Creature));
        }

        [TestMethod]
        public void DynamicCreatureEnemyIcicle_IsCreature()
        {
            var icicle = Uninit<DynamicCreatureEnemyIcicle>();
            Assert.IsInstanceOfType(icicle, typeof(Creature));
        }

        [TestMethod]
        public void DynamicCreatureEnemyBoss_IsCreature()
        {
            var boss = Uninit<DynamicCreatureEnemyBoss>();
            Assert.IsInstanceOfType(boss, typeof(Creature));
        }

        [TestMethod]
        public void DynamicCreatureOverlayIce_IsCreature()
        {
            var overlay = Uninit<DynamicCreatureOverlayIce>();
            Assert.IsInstanceOfType(overlay, typeof(Creature));
        }

        [TestMethod]
        public void DynamicCreatureOverlayIce_IsNotDynamicCreatureOverlay()
        {
            // OverlayIce och Overlay är separata typer — garanterar att
            // typbaserad dispatch skiljer dem åt korrekt.
            var ice = Uninit<DynamicCreatureOverlayIce>();
            Assert.IsNotInstanceOfType(ice, typeof(DynamicCreatureOverlay),
                "DynamicCreatureOverlayIce ska inte ärva från DynamicCreatureOverlay.");
        }
    }
}

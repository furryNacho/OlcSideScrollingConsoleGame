#nullable enable
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnitTest.Fakes;

namespace UnitTest
{
    /// <summary>
    /// Enhetstester för ISettingsService — testar FakeSettingsService i isolation.
    /// Ingen fil-I/O eller Aggregate krävs.
    /// </summary>
    [TestClass]
    public class SettingsServiceTests
    {
        [TestMethod]
        public void AudioOn_DefaultIsTrue()
        {
            var svc = new FakeSettingsService();
            Assert.IsTrue(svc.AudioOn);
        }

        [TestMethod]
        public void AudioOn_CanBeSetToFalse()
        {
            var svc = new FakeSettingsService();
            svc.AudioOn = false;
            Assert.IsFalse(svc.AudioOn);
        }

        [TestMethod]
        public void AudioOn_RoundTrip()
        {
            var svc = new FakeSettingsService();
            svc.AudioOn = false;
            svc.AudioOn = true;
            Assert.IsTrue(svc.AudioOn);
        }

        [TestMethod]
        public void Save_IncrementsSaveCount()
        {
            var svc = new FakeSettingsService();
            svc.Save();
            svc.Save();
            Assert.AreEqual(2, svc.SaveCount);
        }

        [TestMethod]
        public void ClearSaveSlot_SlotOne_IsCleared()
        {
            var svc = new FakeSettingsService();
            svc.SaveSlots.SlotOne.StageCompleted = 5;
            svc.ClearSaveSlot(1);
            Assert.AreEqual(0, svc.SaveSlots.SlotOne.StageCompleted);
        }

        [TestMethod]
        public void ClearSaveSlot_SlotTwo_IsCleared()
        {
            var svc = new FakeSettingsService();
            svc.SaveSlots.SlotTwo.StageCompleted = 3;
            svc.ClearSaveSlot(2);
            Assert.AreEqual(0, svc.SaveSlots.SlotTwo.StageCompleted);
        }

        [TestMethod]
        public void ClearSaveSlot_SlotThree_IsCleared()
        {
            var svc = new FakeSettingsService();
            svc.SaveSlots.SlotThree.StageCompleted = 7;
            svc.ClearSaveSlot(3);
            Assert.AreEqual(0, svc.SaveSlots.SlotThree.StageCompleted);
        }

        [TestMethod]
        public void ClearSaveSlot_TracksLastClearedSlot()
        {
            var svc = new FakeSettingsService();
            svc.ClearSaveSlot(2);
            Assert.AreEqual(2, svc.LastClearedSlot);
        }

        [TestMethod]
        public void ClearSaveSlot_IncrementsClearCount()
        {
            var svc = new FakeSettingsService();
            svc.ClearSaveSlot(1);
            svc.ClearSaveSlot(3);
            Assert.AreEqual(2, svc.ClearSlotCount);
        }

        [TestMethod]
        public void ActivePlayer_MutationIsReflected()
        {
            var svc = new FakeSettingsService();
            svc.ActivePlayer.StageCompleted = 4;
            Assert.AreEqual(4, svc.ActivePlayer.StageCompleted);
        }
    }
}

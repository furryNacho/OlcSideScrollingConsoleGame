﻿using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OlcSideScrollingConsoleGame.Models;

namespace UnitTest
{
    [TestClass]
    public class UnitTest
    {
        [TestMethod]
        public void TestWriteToLog()
        {
            //Arrange
            var init = new OlcSideScrollingConsoleGame.Core.ReadWrite();
            //Act
            init.WriteToLog("Unit test write to log");
            //Assert
        }

        [TestMethod]
        public void TestWriteJson()
        {
            //Arrange
            var rw = new OlcSideScrollingConsoleGame.Core.ReadWrite();
            string Path = "\\Resources\\Settings";
            string FileName = "\\settings";
            string FileExtension = ".json";

            var HSO = new HighScoreObj()
            {
                Id = 1,
                Handle = "AAA",
                Hours = 0,
                Minutes = 5,
                Seconds = 2,
                Time = "00:05:02:123"
            };
            var HSOList = new List<HighScoreObj>();
            HSOList.Add(HSO);

            var testObj = new SettingsObj()
            {
                misc = "Unit test",
                //AttributeIndex = new int[3] { 1, 2, 4 }
                HighScoreList = HSOList
            };

            //Act
            var success = rw.WriteJson<SettingsObj>(Path, FileName, FileExtension, testObj);

            //Assert
            Assert.IsTrue(success);
        }

        [TestMethod]
        public void TestReadJson()
        {
            //Arrange
            var init = new OlcSideScrollingConsoleGame.Core.ReadWrite();
            string Path = "\\Resources\\Settings";
            string FileName = "\\settings";
            string FileExtension = ".json";

            //Act
            var returnObj = init.ReadJson<SettingsObj>(Path, FileName, FileExtension);

            //Assert
            Assert.IsNotNull(returnObj);
            Assert.IsTrue(returnObj is SettingsObj);
            Assert.IsNotNull(returnObj.misc);
            Assert.IsTrue(returnObj.misc == "Unit test");
        }

        
        [TestMethod]
        public void KrabbaMedJson()
        {
            // Måste modda hanteringen av json
            try
            {
                var FullPath = System.IO.Path.Combine(Environment.CurrentDirectory + @"\Resources\Settings\settings.json");
                string json = File.ReadAllText(FullPath);
                var asdf = KassJson.Parse<SettingsObj>(json);

            }
            catch (Exception ex)
            {
                var nogood = ex.ToString();
            }



        }

    }
}

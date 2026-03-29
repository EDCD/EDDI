using EddiUI;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Controls;

namespace Tests
{
    [TestClass, TestCategory("UnitTests")]
    public class EddiUiTests : TestBase
    {
        [TestInitialize]
        public void Start()
        {
            MakeSafe();
        }

        [TestMethod]
        public void LanguageDef_Constructor_WithCultureInfo_SetsDisplayName()
        {
            // Arrange
            var culture = new CultureInfo("en-US");

            // Act
            var languageDef = new MainWindow.LanguageDef(culture);

            // Assert
            Assert.IsNotNull(languageDef.displayName);
            Assert.Contains( culture.NativeName, languageDef.displayName );
            Assert.Contains( culture.DisplayName, languageDef.displayName );
            Assert.AreEqual(culture, languageDef.ci);
        }

        [TestMethod]
        public void LanguageDef_Constructor_WithCustomDisplayName_SetsCustomName()
        {
            // Arrange
            var culture = new CultureInfo("en-US");
            var customName = "Custom English";

            // Act
            var languageDef = new MainWindow.LanguageDef(culture, customName);

            // Assert
            Assert.AreEqual(customName, languageDef.displayName);
            Assert.AreEqual(culture, languageDef.ci);
        }

        [TestMethod]
        public void LanguageDef_CompareTo_SortsAlphabetically()
        {
            // Arrange
            var langA = new MainWindow.LanguageDef(new CultureInfo("en"), "Aaa");
            var langB = new MainWindow.LanguageDef(new CultureInfo("fr"), "Bbb");
            var langC = new MainWindow.LanguageDef(new CultureInfo("de"), "Ccc");

            var languages = new List<MainWindow.LanguageDef> { langC, langA, langB };

            // Act
            languages.Sort();

            // Assert
            Assert.AreEqual("Aaa", languages[0].displayName);
            Assert.AreEqual("Bbb", languages[1].displayName);
            Assert.AreEqual("Ccc", languages[2].displayName);
        }

        [TestMethod]
        public void LanguageDef_CompareTo_ReturnsZeroForEqualNames()
        {
            // Arrange
            var lang1 = new MainWindow.LanguageDef(new CultureInfo("en"), "Same");
            var lang2 = new MainWindow.LanguageDef(new CultureInfo("fr"), "Same");

            // Act
            var result = lang1.CompareTo(lang2);

            // Assert
            Assert.AreEqual(0, result);
        }

        [TestMethod]
        public void LanguageDef_CompareTo_NegativeResult_WhenFirstIsLessThanSecond()
        {
            // Arrange
            var langA = new MainWindow.LanguageDef(new CultureInfo("en"), "Apple");
            var langZ = new MainWindow.LanguageDef(new CultureInfo("fr"), "Zebra");

            // Act
            var result = langA.CompareTo(langZ);

            // Assert
            Assert.IsLessThan( 0, result );
        }

        [TestMethod]
        public void LanguageDef_CompareTo_PositiveResult_WhenFirstIsGreaterThanSecond()
        {
            // Arrange
            var langZ = new MainWindow.LanguageDef(new CultureInfo("en"), "Zebra");
            var langA = new MainWindow.LanguageDef(new CultureInfo("fr"), "Apple");

            // Act
            var result = langZ.CompareTo(langA);

            // Assert
            Assert.IsGreaterThan(0, result );
        }
    }

    [STATestClass, TestCategory("UnitTests")]
    public class MainWindowTabItemComparerTests : TestBase
    {
        [TestInitialize]
        public void Start()
        {
            MakeSafe();
        }

        [TestMethod]
        public void TabItemComparer_Compare_SortsTabsByHeader()
        {
            // Arrange
            var comparer = new MainWindow.TabItemComparer(StringComparer.CurrentCultureIgnoreCase);
            var tab1 = new TabItem { Header = "Zebra" };
            var tab2 = new TabItem { Header = "Apple" };
            var tab3 = new TabItem { Header = "Banana" };

            var tabs = new List<TabItem> { tab1, tab2, tab3 };

            // Act
            tabs.Sort(comparer);

            // Assert
            Assert.AreEqual("Apple", (string)tabs[0].Header);
            Assert.AreEqual("Banana", (string)tabs[1].Header);
            Assert.AreEqual("Zebra", (string)tabs[2].Header);
        }

        [TestMethod]
        public void TabItemComparer_Compare_IgnoresCase()
        {
            // Arrange
            var comparer = new MainWindow.TabItemComparer(StringComparer.CurrentCultureIgnoreCase);
            var tab1 = new TabItem { Header = "zebra" };
            var tab2 = new TabItem { Header = "APPLE" };

            // Act
            var result = comparer.Compare(tab2, tab1);

            // Assert
            Assert.IsLessThan(0, result ); // APPLE comes before zebra
        }

        [TestMethod]
        public void TabItemComparer_Compare_HandlesNullHeaders()
        {
            // Arrange
            var comparer = new MainWindow.TabItemComparer(StringComparer.CurrentCultureIgnoreCase);
            var tab1 = new TabItem { Header = null };
            var tab2 = new TabItem { Header = "Valid" };

            // Act & Assert - should not throw exception
            var result = comparer.Compare(tab1, tab2);
            Assert.AreNotEqual(0, result);
        }

        [TestMethod]
        public void TabItemComparer_Compare_SingleTab()
        {
            // Arrange
            var comparer = new MainWindow.TabItemComparer(StringComparer.CurrentCultureIgnoreCase);
            var tab1 = new TabItem { Header = "OnlyOne" };

            // Act
            var tabs = new List<TabItem> { tab1 };
            tabs.Sort(comparer);

            // Assert
            Assert.HasCount(1, tabs);
            Assert.AreEqual("OnlyOne", (string)tabs[0].Header);
        }
    }
}
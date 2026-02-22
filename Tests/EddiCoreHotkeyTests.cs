using EddiCore.Hotkeys;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace Tests
{
    [STATestClass, TestCategory( "UnitTests" )]
    public class EddiCoreHotkeyTests : TestBase
    {
        private List<HotkeyAction> hotkeyActions => new List<HotkeyAction>
        {
            new HotkeyAction( "Test", "Test", () => Console.WriteLine(@"TestAction") )
        };
        private HotkeyActionCollection hotkeyCollection => new HotkeyActionCollection( hotkeyActions );

        private static HotkeyRegistration CreateRegistration ( HotkeyActionCollection collection )
        {
            // installHook: false => no global hook in unit tests
            // invoke: synchronous => deterministic asserts
            return new HotkeyRegistration( collection, installHook: false, invoke: a => a() );
        }

        [TestInitialize]
        public void Setup ()
        {
            MakeSafe();
        }

        [TestMethod]
        public void RegisterHotkey ()
        {
            // Arrange
            var gesture = new KeyGesture(Key.A, ModifierKeys.Control);
            var manager = new HotkeyManager { Hotkeys = CreateRegistration(hotkeyCollection) };

            // Act
            manager.RegisterHotkey( "Test", gesture );

            // Assert
            manager.Hotkeys.Collection.TryGetValue( "Test", out var actionHotkey );
            Assert.IsNotNull( actionHotkey,"We should be able to retrieve the item we just modified" );
            Assert.AreEqual( actionHotkey.KeyGesture, gesture,"The retrieved item should include our configured gesture." );
        }

        [TestMethod]
        public void UnregisterHotkey ()
        {
            // Arrange
            var gesture = new KeyGesture(Key.A, ModifierKeys.Control);
            var manager = new HotkeyManager { Hotkeys = CreateRegistration(hotkeyCollection) };
            manager.RegisterHotkey( "Test", gesture );

            // Act
            manager.UnregisterHotkey( "Test" );

            // Assert
            manager.Hotkeys.Collection.TryGetValue( "Test", out var actionHotkey );
            Assert.IsNotNull( actionHotkey,"We should be able to retrieve the item we just modified" );
            Assert.IsNull( actionHotkey.KeyGesture,"The retrieved item should not include any configured gesture." );
        }

        [TestMethod]
        public void UnregisterAllHotkeys ()
        {
            // Arrange
            var gesture = new KeyGesture(Key.A, ModifierKeys.Control);
            var manager = new HotkeyManager { Hotkeys = CreateRegistration(hotkeyCollection) };
            manager.RegisterHotkey( "Test", gesture );

            // Act
            manager.UnregisterAllHotkeys();

            // Assert
            manager.Hotkeys.Collection.TryGetValue( "Test", out var actionHotkey );
            Assert.IsNotNull( actionHotkey, "We should be able to retrieve the item we just modified" );
            Assert.IsNull( actionHotkey.KeyGesture, "The retrieved item should not include any configured gesture." );
        }

        [TestMethod]
        public void TriggerHotkey_InvokesAction ()
        {
            // Arrange
            var invoked = false;
            var action = new HotkeyAction("Test", "Test", () => invoked = true);
            var collection = new HotkeyActionCollection(new List<HotkeyAction> { action });

            var manager = new HotkeyManager { Hotkeys = CreateRegistration(collection) };
            manager.RegisterHotkey( "Test", new KeyGesture( Key.A, ModifierKeys.Control ) );

            // Act
            manager.Hotkeys.TestTrigger( Key.A, ModifierKeys.Control );

            // Assert
            Assert.IsTrue( invoked, "Action should be invoked when matching gesture is triggered." );
        }

        [TestMethod]
        public void TriggerHotkey_DoesNotInvokeOnWrongModifiers ()
        {
            // Arrange
            var invoked = false;
            var action = new HotkeyAction("Test", "Test", () => invoked = true);
            var collection = new HotkeyActionCollection(new List<HotkeyAction> { action });

            var manager = new HotkeyManager { Hotkeys = CreateRegistration(collection) };
            manager.RegisterHotkey( "Test", new KeyGesture( Key.A, ModifierKeys.Control ) );

            // Act
            manager.Hotkeys.TestTrigger( Key.A, ModifierKeys.Alt );

            // Assert
            Assert.IsFalse( invoked, "Action should not be invoked when modifiers do not match." );
        }
    }
}

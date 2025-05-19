using EddiCore.Hotkeys;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace Tests
{
    [TestClass, TestCategory( "UnitTests" )]
    public class EddiCoreHotkeyTests : TestBase
    {
        private List<HotkeyAction> hotkeyActions => new List<HotkeyAction>
        {
            new HotkeyAction( "Test", "Test", () => Console.WriteLine(@"TestAction") )
        };
        private HotkeyActionCollection hotkeyCollection => new HotkeyActionCollection( hotkeyActions );

        private static IntPtr GetValidWindowHandle (out Window window)
        {
            window = new Window { Width = 1, Height = 1, ShowInTaskbar = false, WindowStyle = WindowStyle.None };
            window.Show();
            var handle = new WindowInteropHelper(window).Handle;
            window.Hide(); // Optionally hide the window immediately
            return handle;
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
            var manager = new HotkeyManager { Hotkeys = new HotkeyRegistration( GetValidWindowHandle(out var window), hotkeyCollection ) };

            // Act
            manager.RegisterHotkey( "Test", gesture );

            // Assert
            manager.Hotkeys.Collection.TryGetValue( "Test", out var actionHotkey );
            Assert.IsNotNull( actionHotkey,"We should be able to retrieve the item we just modified" );
            Assert.AreEqual( actionHotkey.KeyGesture, gesture,"The retrieved item should include our configured gesture." );
            Assert.IsNotNull( actionHotkey.id,"RegisterHotkey should have assigned an id once the hotkey was registered." );

            // Cleanup
            window.Close(); // Close the window after the test
        }

        [TestMethod]
        public void UnregisterHotkey ()
        {
            // Arrange
            var gesture = new KeyGesture(Key.A, ModifierKeys.Control);
            var manager = new HotkeyManager { Hotkeys = new HotkeyRegistration( GetValidWindowHandle(out var window), hotkeyCollection ) };
            manager.RegisterHotkey( "Test", gesture );

            // Act
            manager.UnregisterHotkey( "Test" );

            // Assert
            manager.Hotkeys.Collection.TryGetValue( "Test", out var actionHotkey );
            Assert.IsNotNull( actionHotkey,"We should be able to retrieve the item we just modified" );
            Assert.IsNull( actionHotkey.KeyGesture,"The retrieved item should not include any configured gesture." );
            Assert.IsNull( actionHotkey.id,"Any assigned id must be removed." );

            // Cleanup
            window.Close(); // Close the window after the test
        }

        [TestMethod]
        public void UnregisterAllHotkeys ()
        {
            // Arrange
            var gesture = new KeyGesture(Key.A, ModifierKeys.Control);
            var manager = new HotkeyManager { Hotkeys = new HotkeyRegistration( GetValidWindowHandle(out var window), hotkeyCollection ) };
            manager.RegisterHotkey( "Test", gesture );

            // Act
            manager.UnregisterAllHotkeys();

            // Assert
            manager.Hotkeys.Collection.TryGetValue( "Test", out var actionHotkey );
            Assert.IsNotNull( actionHotkey, "We should be able to retrieve the item we just modified" );
            Assert.IsNull( actionHotkey.KeyGesture, "The retrieved item should not include any configured gesture." );
            Assert.IsNull( actionHotkey.id, "Any assigned id must be removed." );

            // Cleanup
            window.Close(); // Close the window after the test
        }

        [TestMethod]
        public void HandleHotkeyMessage_InvokesAction ()
        {
            // Arrange
            var actionInvoked = false;
            var action = new HotkeyAction("Test", "Test", () => actionInvoked = true);
            var collection = new HotkeyActionCollection(new List<HotkeyAction> { action });
            collection.AddId( "Test", 1 );
            var manager = new HotkeyManager { Hotkeys = new HotkeyRegistration( GetValidWindowHandle(out var window), collection ) };

            var handled = false;

            // Act
            manager.HandleHotkeyMessage( 0x0312, new IntPtr( 1 ), ref handled );

            // Assert
            Assert.IsTrue( actionInvoked, "Action should be invoked when hotkey message is handled." );
            Assert.IsTrue( handled, "Handled should be set to true." );

            // Cleanup
            window.Close(); // Close the window after the test
        }
    }
}

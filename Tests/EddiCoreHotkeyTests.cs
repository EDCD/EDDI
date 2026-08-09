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
        private static List<HotkeyAction> hotkeyActions =>
            [ new( "Test", "Test", () => Console.WriteLine( @"TestAction" ) ) ];
        private static HotkeyActionCollection hotkeyCollection => new( hotkeyActions );

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
            var collection = new HotkeyActionCollection( [ action ] );

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
            var collection = new HotkeyActionCollection( [ action ] );

            var manager = new HotkeyManager { Hotkeys = CreateRegistration(collection) };
            manager.RegisterHotkey( "Test", new KeyGesture( Key.A, ModifierKeys.Control ) );

            // Act
            manager.Hotkeys.TestTrigger( Key.A, ModifierKeys.Alt );

            // Assert
            Assert.IsFalse( invoked, "Action should not be invoked when modifiers do not match." );
        }

        [TestMethod]
        public void HookInstallationFailure_DoesNotInstallGlobalHook ()
        {
            // Arrange
            var messages = new List<string>();

            // Act
            var registration = new HotkeyRegistration(
                hotkeyCollection,
                installHook: true,
                invoke: a => a(),
                installHookOverride: () => IntPtr.Zero,
                logHookUnavailable: messages.Add,
                scheduleRetries: false );

            // Assert
            Assert.AreEqual( 1, registration.HookInstallAttempts, "The constructor should attempt hook installation once." );
            Assert.IsFalse( registration.IsHookInstalled, "The hook should remain uninstalled when the native call fails." );
            Assert.HasCount( 1, messages, "Hook installation failures should be logged once per failed attempt." );
            Assert.Contains( "Attempt=1/3" , messages[ 0 ]);
        }

        [TestMethod]
        public void HookInstallationFailure_StopsAfterMaxAttempts ()
        {
            // Arrange
            var messages = new List<string>();
            var registration = new HotkeyRegistration(
                hotkeyCollection,
                installHook: true,
                invoke: a => a(),
                installHookOverride: () => IntPtr.Zero,
                logHookUnavailable: messages.Add,
                scheduleRetries: false );

            // Act
            registration.RetryHookInstallation();
            registration.RetryHookInstallation();
            registration.RetryHookInstallation();

            // Assert
            Assert.AreEqual( 3, registration.HookInstallAttempts, "Retries should stop at the configured maximum." );
            Assert.IsTrue( registration.IsHookUnavailable, "The hook should be marked unavailable after repeated failures." );
            Assert.HasCount( 3, messages, "No failure should be logged after retry attempts are exhausted." );
        }

        [TestMethod]
        public void HookInstallationFailure_DoesNotPreventHotkeyRegistration ()
        {
            // Arrange
            var gesture = new KeyGesture( Key.A, ModifierKeys.Control );
            var registration = new HotkeyRegistration(
                hotkeyCollection,
                installHook: true,
                invoke: a => a(),
                installHookOverride: () => IntPtr.Zero,
                logHookUnavailable: _ => { },
                scheduleRetries: false );
            var manager = new HotkeyManager { Hotkeys = registration };

            // Act
            manager.RegisterHotkey( "Test", gesture );

            // Assert
            manager.Hotkeys.Collection.TryGetValue( "Test", out var actionHotkey );
            Assert.IsNotNull( actionHotkey, "We should be able to retrieve the item we just modified." );
            Assert.AreEqual( gesture, actionHotkey.KeyGesture, "Hook availability should not block hotkey configuration." );
        }
    }
}

using BuildSecrets;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Tests
{
    [TestClass, TestCategory( "UnitTests" )]
    public class BuildSecretsTests
    {
        [TestMethod]
        public void EddnSignatureMatchesKnownVectorAndClearsKey ()
        {
            var key = Enumerable.Repeat( (byte)0x0b, 20 ).ToArray();
            void Provide ( ref byte[] value ) => value = key;

            Assert.IsTrue( BuildInjectedSecrets.TrySignEddnPayload( Encoding.ASCII.GetBytes( "Hi There" ), Provide, out var signature ) );
            Assert.AreEqual( "b0344c61d8db38535ca8afceaf0bf12b881dc200c9833da726e9376c2e32cff7", signature );
            Assert.IsTrue( key.All( b => b == 0 ) );
        }

        [TestMethod]
        public void EddnSignaturePreservesUtf8AndUsesLowercaseHex ()
        {
            var key = Encoding.UTF8.GetBytes( "clé-test-星" );
            var payload = Encoding.UTF8.GetBytes( "Étoile 星" );
            using var reference = new HMACSHA256( key );
            var expected = Convert.ToHexString( reference.ComputeHash( payload ) ).ToLowerInvariant();
            void Provide ( ref byte[] value ) => value = key;

            Assert.IsTrue( BuildInjectedSecrets.TrySignEddnPayload( payload, Provide, out var signature ) );
            Assert.AreEqual( expected, signature );
            Assert.AreEqual( 64, signature.Length );
            Assert.IsTrue( signature.All( c => c is (>= '0' and <= '9') or (>= 'a' and <= 'f') ) );
            Assert.IsTrue( key.All( b => b == 0 ) );
        }

        [TestMethod]
        public void MissingEddnKeyReturnsUnsigned ()
        {
            static void Provide ( ref byte[] value ) { }
            Assert.IsFalse( BuildInjectedSecrets.TrySignEddnPayload( [1], Provide, out var signature ) );
            Assert.AreEqual( string.Empty, signature );
        }

        [TestMethod]
        public void EmptyEddnKeyFailsWithoutSignature ()
        {
            static void Provide ( ref byte[] value ) => value = [];
            var signature = "previous result";
            Assert.ThrowsExactly<InvalidOperationException>( () =>
                BuildInjectedSecrets.TrySignEddnPayload( [1], Provide, out signature ) );
            Assert.AreEqual( string.Empty, signature );
        }

        [TestMethod]
        public void ReconstructionFailureClearsAssignedKeyWithoutSignature ()
        {
            var key = new byte[] { 1, 2, 3 };
            void Provide ( ref byte[] value )
            {
                value = key;
                throw new InvalidOperationException( "Disposable reconstruction failure" );
            }
            var signature = "previous result";
            Assert.ThrowsExactly<InvalidOperationException>( () =>
                BuildInjectedSecrets.TrySignEddnPayload( [1], Provide, out signature ) );
            Assert.AreEqual( string.Empty, signature );
            Assert.IsTrue( key.All( b => b == 0 ) );
        }

        [TestMethod]
        public void NullPayloadFailsBeforeRequestingKey ()
        {
            static void Provide ( ref byte[] value ) => Assert.Fail( "Key must not be requested" );
            Assert.ThrowsExactly<ArgumentNullException>( () =>
                BuildInjectedSecrets.TrySignEddnPayload( null, Provide, out _ ) );
        }
    }
}

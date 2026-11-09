using EddiDataDefinitions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace Tests
{
    [TestClass, TestCategory("UnitTests")]
    public class CoriolisTests
    {
        [TestMethod]
        [DataRow( false, "coriolis.io" )]
        [DataRow( true, "beta.coriolis.io" )]
        public void CoriolisUriIncludesEscapedNameAndCompressedShipJson( bool beta, string host )
        {
            const string raw = "{\"name\":\"星\",\"modules\":[]}";
            const string name = "BZ+24 & 星 #123";
            var ship = new Ship { raw = raw, name = name };

            var uri = new Uri( ship.CoriolisUri( beta ) );

            Assert.AreEqual( "https", uri.Scheme );
            Assert.AreEqual( host, uri.Host );
            Assert.AreEqual( "/import", uri.AbsolutePath );
            var query = uri.Query.TrimStart( '?' ).Split( '&' )
                .Select( item => item.Split( '=', 2 ) )
                .ToDictionary( item => item[0], item => item[1] );
            Assert.HasCount( 2, query );
            Assert.AreEqual( Uri.EscapeDataString( name ), query["bn"] );
            Assert.AreEqual( name, Uri.UnescapeDataString( query["bn"] ) );

            var compressed = Convert.FromBase64String( Uri.UnescapeDataString( query["data"] ) );
            using var input = new MemoryStream( compressed );
            using var gzip = new GZipStream( input, CompressionMode.Decompress );
            using var reader = new StreamReader( gzip );
            Assert.AreEqual( raw, reader.ReadToEnd() );
        }

        [TestMethod]
        [DataRow( false )]
        [DataRow( true )]
        public void CoriolisUriWithoutRawShipJsonReturnsNull( bool beta )
        {
            Assert.IsNull( new Ship().CoriolisUri( beta ) );
        }
    }
}

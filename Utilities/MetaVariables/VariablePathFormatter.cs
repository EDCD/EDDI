using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utilities.MetaVariables
{
    public static class VariablePathFormatter
    {
        public static string RenderCottlePath ( IEnumerable<string> keysPath )
        {
            var path = ( keysPath ?? [] )
                .Where( k => !string.IsNullOrEmpty( k ) )
                .ToList();

            return string
                .Join( ".", path )
                .Replace( $".{MetaVariables.indexMarker}", @"[\<index\>]" );
        }

        public static string RenderVoiceAttackName (
            string startingPrefix,
            string eventType,
            IEnumerable<string> keysPath )
        {
            var key = startingPrefix ?? string.Empty;
            var path = ( keysPath ?? [] )
                .Prepend( eventType?.ToLowerInvariant() )
                .Where( k => !string.IsNullOrEmpty( k ) )
                .ToList();

            foreach ( var keySegment in path )
            {
                var childKey = AddSpacesToTitleCasedName( keySegment )
                    .Replace( "_", " " )
                    .ToLowerInvariant();

                key = ConcatOverlappingNames( key, childKey );
            }

            return key.Replace( MetaVariables.indexMarker, @"\<index\>" );
        }

        public static string RenderVoiceAttackTypeName ( Type type )
        {
            if ( type == typeof( string ) )
            {
                return "TXT";
            }

            if ( type == typeof( int ) )
            {
                return "INT";
            }

            if ( type == typeof( bool ) )
            {
                return "BOOL";
            }

            if ( type == typeof( decimal ) ||
                 type == typeof( double ) ||
                 type == typeof( float ) ||
                 type == typeof( long ) ||
                 type == typeof( ulong ) ||
                 type == typeof( uint ) )
            {
                return "DEC";
            }

            if ( type == typeof( DateTime ) )
            {
                return "DATE";
            }

            if ( type != typeof( string ) &&
                 type != null &&
                 typeof( IEnumerable ).IsAssignableFrom( type ) )
            {
                return "INT";
            }

            return string.Empty;
        }

        private static string AddSpacesToTitleCasedName ( string text )
        {
            if ( string.IsNullOrWhiteSpace( text ) )
            {
                return string.Empty;
            }

            var newText = new StringBuilder( text.Length * 2 );
            newText.Append( text[ 0 ] );
            for ( var i = 1; i < text.Length; i++ )
            {
                if ( char.IsUpper( text[ i ] ) &&
                     text[ i - 1 ] != ' ' &&
                     !char.IsUpper( text[ i - 1 ] ) )
                {
                    newText.Append( ' ' );
                }
                newText.Append( text[ i ] );
            }
            return newText.ToString();
        }

        private static string ConcatOverlappingNames ( string prefix, string childKey )
        {
            var skip = 0;
            if ( !prefix.EndsWith( ' ' ) )
            {
                prefix += " ";
            }

            while ( skip < childKey.Length ||
                    prefix.Skip( skip ).Count() - 1 > childKey.Length ||
                    (prefix.Skip( skip ).Zip( childKey, ( a, b ) => a.Equals( b ) ).Any( x => !x ) && skip < prefix.Length) )
            {
                skip++;
            }

            return string.Concat( prefix.Take( skip ).Concat( childKey ) );
        }
    }
}

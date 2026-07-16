using System;

namespace DocumentationGenerator
{
    internal static class Program
    {
        private static int Main ( string[] args )
        {
            if ( args.Length != 1 )
            {
                Console.Error.WriteLine( "Usage: DocumentationGenerator <output-directory>" );
                return 1;
            }

            DocumentationGenerator.WriteWikiOutput( args[ 0 ] );
            Console.WriteLine( $"Generated documentation output in '{args[ 0 ]}'." );
            return 0;
        }
    }
}

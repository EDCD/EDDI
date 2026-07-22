using EddiSpeechResponder.AvalonEdit;
using EddiSpeechResponder.ScriptResolverService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Utilities;
using Utilities.MetaVariables;

[assembly: InternalsVisibleTo( "Tests" )]
namespace EddiSpeechResponder
{
    public class TextCompletion ( IEnumerable<MetaVariable> metaVars )
    {
        private static readonly List<ICustomFunction> customFunctions = ScriptResolver.GetCustomFunctions();
        private static readonly object metaVarLock = new();

        internal sealed record CompletionAlias (
            string Expression,
            bool IsEnumerationKey = false
        );

        public static string GetLookupItem ( string lineTxt )
        {
            if ( string.IsNullOrWhiteSpace( lineTxt ) )
            {
                return string.Empty;
            }

            var match = GeneratedRegex.CottleCompletionLookupRegex().Match( lineTxt );
            return match.Success
                ? NormalizeLookupExpression( match.Groups[ "lookup" ].Value )
                : string.Empty;
        }

        internal static string NormalizeLookupExpression ( string expression )
        {
            if ( string.IsNullOrWhiteSpace( expression ) )
            {
                return string.Empty;
            }

            expression = expression
                .Replace( "!", "" )
                .Trim()
                .TrimEnd( '.' );

            // Normalize map/index access:
            // bodies[5]       -> bodies.<indexMarker>
            // mymap["f1"]     -> mymap.<indexMarker>
            // mymap['f1']     -> mymap.<indexMarker>
            // mymap[key]      -> mymap.<indexMarker>
            expression = GeneratedRegex.CottleIndexerRegex()
                .Replace( expression, $".{MetaVariables.indexMarker}" );

            // Completion expressions should not preserve whitespace.
            return GeneratedRegex.WhiteSpaceRegex().Replace( expression, string.Empty );
        }

        internal static Dictionary<string, CompletionAlias> BuildCompletionAliases ( string priorText )
        {
            var aliases = new Dictionary<string, CompletionAlias>( StringComparer.Ordinal );

            if ( string.IsNullOrWhiteSpace( priorText ) )
            {
                return aliases;
            }

            foreach ( Match match in GeneratedRegex.CottleSetExpressionRegex().Matches( priorText ) )
            {
                var key = match.Groups[ "key" ].Value;
                var expression = NormalizeLookupExpression( match.Groups[ "expression" ].Value );

                if ( !string.IsNullOrEmpty( key ) && !string.IsNullOrEmpty( expression ) )
                {
                    aliases[ key ] = new CompletionAlias( expression );
                }
            }

            foreach ( Match match in GeneratedRegex.CottleForEnumerationRegex().Matches( priorText ) )
            {
                var collection = NormalizeLookupExpression( match.Groups[ "collection" ].Value );
                var valueAlias = match.Groups[ "value" ].Value;
                var keyAlias = match.Groups[ "key" ].Success
                    ? match.Groups[ "key" ].Value
                    : string.Empty;

                if ( !string.IsNullOrEmpty( valueAlias ) && !string.IsNullOrEmpty( collection ) )
                {
                    aliases[ valueAlias ] = new CompletionAlias(
                        $"{collection}.{MetaVariables.indexMarker}" );
                }

                if ( !string.IsNullOrEmpty( keyAlias ) )
                {
                    aliases[ keyAlias ] = new CompletionAlias(
                        string.Empty,
                        IsEnumerationKey: true );
                }
            }

            return aliases;
        }

        internal static List<string> ResolveLookupKeys ( string lookupItem, string priorText )
        {
            var lookupKeys = SplitLookupPath( lookupItem );
            if ( lookupKeys.Count == 0 )
            {
                return lookupKeys;
            }

            var aliases = BuildCompletionAliases( priorText );

            // Prevent alias loops from locking the editor.
            for ( var depth = 0; depth < 10; depth++ )
            {
                if ( lookupKeys.Count == 0 )
                {
                    return lookupKeys;
                }

                var root = lookupKeys[ 0 ];

                if ( !aliases.TryGetValue( root, out var alias ) )
                {
                    return lookupKeys;
                }

                if ( alias.IsEnumerationKey || string.IsNullOrEmpty( alias.Expression ) )
                {
                    return [];
                }

                var aliasKeys = SplitLookupPath( alias.Expression );
                if ( aliasKeys.Count == 0 )
                {
                    return [];
                }

                lookupKeys.RemoveAt( 0 );
                lookupKeys.InsertRange( 0, aliasKeys );
            }

            // Alias recursion limit exceeded. Fail closed rather than offering bad completions.
            return [];
        }

        internal static List<string> SplitLookupPath ( string expression )
        {
            expression = NormalizeLookupExpression( expression );

            return string.IsNullOrEmpty( expression )
                ? []
                : expression.Split( [ '.' ], StringSplitOptions.RemoveEmptyEntries ).ToList();
        }

        public List<TextCompletionItem> GetCompletionItems ( string lookupItem, string priorText )
        {
            var textCompletionItems = new List<TextCompletionItem>();

            if ( string.IsNullOrEmpty( lookupItem ) || string.IsNullOrEmpty( priorText ) )
            {
                return textCompletionItems;
            }

            var lookupKeys = ResolveLookupKeys( lookupItem, priorText );
            if ( lookupKeys.Count == 0 )
            {
                return textCompletionItems;
            }

            var filteredMetaVars = new List<MetaVariable>();

            filteredMetaVars = ResolveDirectFunctionInvocations( filteredMetaVars, lookupKeys );

            if ( filteredMetaVars.Count == 0 )
            {
                lock ( metaVarLock )
                {
                    filteredMetaVars = FilterMetaVars( metaVars, lookupKeys );
                }
            }

            foreach ( var item in filteredMetaVars.OrderBy( v => string.Join( ".", v.keysPath ) ) )
            {
                var itemKey = item.keysPath.Last();

                if ( string.IsNullOrEmpty( itemKey ) ||
                     textCompletionItems.Any( d => d.Text == itemKey ) ||
                     MetaVariables.indexMarker == itemKey )
                {
                    continue;
                }

                textCompletionItems.Add( CreateTextCompletionItem( itemKey, item ) );
            }

            return textCompletionItems;
        }
        
        private static List<MetaVariable> ResolveDirectFunctionInvocations ( List<MetaVariable> filteredMetaVars, List<string> lookupKeys )
        {
            if ( filteredMetaVars.Count == 0 )
            {
                if ( lookupKeys[ 0 ].Contains( '(' ) )
                {
                    var functionKey = GeneratedRegex.CottleFunctionArgs().Replace( lookupKeys[ 0 ], string.Empty );
                    // If a match is found then we won't need to search our metavariables for a match.
                    var customFunction = customFunctions.FirstOrDefault( f => f.name == functionKey );
                    if ( customFunction != null )
                    {
                        var unfilteredMetaVars = new MetaVariables( customFunction.ReturnType, null, lookupKeys.Count + 1 ).Results;
                        unfilteredMetaVars.ForEach( mV =>
                            mV.keysPath = mV.keysPath.Prepend( lookupKeys[ 0 ] ).ToList() );
                        filteredMetaVars = FilterMetaVars( unfilteredMetaVars, lookupKeys );
                    }
                }
            }

            return filteredMetaVars;
        }

        private static List<MetaVariable> FilterMetaVars ( IEnumerable<MetaVariable> metaVariables, List<string> lookupKeys )
        {
            var filteredMetaVariables = metaVariables
                .Where( v => v.keysPath.Count == lookupKeys.Count + 1 )
                .Where( v => v.keysPath.Take( lookupKeys.Count )
                    .SequenceEqual( lookupKeys, StringComparer.Ordinal ) )
                .ToList();

            var localizedNameVar = filteredMetaVariables
                .FirstOrDefault( v => v.keysPath.Last() == "localizedName" );

            if ( localizedNameVar != null &&
                 filteredMetaVariables.Any( v =>
                     v.keysPath.Last() == "name" &&
                     Equals( v.value, localizedNameVar.value ) ) )
            {
                filteredMetaVariables.Remove( localizedNameVar );
            }

            return filteredMetaVariables;
        }

        private static TextCompletionItem CreateTextCompletionItem ( string itemKey, MetaVariable item )
        {
            if ( item.type == typeof( bool ) )
            {
                return new TextCompletionItem(
                    itemKey,
                    typeof( Cottle.Values.BooleanValue ),
                    item.description );
            }

            if ( item.type == typeof( int ) ||
                 item.type == typeof( double ) ||
                 item.type == typeof( float ) ||
                 item.type == typeof( long ) ||
                 item.type == typeof( ulong ) )
            {
                return new TextCompletionItem(
                    itemKey,
                    typeof( Cottle.Values.NumberValue ),
                    item.description );
            }

            if ( item.type == typeof( string ) )
            {
                return new TextCompletionItem(
                    itemKey,
                    typeof( Cottle.Values.StringValue ),
                    item.description );
            }

            if ( item.type == typeof( IEnumerable<> ) )
            {
                return new TextCompletionItem(
                    itemKey,
                    typeof( Cottle.Values.MapValue ),
                    item.description );
            }

            return new TextCompletionItem( itemKey, item.type, item.description );
        }
    }
}
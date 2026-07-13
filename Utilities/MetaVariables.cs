using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Utilities
{
    public class MetaVariables
    {
        public MetaVariables(
            Type reflectionObjectType,
            object reflectionObject = null,
            int? maxRecursionLevel = null,
            MetaVariableDiscoveryOptions options = null )
        {
            DiscoveryOptions = options ?? MetaVariableDiscoveryOptions.Runtime;
            Results = [];

            if ( reflectionObjectType is not null )
            {
                GetVariables( reflectionObjectType, maxRecursionLevel, reflectionObject );
            }

            Descriptors = Results.Select( v => v.Descriptor ).ToList().AsReadOnly();
            ValidateDescriptors( Descriptors );
        }

        public MetaVariables(
            IEnumerable<RuntimeVariableDeclaration> runtimeVariableDeclarations,
            int? maxRecursionLevel = null,
            MetaVariableDiscoveryOptions options = null )
        {
            DiscoveryOptions = options ?? MetaVariableDiscoveryOptions.Runtime;
            Results = [];

            foreach ( var declaration in runtimeVariableDeclarations ?? [] )
            {
                GetVariable(
                    [],
                    declaration.Definition.Name,
                    declaration.Definition.Type,
                    declaration.Description,
                    null,
                    maxRecursionLevel,
                    declaration.SourceType,
                    declaration.SourceMemberName,
                    declaration.ObsoleteAttribute );
            }

            Descriptors = Results.Select( v => v.Descriptor ).ToList().AsReadOnly();
            ValidateDescriptors( Descriptors );
        }

        public List<MetaVariable> Results { get; private set; }

        public IReadOnlyList<VariableDescriptor> Descriptors { get; private set; }

        private MetaVariableDiscoveryOptions DiscoveryOptions { get; }

        private static readonly HashSet<Type> undecomposedTypes =
        [
            typeof( string ),
            typeof( bool ),
            typeof( int ),
            typeof( uint ),
            typeof( decimal ),
            typeof( long ),
            typeof( ulong ),
            typeof( double ),
            typeof( float ),
            typeof( DateTime ),
            typeof( TimeSpan )
        ];

        public const string indexMarker = @"<index\>";

        private static readonly ConcurrentDictionary<Type, (PropertyInfo[], FieldInfo[])> typeCache = new();

        private static (PropertyInfo[], FieldInfo[]) GetTypeMembers ( Type type )
        {
            return typeCache.GetOrAdd( type, t => (
                t.GetProperties( BindingFlags.Public | BindingFlags.Instance ),
                t.GetFields( BindingFlags.Public | BindingFlags.Instance )
            ) );
        }

        private List<MetaVariable> GetVariables (
            Type reflectionObjectType,
            int? maxRecursionLevel,
            object reflectionObject = null,
            List<string> keysPath = null )
        {
            keysPath ??= [];

            if ( reflectionObjectType is null )
            {
                return Results;
            }

            if ( reflectionObjectType.IsGenericType &&
                 reflectionObjectType.GetGenericTypeDefinition() == typeof( Nullable<> ) )
            {
                reflectionObjectType = Nullable.GetUnderlyingType( reflectionObjectType );
            }

            if ( undecomposedTypes.Contains( reflectionObjectType ) )
            {
                GetVariable(
                    keysPath.ToList(),
                    string.Empty,
                    reflectionObjectType,
                    string.Empty,
                    reflectionObject,
                    maxRecursionLevel );
                return Results;
            }

            if ( IsEnumerableType( reflectionObjectType ) )
            {
                GetEnumerableVariables(
                    keysPath.ToList(),
                    reflectionObjectType,
                    string.Empty,
                    reflectionObject,
                    maxRecursionLevel,
                    addCollectionRoot: keysPath.Count > 0 );

                return Results;
            }

            var (objectProperties, objectFields) = GetTypeMembers( reflectionObjectType );

            foreach ( var eventProperty in objectProperties )
            {
                var publicAPIAttribute = eventProperty.GetCustomAttribute<PublicAPIAttribute>();
                if ( publicAPIAttribute is null )
                {
                    continue;
                }

                GetVariable(
                    keysPath.ToList(),
                    eventProperty.Name,
                    eventProperty.PropertyType,
                    publicAPIAttribute.Description,
                    eventProperty.CanRead && reflectionObject != null
                        ? ReadMemberValue( eventProperty, reflectionObject )
                        : null,
                    maxRecursionLevel,
                    eventProperty.DeclaringType,
                    eventProperty.Name,
                    eventProperty.GetCustomAttribute<ObsoleteAttribute>() );
            }

            foreach ( var eventField in objectFields )
            {
                var publicAPIAttribute = eventField.GetCustomAttribute<PublicAPIAttribute>();
                if ( publicAPIAttribute is null )
                {
                    continue;
                }

                GetVariable(
                    keysPath.ToList(),
                    eventField.Name,
                    eventField.FieldType,
                    publicAPIAttribute.Description,
                    reflectionObject != null ? ReadMemberValue( eventField, reflectionObject ) : null,
                    maxRecursionLevel,
                    eventField.DeclaringType,
                    eventField.Name,
                    eventField.GetCustomAttribute<ObsoleteAttribute>() );
            }

            return Results;
        }

        private object ReadMemberValue ( MemberInfo memberInfo, object reflectionObject )
        {
            try
            {
                return memberInfo switch
                {
                    PropertyInfo propertyInfo => propertyInfo.GetValue( reflectionObject ),
                    FieldInfo fieldInfo => fieldInfo.GetValue( reflectionObject ),
                    _ => null
                };
            }
            catch ( Exception ex )
            {
                HandleDiscoveryError(
                    $"Failed to read variable member '{memberInfo.DeclaringType?.FullName}.{memberInfo.Name}'.",
                    ex );
                return null;
            }
        }

        private void GetVariable (
            List<string> keysPath,
            string key,
            Type type,
            string description,
            object value,
            int? maxRecursionLevel,
            Type sourceType = null,
            string sourceMemberName = null,
            ObsoleteAttribute obsoleteAttribute = null )
        {
            try
            {
                var oldKeysPath = keysPath.ToList();
                keysPath.Add( key );

                if ( Results.Any( v => v.keysPath.SequenceEqual( keysPath ) ) )
                {
                    return;
                }

                if ( type != null &&
                     type.IsGenericType &&
                     type.GetGenericTypeDefinition() == typeof( Nullable<> ) )
                {
                    type = Nullable.GetUnderlyingType( type );
                }

                if ( type == typeof( bool ) )
                {
                    AddDescriptor( keysPath, type, description, value as bool?, sourceType, sourceMemberName, obsoleteAttribute );
                }
                else if ( type == typeof( string ) )
                {
                    AddDescriptor( keysPath, type, description, value as string, sourceType, sourceMemberName, obsoleteAttribute );
                }
                else if ( type == typeof( int ) )
                {
                    AddDescriptor( keysPath, type, description, value as int?, sourceType, sourceMemberName, obsoleteAttribute );
                }
                else if ( type == typeof( uint ) ||
                          type == typeof( long ) )
                {
                    AddDescriptor( keysPath, type, description, value is null ? null : Convert.ToInt64( value, CultureInfo.InvariantCulture ), sourceType, sourceMemberName, obsoleteAttribute );
                }
                else if ( type == typeof( ulong ) )
                {
                    AddDescriptor( keysPath, type, description, value is null ? null : Convert.ToUInt64( value, CultureInfo.InvariantCulture ), sourceType, sourceMemberName, obsoleteAttribute );
                }
                else if ( type == typeof( double ) )
                {
                    AddDescriptor( keysPath, type, description, value is null ? null : Convert.ToDouble( value, CultureInfo.InvariantCulture ), sourceType, sourceMemberName, obsoleteAttribute );
                }
                else if ( type == typeof( float ) )
                {
                    AddDescriptor( keysPath, type, description, value is null ? null : Convert.ToSingle( value, CultureInfo.InvariantCulture ), sourceType, sourceMemberName, obsoleteAttribute );
                }
                else if ( type == typeof( decimal ) )
                {
                    AddDescriptor( keysPath, type, description, value is null ? null : Convert.ToDecimal( value, CultureInfo.InvariantCulture ), sourceType, sourceMemberName, obsoleteAttribute );
                }
                else if ( type == typeof( DateTime ) )
                {
                    AddDescriptor(
                        keysPath,
                        type,
                        description,
                        value is null ? null : Convert.ToDateTime( value, CultureInfo.InvariantCulture ),
                        sourceType,
                        sourceMemberName,
                        obsoleteAttribute );
                }
                else if ( type is null )
                {
                    AddDescriptor( keysPath, null, description, null, sourceType, sourceMemberName, obsoleteAttribute );
                }
                else if ( !type.IsGenericType && type.IsEnum )
                {
                    var enumName = value != null ? Enum.GetName( type, value ) : null;
                    AddDescriptor( keysPath, typeof( string ), description, enumName, sourceType, sourceMemberName, obsoleteAttribute );
                }
                else if ( IsEnumerableType( type ) )
                {
                    GetEnumerableVariables(
                        keysPath,
                        type,
                        description,
                        value,
                        maxRecursionLevel );
                }
                else
                {
                    if ( undecomposedTypes.Contains( type ) )
                    {
                        return;
                    }

                    if ( (type.IsGenericType && type.GetGenericTypeDefinition() == typeof( Dictionary<,> )) ||
                         type.GetInterfaces().Contains( typeof( IDictionary ) ) )
                    {
                        if ( value != null )
                        {
                            foreach ( DictionaryEntry kvp in (IDictionary)value )
                            {
                                if ( kvp.Value != null )
                                {
                                    GetVariable(
                                        oldKeysPath,
                                        kvp.Key.ToString(),
                                        kvp.Value.GetType(),
                                        description,
                                        kvp.Value,
                                        maxRecursionLevel,
                                        sourceType,
                                        sourceMemberName,
                                        obsoleteAttribute );
                                }
                            }
                        }
                    }
                    else if ( ( type.IsClass || type.IsInterface ) && !type.IsGenericType )
                    {
                        AddDescriptor(
                            keysPath,
                            typeof( object ),
                            description,
                            null,
                            sourceType,
                            sourceMemberName,
                            obsoleteAttribute,
                            isObjectRoot: true,
                            declaredType: type );

                        if ( maxRecursionLevel is null || keysPath.Count < maxRecursionLevel )
                        {
                            GetVariables( type, maxRecursionLevel, value, keysPath );
                        }
                    }
                    else
                    {
                        throw new MetaVariableDiscoveryException( $"Unexpected variable type '{type.FullName}'." );
                    }
                }
            }
            catch ( Exception ex )
            {
                HandleDiscoveryError( "Failed to obtain variable metadata by reflection.", ex );
            }
        }

        private void GetEnumerableVariables (
            List<string> keysPath,
            Type type,
            string description,
            object value,
            int? maxRecursionLevel,
            bool addCollectionRoot = true )
        {
            var elementType = GetEnumerableElementType( type );

            int? i = 0;

            if ( value != null )
            {
                foreach ( var item in (IEnumerable)value )
                {
                    i++;

                    var elementKeysPath = keysPath.ToList();
                    elementKeysPath.Add( i.ToString() );

                    if ( maxRecursionLevel is null || keysPath.Count < maxRecursionLevel )
                    {
                        GetVariables( elementType, maxRecursionLevel, item, elementKeysPath );
                    }
                }
            }
            else
            {
                var elementKeysPath = keysPath.ToList();
                elementKeysPath.Add( indexMarker );

                if ( maxRecursionLevel is null || keysPath.Count < maxRecursionLevel )
                {
                    GetVariables( elementType, maxRecursionLevel, null, elementKeysPath );
                }

                i = null;
            }

            if ( addCollectionRoot && keysPath.Count > 0 )
            {
                AddDescriptor(
                    keysPath.ToList(),
                    typeof( IEnumerable<> ),
                    description,
                    i,
                    isCollectionRoot: true );
            }
        }

        private void AddDescriptor (
            List<string> keysPath,
            Type type,
            string description,
            object value,
            Type sourceType = null,
            string sourceMemberName = null,
            ObsoleteAttribute obsoleteAttribute = null,
            bool isCollectionRoot = false,
            bool isObjectRoot = false,
            Type declaredType = null )
        {
            var descriptor = VariableDescriptor.Create(
                keysPath,
                type,
                description,
                value,
                sourceType,
                sourceMemberName,
                obsoleteAttribute,
                isCollectionRoot,
                isObjectRoot,
                declaredType,
                DiscoveryOptions );

            Results.Add( new MetaVariable( descriptor ) );
        }

        private void ValidateDescriptors ( IReadOnlyList<VariableDescriptor> descriptors )
        {
            if ( DiscoveryOptions.Strict != true )
            {
                return;
            }

            var duplicateCottlePaths = descriptors
                .Where( d => d.KeysPath.Count > 0 &&
                             d.KeysPath[  d.KeysPath.Count  -  1  ] != indexMarker )
                .GroupBy( d => d.CottlePath )
                .Where( g => !string.IsNullOrEmpty( g.Key ) && g.Count() > 1 )
                .Select( g => g.Key )
                .ToList();

            if ( duplicateCottlePaths.Count > 0 )
            {
                throw new MetaVariableDiscoveryException(
                    "Duplicate Cottle variable paths discovered: " +
                    string.Join( ", ", duplicateCottlePaths ) );
            }

            var duplicateVoiceAttackPaths = descriptors
                .Where( d => d.VariableType != typeof( object ) )
                .GroupBy( d => d.RenderVoiceAttackName( string.Empty ) )
                .Where( g => !string.IsNullOrEmpty( g.Key ) && g.Count() > 1 )
                .Select( g => g.Key )
                .ToList();

            if ( duplicateVoiceAttackPaths.Count > 0 )
            {
                throw new MetaVariableDiscoveryException(
                    "Duplicate VoiceAttack variable paths discovered: " +
                    string.Join( ", ", duplicateVoiceAttackPaths ) );
            }

            if ( DiscoveryOptions.RequireDescriptions )
            {
                var missingDescriptions = descriptors
                    .Where( d => !string.IsNullOrEmpty( d.SourceMemberName ) )
                    .Where( d => string.IsNullOrWhiteSpace( d.Description ) )
                    .Where( d => !DiscoveryOptions.MissingDescriptionAllowlist.Contains( d.SourceId ) )
                    .Select( d => d.SourceId )
                    .Distinct()
                    .OrderBy( id => id )
                    .ToList();

                if ( missingDescriptions.Count > 0 )
                {
                    throw new MetaVariableDiscoveryException(
                        "User-facing PublicAPI variables are missing descriptions: " +
                        string.Join( ", ", missingDescriptions ) );
                }
            }
        }

        private void HandleDiscoveryError ( string message, Exception exception )
        {
            if ( DiscoveryOptions.Strict )
            {
                throw exception is MetaVariableDiscoveryException
                    ? exception
                    : new MetaVariableDiscoveryException( message, exception );
            }

            Logging.Error( message, exception );
        }

        private static bool IsEnumerableType ( Type type )
        {
            return type != typeof( string ) &&
                   typeof( IEnumerable ).IsAssignableFrom( type );
        }

        private static Type GetEnumerableElementType ( Type type )
        {
            if ( type.IsArray )
            {
                return type.GetElementType() ?? typeof( object );
            }

            if ( type.IsGenericType )
            {
                var genericTypeDefinition = type.GetGenericTypeDefinition();

                if ( genericTypeDefinition == typeof( Dictionary<,> ) ||
                     genericTypeDefinition == typeof( IDictionary<,> ) ||
                     genericTypeDefinition == typeof( IReadOnlyDictionary<,> ) )
                {
                    return type.GetGenericArguments()[ 1 ];
                }

                var genericArguments = type.GetGenericArguments();
                if ( genericArguments.Length > 0 )
                {
                    return genericArguments.Last();
                }
            }

            var enumerableInterface = type
                .GetInterfaces()
                .FirstOrDefault( i =>
                    i.IsGenericType &&
                    i.GetGenericTypeDefinition() == typeof( IEnumerable<> ) );

            return enumerableInterface?.GetGenericArguments().Last() ?? typeof( object );
        }
    }

    public class MetaVariable ( VariableDescriptor descriptor )
    {
        public MetaVariable ( List<string> keysPath, Type type, string description = null, object value = null )
            : this( VariableDescriptor.Create( keysPath, type, description, value ) )
        { }

        public VariableDescriptor Descriptor { get; } = descriptor;

        public List<string> keysPath { get; set; } = descriptor.KeysPath.ToList();

        public Type type { get; } = descriptor.VariableType;

        public string description { get; } = descriptor.Description;

        public object value { get; set; } = descriptor.Value;
    }

    public class CottleVariable ( List<string> keysPath, string description, object value )
    {
        public string key { get; } = VariablePathFormatter.RenderCottlePath( keysPath );

        public string description { get; } = description;

        public object value { get; } = value;

        public CottleVariable ( VariableDescriptor descriptor )
            : this( descriptor.KeysPath.ToList(), descriptor.Description, descriptor.Value )
        { }
    }

    [UsedImplicitly]
    public static class MetaVariablesExtensions
    {
        public static List<CottleVariable> AsCottleVariables ( this List<MetaVariable> source )
        {
            return source
                .Where( v => v.keysPath.Last() != MetaVariables.indexMarker )
                .Select( v => new CottleVariable( v.Descriptor ) )
                .ToList();
        }
    }

    public sealed class MetaVariableDiscoveryOptions
    {
        public static MetaVariableDiscoveryOptions Runtime { get; } = new();

        public static MetaVariableDiscoveryOptions StrictDocumentation { get; } = new()
        {
            Strict = true,
            RequireDescriptions = true
        };

        public bool Strict { get; init; }

        public bool RequireDescriptions { get; init; }

        public int MaxInlineAllowedValues { get; init; } = 64;

        public ISet<string> MissingDescriptionAllowlist { get; init; } =
            new HashSet<string>( StringComparer.Ordinal );
    }

    public class MetaVariableDiscoveryException : Exception
    {
        public MetaVariableDiscoveryException ( string message )
            : base( message )
        { }

        public MetaVariableDiscoveryException ( string message, Exception innerException )
            : base( message, innerException )
        { }
    }

    public sealed record VariableDescriptor
    {
        private VariableDescriptor (
            IReadOnlyList<string> keysPath,
            Type variableType,
            Type declaredType,
            string description,
            object value,
            Type sourceType,
            string sourceMemberName,
            bool isCollectionRoot,
            bool isObjectRoot,
            bool isObsolete,
            string obsoleteMessage,
            IReadOnlyList<VariableAllowedValue> allowedValues,
            string allowedValuesOmittedReason )
        {
            KeysPath = keysPath;
            VariableType = variableType;
            DeclaredType = declaredType;
            Description = description;
            Value = value;
            SourceType = sourceType;
            SourceMemberName = sourceMemberName;
            IsCollectionRoot = isCollectionRoot;
            IsObjectRoot = isObjectRoot;
            IsObsolete = isObsolete;
            ObsoleteMessage = obsoleteMessage;
            AllowedValues = allowedValues;
            AllowedValuesOmittedReason = allowedValuesOmittedReason;
        }

        public IReadOnlyList<string> KeysPath { get; }

        public Type VariableType { get; }

        public Type DeclaredType { get; }

        public string Description { get; }

        public object Value { get; }

        public Type SourceType { get; }

        public string SourceMemberName { get; }

        public string SourceId => SourceType is null || string.IsNullOrEmpty( SourceMemberName )
            ? string.Empty
            : $"{SourceType.FullName}.{SourceMemberName}";

        public bool IsCollectionRoot { get; }

        public bool IsObjectRoot { get; }

        public bool IsIndexed => KeysPath.Any( k => k == MetaVariables.indexMarker );

        public bool IsObsolete { get; }

        public string ObsoleteMessage { get; }

        public IReadOnlyList<VariableAllowedValue> AllowedValues { get; }

        public string AllowedValuesOmittedReason { get; }

        public string CottlePath => VariablePathFormatter.RenderCottlePath( KeysPath );

        public string VoiceAttackTypeName => VariablePathFormatter.RenderVoiceAttackTypeName( VariableType );

        public string RenderVoiceAttackName ( string startingPrefix, string eventType = null )
            => VariablePathFormatter.RenderVoiceAttackName( startingPrefix, eventType, KeysPath );

        public static VariableDescriptor Create (
            IEnumerable<string> keysPath,
            Type variableType,
            string description = null,
            object value = null,
            Type sourceType = null,
            string sourceMemberName = null,
            ObsoleteAttribute obsoleteAttribute = null,
            bool isCollectionRoot = false,
            bool isObjectRoot = false,
            Type declaredType = null,
            MetaVariableDiscoveryOptions options = null )
        {
            options ??= MetaVariableDiscoveryOptions.Runtime;

            var path = new ReadOnlyCollection<string>(
                ( keysPath ?? [] )
                .Select( k => k ?? string.Empty )
                .ToList() );

            var (allowedValues, omittedReason) = VariableAllowedValue.Discover(
                declaredType ?? variableType,
                options.MaxInlineAllowedValues );

            return new VariableDescriptor(
                path,
                variableType,
                declaredType ?? variableType,
                description,
                value,
                sourceType,
                sourceMemberName,
                isCollectionRoot,
                isObjectRoot,
                obsoleteAttribute != null,
                obsoleteAttribute?.Message,
                allowedValues,
                omittedReason );
        }
    }

    public sealed record VariableAllowedValue (
        string EdName,
        string InvariantName,
        string LocalizedName )
    {
        public static (IReadOnlyList<VariableAllowedValue> Values, string OmittedReason) Discover (
            Type type,
            int maxInlineAllowedValues )
        {
            if ( type is null )
            {
                return ([], null);
            }

            var localizedEdNameType = FindResourceBasedLocalizedEDNameType( type );
            if ( localizedEdNameType is null )
            {
                return ([], null);
            }

            var allOfThemProperty = localizedEdNameType.GetProperty(
                "AllOfThem",
                BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy );

            if ( allOfThemProperty?.GetValue( null ) is not IEnumerable allOfThem )
            {
                return ([], null);
            }

            var values = allOfThem
                .Cast<object>()
                .Select( item => new VariableAllowedValue(
                    ReadStringMember( item, "edname" ),
                    ReadStringMember( item, "invariantName" ),
                    ReadStringMember( item, "localizedName" ) ) )
                .Where( v => !string.IsNullOrEmpty( v.EdName ) ||
                             !string.IsNullOrEmpty( v.InvariantName ) ||
                             !string.IsNullOrEmpty( v.LocalizedName ) )
                .OrderBy( v => v.InvariantName, StringComparer.InvariantCulture )
                .ThenBy( v => v.EdName, StringComparer.InvariantCulture )
                .ToList();

            if ( values.Count > maxInlineAllowedValues )
            {
                return ([], $"{values.Count} values omitted by the inline value-list size policy.");
            }

            return (values.AsReadOnly(), null);
        }

        private static Type FindResourceBasedLocalizedEDNameType ( Type type )
        {
            while ( type != null )
            {
                if ( type.IsGenericType &&
                     type.GetGenericTypeDefinition().FullName == "EddiDataDefinitions.ResourceBasedLocalizedEDName`1" )
                {
                    return type;
                }

                type = type.BaseType;
            }

            return null;
        }

        private static string ReadStringMember ( object item, string memberName )
        {
            var type = item.GetType();
            var property = type.GetProperty( memberName, BindingFlags.Public | BindingFlags.Instance );
            if ( property != null )
            {
                return property.GetValue( item ) as string;
            }

            var field = type.GetField( memberName, BindingFlags.Public | BindingFlags.Instance );
            return field?.GetValue( item ) as string;
        }
    }

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

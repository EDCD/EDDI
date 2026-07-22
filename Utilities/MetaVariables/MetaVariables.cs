using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace Utilities.MetaVariables
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
                    GetPublicAPIDescription( publicAPIAttribute, eventProperty ),
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
                    GetPublicAPIDescription( publicAPIAttribute, eventField ),
                    reflectionObject != null ? ReadMemberValue( eventField, reflectionObject ) : null,
                    maxRecursionLevel,
                    eventField.DeclaringType,
                    eventField.Name,
                    eventField.GetCustomAttribute<ObsoleteAttribute>() );
            }

            return Results;
        }

        private static string GetPublicAPIDescription ( PublicAPIAttribute publicAPIAttribute, MemberInfo memberInfo )
        {
            if ( !string.IsNullOrWhiteSpace( publicAPIAttribute.Description ) )
            {
                return publicAPIAttribute.Description;
            }

            if ( IsResourceBasedLocalizedEDNameMember( memberInfo ) )
            {
                return memberInfo.Name switch
                {
                    "name" => "The localized name.",
                    "invariantName" => "The invariant English name.",
                    _ => publicAPIAttribute.Description
                };
            }

            return publicAPIAttribute.Description;
        }

        private static bool IsResourceBasedLocalizedEDNameMember ( MemberInfo memberInfo )
        {
            var declaringType = memberInfo.DeclaringType;
            while ( declaringType != null )
            {
                if ( declaringType.IsGenericType &&
                     declaringType.GetGenericTypeDefinition().FullName ==
                     "EddiDataDefinitions.ResourceBasedLocalizedEDName`1" )
                {
                    return true;
                }

                declaringType = declaringType.BaseType;
            }

            return false;
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
}

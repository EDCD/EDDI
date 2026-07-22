using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Utilities
{
    public enum RuntimeVariableSourceKind
    {
        TopLevelRuntime,
        MonitorRuntime,
        VoiceAttackRuntime
    }

    public sealed record RuntimeVariableDefinition (
        string Name,
        Type Type,
        RuntimeVariableSourceKind SourceKind = RuntimeVariableSourceKind.MonitorRuntime,
        string VoiceAttackName = null,
        bool IntendedForVoiceAttack = false,
        bool CurrentlyEmittedByVoiceAttack = false,
        Type VoiceAttackType = null );

    public sealed record RuntimeVariableValue (
        string Name,
        Type Type,
        object Value,
        object VoiceAttackValue = null )
    {
        public object GetVoiceAttackValue () => VoiceAttackValue ?? Value;
    }

    public static class RuntimeVariableDefinitionExtensions
    {
        public static IReadOnlyList<RuntimeVariableDeclaration> DiscoverDeclarations ( Type declaringType )
        {
            return declaringType
                .GetProperties( BindingFlags.Public | BindingFlags.Static )
                .Where( property => typeof(RuntimeVariableDefinition).IsAssignableFrom( property.PropertyType ) )
                .Select( RuntimeVariableDeclaration.FromMember )
                .Where( declaration => declaration is not null )
                .ToList();
        }

        public static bool TryGetValue<T> ( this IEnumerable<RuntimeVariableValue> values, string name, out T value )
        {
            var variableValue = values.FirstOrDefault( d => d.Name == name );
            if ( variableValue?.Value is T typedValue )
            {
                value = typedValue;
                return true;
            }

            value = default;
            return false;
        }

        public static Dictionary<string, Tuple<Type, object>> ToRuntimeValueDictionary (
            this IEnumerable<RuntimeVariableValue> values )
        {
            return values.ToDictionary(
                value => value.Name,
                value => new Tuple<Type, object>( value.Type, value.Value ) );
        }
    }

    public sealed record RuntimeVariableDeclaration (
        RuntimeVariableDefinition Definition,
        string Description,
        Type SourceType,
        string SourceMemberName,
        ObsoleteAttribute ObsoleteAttribute )
    {
        public static RuntimeVariableDeclaration FromMember ( PropertyInfo propertyInfo )
        {
            var publicAPIAttribute = propertyInfo.GetCustomAttribute<PublicAPIAttribute>();
            if ( publicAPIAttribute is null )
            {
                return null;
            }

            var definition = (RuntimeVariableDefinition)propertyInfo.GetValue( null );
            if ( definition is null )
            {
                return null;
            }

            return new RuntimeVariableDeclaration(
                definition,
                publicAPIAttribute.Description,
                propertyInfo.DeclaringType,
                propertyInfo.Name,
                propertyInfo.GetCustomAttribute<ObsoleteAttribute>() );
        }
    }
}

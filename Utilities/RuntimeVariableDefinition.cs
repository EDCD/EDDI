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
        Func<object> ValueProvider,
        RuntimeVariableSourceKind SourceKind = RuntimeVariableSourceKind.MonitorRuntime,
        string VoiceAttackName = null,
        bool CurrentlyEmittedByVoiceAttack = false,
        Func<object> VoiceAttackValueProvider = null )
    {
        public object GetValue () => ValueProvider();

        public object GetVoiceAttackValue () => ( VoiceAttackValueProvider ?? ValueProvider )();
    }

    public static class RuntimeVariableDefinitionExtensions
    {
        public static IReadOnlyList<RuntimeVariableDeclaration> DiscoverDeclarations ( Type declaringType, object instance = null )
        {
            var bindingFlags = BindingFlags.Public |
                               ( instance is null ? BindingFlags.Static : BindingFlags.Instance );

            return declaringType
                .GetProperties( bindingFlags )
                .Where( property => typeof(RuntimeVariableDefinition).IsAssignableFrom( property.PropertyType ) )
                .Select( property => RuntimeVariableDeclaration.FromMember( property, instance ) )
                .Where( declaration => declaration is not null )
                .ToList();
        }

        public static bool TryGetValue<T> ( this IEnumerable<RuntimeVariableDefinition> definitions, string name, out T value )
        {
            var definition = definitions.FirstOrDefault( d => d.Name == name );
            if ( definition?.GetValue() is T typedValue )
            {
                value = typedValue;
                return true;
            }

            value = default;
            return false;
        }

        public static Dictionary<string, Tuple<Type, object>> ToRuntimeValueDictionary (
            this IEnumerable<RuntimeVariableDefinition> definitions )
        {
            return definitions.ToDictionary(
                definition => definition.Name,
                definition => new Tuple<Type, object>( definition.Type, definition.GetValue() ) );
        }
    }

    public sealed record RuntimeVariableDeclaration (
        RuntimeVariableDefinition Definition,
        string Description,
        Type SourceType,
        string SourceMemberName,
        ObsoleteAttribute ObsoleteAttribute )
    {
        public static RuntimeVariableDeclaration FromMember ( PropertyInfo propertyInfo, object instance = null )
        {
            var publicAPIAttribute = propertyInfo.GetCustomAttribute<PublicAPIAttribute>();
            if ( publicAPIAttribute is null )
            {
                return null;
            }

            var definition = (RuntimeVariableDefinition)propertyInfo.GetValue( instance );
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

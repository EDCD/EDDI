using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;

namespace Utilities.MetaVariables
{
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

            var resourceLockField = localizedEdNameType.GetField(
                "resourceLock",
                BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy );
            var resourceLock = resourceLockField?.GetValue( null );
            List<object> allOfThemSnapshot;
            if ( resourceLock != null )
            {
                lock ( resourceLock )
                {
                    allOfThemSnapshot = allOfThem.Cast<object>().ToList();
                }
            }
            else
            {
                allOfThemSnapshot = allOfThem.Cast<object>().ToList();
            }

            var values = allOfThemSnapshot
                .Select( item => new VariableAllowedValue(
                    ReadStringMember( item, "edname" ),
                    ReadStringMember( item, "invariantName" ),
                    ReadStringMember( item, "localizedName" ) ) )
                .Where( v => !string.IsNullOrEmpty( v.EdName ) ||
                             !string.IsNullOrEmpty( v.InvariantName ) ||
                             !string.IsNullOrEmpty( v.LocalizedName ) )
                .GroupBy( v => new { v.EdName, v.InvariantName, v.LocalizedName } )
                .Select( g => g.First() )
                .OrderBy( v => v.InvariantName, StringComparer.InvariantCulture )
                .ThenBy( v => v.EdName, StringComparer.InvariantCulture )
                .ToList();

            if ( values.Count > maxInlineAllowedValues )
            {
                return ([], "Values omitted by the inline value-list size policy.");
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
}

using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Utilities.MetaVariables
{
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
}

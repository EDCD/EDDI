using EddiDataDefinitions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Resources;

namespace EddiMaterialMonitor
{
    internal static class MaterialViewFilter
    {
        internal static List<Material> Definitions()
        {
            lock (Material.resourceLock) { return Material.AllOfThem.ToList(); }
        }

        internal static bool Matches(MaterialAmount item, string search, string category, int? grade, string status)
        {
            if (item.amount == 0 &&
                (item.MaterialDef?.Category == null || item.MaterialDef.Category == MaterialCategory.Unknown ||
                 item.Rarity == Rarity.Unknown))
            {
                return false;
            }

            var query = search?.Trim() ?? "";
            return (query.Length == 0 || new[] { item.material, item.MaterialDef?.invariantName, item.edname }
                       .Any(name => name?.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0))
                   && (category == null || item.MaterialDef?.Category?.edname == category)
                   && (grade == null || item.Rarity.level == grade)
                   && (status switch
                   {
                       "Owned" => item.amount > 0,
                       "BelowMinimum" => item.minimum.HasValue && item.amount < item.minimum.Value,
                       "BelowDesired" => item.desired.HasValue && item.amount < item.desired.Value,
                       _ => true
                   });
        }

        internal static bool HasLocalizedNames(CultureInfo culture, IEnumerable<Material> definitions)
        {
            var materials = definitions.Where(m => m != null).ToList();
            // Use fresh managers: a manager used for GetString may cache a parent's resource
            // set under a child culture, obscuring the provenance we need here.
            var assembly = typeof(Material).Assembly;
            var neutralCulture = assembly.GetCustomAttribute<NeutralResourcesLanguageAttribute>()?.CultureName ?? "";
            var names = new ResourceManager(EddiDataDefinitions.Properties.Materials.ResourceManager.BaseName, assembly);
            var categories = new ResourceManager(EddiDataDefinitions.Properties.MaterialCategories.ResourceManager.BaseName, assembly);
            try
            {
                return HasLocalizedResources(culture, neutralCulture, materials.Select(m => m.basename),
                           c => names.GetResourceSet(c, true, false))
                       || HasLocalizedResources(culture, neutralCulture, materials.Select(m => m.Category?.basename),
                           c => categories.GetResourceSet(c, true, false));
            }
            finally
            {
                names.ReleaseAllResources();
                categories.ReleaseAllResources();
            }
        }

        internal static bool HasLocalizedResources(CultureInfo culture, string neutralCulture,
            IEnumerable<string> keys, Func<CultureInfo, ResourceSet> getExactResourceSet)
        {
            var applicableKeys = keys.Where(k => k != null).Distinct().ToArray();
            for (var current = culture; !Equals(current, CultureInfo.InvariantCulture); current = current.Parent)
            {
                if (current.Name.Equals(neutralCulture, StringComparison.OrdinalIgnoreCase)) { break; }
                var resources = getExactResourceSet(current);
                if (resources != null && applicableKeys.Any(k => resources.GetString(k) != null)) { return true; }
            }
            return false;
        }
    }
}

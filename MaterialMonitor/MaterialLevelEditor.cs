using EddiDataDefinitions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EddiMaterialMonitor
{
    internal enum MaterialLevelAction
    {
        LeaveUnchanged,
        FillBlanks,
        ReplaceValues,
        ClearThreshold
    }

    internal enum MaterialLevelUnits
    {
        Quantity,
        Percentage
    }

    internal sealed record MaterialLevelChange(
        string EDName,
        int? PreviousMinimum,
        int? PreviousDesired,
        int? NewMinimum,
        int? NewDesired);

    internal enum MaterialLevelValidationError
    {
        None,
        Negative,
        ExceedsCapacity,
        MinimumExceedsDesired
    }

    internal sealed record MaterialLevelConflict(string EDName, MaterialLevelValidationError Error);

    internal sealed class MaterialLevelPreview
    {
        internal int TargetCount { get; init; }
        internal IReadOnlyList<MaterialLevelChange> Changes { get; init; } = [];
        internal IReadOnlyList<MaterialLevelConflict> Conflicts { get; init; } = [];
        internal string SourceSignature { get; init; } = "";

        internal string Signature => SourceSignature + "#" + string.Join("|", Changes.Select(c =>
            $"{c.EDName}:{c.PreviousMinimum}:{c.PreviousDesired}:{c.NewMinimum}:{c.NewDesired}"))
            + "#" + string.Join("|", Conflicts.Select(c => $"{c.EDName}:{c.Error}"));
    }

    internal static class MaterialLevelEditor
    {
        internal static MaterialLevelPreview Preview(
            IEnumerable<MaterialAmount> targets,
            MaterialLevelAction minimumAction,
            int? minimumValue,
            MaterialLevelAction desiredAction,
            int? desiredValue,
            MaterialLevelUnits units)
        {
            var targetList = targets.Distinct().ToList();
            var changes = new List<MaterialLevelChange>();
            var conflicts = new List<MaterialLevelConflict>();

            foreach (var material in targetList)
            {
                var nextMinimum = ApplyAction(material.minimum, minimumAction,
                    CalculateValue(material, minimumValue, units));
                var nextDesired = ApplyAction(material.desired, desiredAction,
                    CalculateValue(material, desiredValue, units));
                if (nextMinimum == material.minimum && nextDesired == material.desired) { continue; }

                if (!IsValid(nextMinimum, nextDesired, material.maximum, out var error))
                {
                    conflicts.Add(new MaterialLevelConflict(material.edname, error));
                    continue;
                }

                changes.Add(new MaterialLevelChange(material.edname, material.minimum, material.desired,
                    nextMinimum, nextDesired));
            }

            return new MaterialLevelPreview
            {
                TargetCount = targetList.Count,
                Changes = changes,
                Conflicts = conflicts,
                SourceSignature = string.Join("|", targetList.OrderBy(m => m.edname)
                    .Select(m => $"{m.edname}:{m.minimum}:{m.desired}:{m.maximum}"))
            };
        }

        internal static bool IsValid(int? minimum, int? desired, int? capacity, out MaterialLevelValidationError error)
        {
            if (minimum < 0 || desired < 0)
            {
                error = MaterialLevelValidationError.Negative;
                return false;
            }
            if ((minimum.HasValue && minimum > capacity) || (desired.HasValue && desired > capacity))
            {
                error = MaterialLevelValidationError.ExceedsCapacity;
                return false;
            }
            if (minimum.HasValue && desired.HasValue && minimum > desired)
            {
                error = MaterialLevelValidationError.MinimumExceedsDesired;
                return false;
            }
            error = MaterialLevelValidationError.None;
            return true;
        }

        private static int? CalculateValue(MaterialAmount material, int? value, MaterialLevelUnits units)
        {
            if (!value.HasValue) { return null; }
            return units == MaterialLevelUnits.Percentage
                ? (int)Math.Floor((material.maximum ?? 0) * value.Value / 100M)
                : value;
        }

        private static int? ApplyAction(int? current, MaterialLevelAction action, int? value) => action switch
        {
            MaterialLevelAction.FillBlanks => current ?? value,
            MaterialLevelAction.ReplaceValues => value,
            MaterialLevelAction.ClearThreshold => null,
            _ => current
        };
    }
}

using AmphetamineSerializer.Common;
using AmphetamineSerializer.Common.Attributes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace AmphetamineSerializer
{
    /// <summary>
    /// 
    /// </summary>
    static public class VersionHelper
    {
        /// <summary>
        /// Get the intervals between the min version and the max version
        /// </summary>
        /// <param name="rootType"></param>
        /// <returns></returns>
        static public IEnumerable<object> GetExplicitlyManagedVersions(Type rootType)
        {
            var versionField = GetAllFields(null, rootType).First();

            if (versionField.Field.FieldType != typeof(int))
                return GetNonNumericVersions(rootType);
            else
                return GetNumericVersions(rootType);
        }

        private static IEnumerable<object> GetNonNumericVersions(Type rootType)
        {
            return GetAllFields(null, rootType)
                .Where(x=>x.Attribute.Version != null)
                .Select(x=>x.Attribute.Version)
                .Distinct();
        }

        private static IEnumerable<object> GetNumericVersions(Type rootType)
        {
            int? maxExplicitlyManagedVersion = null;
            int? minExplicitlyManagedVersion = null;

            if (GetFields(null, rootType).Where(x => x.Attribute.VersionBegin != -1).Any())
            {
                minExplicitlyManagedVersion = GetFields(null, rootType)
                                                .Where(x => x.Attribute.VersionBegin != -1)
                                                .Select(x => x.Attribute.VersionBegin)
                                                .Min();
            }

            if (GetFields(null, rootType).Where(x => x.Attribute.VersionEnd != -1).Any())
            {
                maxExplicitlyManagedVersion = GetFields(null, rootType)
                                                .Where(x => x.Attribute.VersionBegin != -1)
                                                .Select(x => x.Attribute.VersionBegin)
                                                .Max();
            }

            if (maxExplicitlyManagedVersion == null && minExplicitlyManagedVersion != null)
                maxExplicitlyManagedVersion = minExplicitlyManagedVersion;
            else if (maxExplicitlyManagedVersion != null && minExplicitlyManagedVersion == null)
                minExplicitlyManagedVersion = maxExplicitlyManagedVersion;

            Debug.Assert(maxExplicitlyManagedVersion.HasValue == minExplicitlyManagedVersion.HasValue);

            if (maxExplicitlyManagedVersion.HasValue)
                return (IEnumerable<object>)Enumerable.Range(minExplicitlyManagedVersion.Value, (maxExplicitlyManagedVersion.Value - minExplicitlyManagedVersion.Value) + 1);
            else
                return (IEnumerable<object>)Enumerable.Empty<int>();
        }

        /// <summary>
        /// Get the fields from a type for a given version.
        /// </summary>
        /// <param name="rootType">Type</param>
        /// <param name="version">Version</param>
        /// <returns>All the fields that match the given version</returns>
        static public IEnumerable<FieldElement> GetVersionSnapshot(IElement instance, Type rootType, object version)
        {
            return GetFields(instance, rootType, version);
        }
        
        /// <summary>
        /// Get all fields from a type.
        /// </summary>
        /// <param name="rootType"></param>
        /// <returns>All the fields contained in a type</returns>
        static public IEnumerable<FieldElement> GetAllFields(IElement instance, Type rootType)
        {
            return GetFields(instance, rootType);
        }

        /// <summary>
        /// Extract the fields to deserialize.
        /// </summary>
        /// <param name="rootType">Context</param>
        /// <param name="version">Version used to filter fields.</param>
        static private IEnumerable<FieldElement> GetFields(IElement instance, Type rootType, object version = null)
        {
            var attributes = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

            var fields = rootType
                            .GetFields(attributes)
                            .Where(x => x.GetCustomAttribute<ASIndexAttribute>(false) != null)
                            .OrderBy(x => x.GetCustomAttribute<ASIndexAttribute>(false).Index)
                            .Select(x=> new FieldElement(instance, x));

            if (version != null && version.GetType() == typeof(int))
            {
                fields = fields
                            .Where(x => !(x.Attribute.VersionBegin != -1) ||
                                         x.Attribute.VersionBegin <= (int)version)
                            .Where(x => !(x.Attribute.VersionEnd != -1) ||
                                         x.Attribute.VersionEnd >= (int)version)
                            .OrderBy(x => x.Attribute.Index);
            }
            else
            {
                fields = fields.Where(x => x.Attribute.Version == version);
            }

            return fields;
        }
    }
}

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
        static public IEnumerable<int> GetExplicitlyManagedVersions(Type rootType)
        {
            int? maxExplicitlyManagedVersion = null;
            int? minExplicitlyManagedVersion = null;

            foreach (var item in GetFields(null, rootType))
            {
                var attribute = item.Attribute;
                if (attribute.VersionBegin != -1)
                {
                    if (!minExplicitlyManagedVersion.HasValue)
                        minExplicitlyManagedVersion = attribute.VersionBegin;
                    else
                        minExplicitlyManagedVersion = Math.Min(minExplicitlyManagedVersion.Value, attribute.VersionBegin);

                    if (!maxExplicitlyManagedVersion.HasValue)
                        maxExplicitlyManagedVersion = attribute.VersionBegin;
                    else
                        maxExplicitlyManagedVersion = Math.Max(maxExplicitlyManagedVersion.Value, attribute.VersionBegin);
                }

                if (attribute.VersionEnd != -1)
                {
                    if (!maxExplicitlyManagedVersion.HasValue)
                        minExplicitlyManagedVersion = attribute.VersionEnd;
                    else
                        minExplicitlyManagedVersion = Math.Min(minExplicitlyManagedVersion.Value, attribute.VersionEnd);

                    if (minExplicitlyManagedVersion.HasValue)
                        maxExplicitlyManagedVersion = attribute.VersionEnd;
                    else
                        maxExplicitlyManagedVersion = Math.Max(maxExplicitlyManagedVersion.Value, attribute.VersionEnd);
                }
            }

            Debug.Assert(maxExplicitlyManagedVersion.HasValue == minExplicitlyManagedVersion.HasValue);

            if (maxExplicitlyManagedVersion.HasValue)
                return Enumerable.Range(minExplicitlyManagedVersion.Value, (maxExplicitlyManagedVersion.Value - minExplicitlyManagedVersion.Value) + 1);
            else
                return Enumerable.Empty<int>();
        }

        /// <summary>
        /// Get the fields from a type for a given version.
        /// </summary>
        /// <param name="rootType">Type</param>
        /// <param name="version">Version</param>
        /// <returns>All the fields that match the given version</returns>
        static public IEnumerable<FieldElement> GetVersionSnapshot(IElement instance, Type rootType, int version)
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
        static private IEnumerable<FieldElement> GetFields(IElement instance, Type rootType, int? version = null)
        {
            var attributes = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

            var allFields = rootType
                            .GetFields(attributes)
                            .Where(x => x.GetCustomAttribute<ASIndexAttribute>(false) != null)
                            .OrderBy(x => x.GetCustomAttribute<ASIndexAttribute>(false).Index)
                            .Select(x=> new FieldElement(instance, x));

            if (version.HasValue)
            {
                return allFields
                          .Where(x => !(x.Attribute.VersionBegin != -1) ||
                                       x.Attribute.VersionBegin <= version.Value)
                          .Where(x => !(x.Attribute.VersionEnd != -1) ||
                                       x.Attribute.VersionEnd >= version.Value)
                          .OrderBy(x => x.Attribute.Index);
            }

            return allFields;
        }
    }
}

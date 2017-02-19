using AmphetamineSerializer.Common;
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
    public class VersionHelper
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

            foreach (var item in GetFields(rootType))
            {
                var attribute = item.GetCustomAttribute<ASIndexAttribute>(false);
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
        /// 
        /// </summary>
        /// <param name="rootType"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        static public IEnumerable<FieldInfo> GetVersionSnapshot(Type rootType, int version)
        {
            return GetFields(rootType, version);
        }
        
        static public IEnumerable<FieldInfo> GetAllFields(Type rootType)
        {
            return GetFields(rootType);
        }

        /// <summary>
        /// Extract the fields to deserialize.
        /// </summary>
        /// <param name="rootType">Context</param>
        /// <param name="version">Version used to filter fields.</param>
        static private IEnumerable<FieldInfo> GetFields(Type rootType, int? version = null)
        {
            var allFields = rootType
                            .GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                            .Where(x => x.GetCustomAttribute<ASIndexAttribute>(false) != null)
                            .OrderBy(x => x.GetCustomAttribute<ASIndexAttribute>(false).Index);

            if (version.HasValue)
            {
                return allFields
                          .Where(x => !(x.GetCustomAttribute<ASIndexAttribute>().VersionBegin != -1) ||
                                       x.GetCustomAttribute<ASIndexAttribute>().VersionBegin <= version.Value)
                          .Where(x => !(x.GetCustomAttribute<ASIndexAttribute>().VersionEnd != -1) ||
                                       x.GetCustomAttribute<ASIndexAttribute>().VersionEnd >= version.Value)
                          .OrderBy(x => x.GetCustomAttribute<ASIndexAttribute>().Index);
            }

            return allFields;
        }
    }
}

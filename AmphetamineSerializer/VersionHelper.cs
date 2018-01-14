using AmphetamineSerializer.Common.Element;
using AmphetamineSerializer.Interfaces;
using AmphetamineSerializer.Model.Attributes;
using System;
using System.Collections.Generic;
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
            var versionField = GetSerializableMembers(null, rootType).First();

            if (versionField.LoadedType != typeof(int))
                return GetNonNumericVersions(rootType);
            else
                return GetNumericVersions(rootType);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rootType"></param>
        /// <returns></returns>
        private static IEnumerable<object> GetNonNumericVersions(Type rootType)
        {
            return GetSerializableMembers(null, rootType)
                .Where(x=>x.Attribute.Version != null)
                .Select(x=>x.Attribute.Version)
                .Distinct();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rootType"></param>
        /// <returns></returns>
        private static IEnumerable<object> GetNumericVersions(Type rootType)
        {
            int? maxExplicitlyManagedVersion = null;
            int? minExplicitlyManagedVersion = null;

            var vBegin = GetMembers(null, rootType)
                            .Where(x => x.Attribute.VersionBegin != -1)
                            .Select(x => x.Attribute.VersionBegin)
                            .Distinct();

            var vEnd = GetMembers(null, rootType)
                            .Where(x => x.Attribute.VersionEnd != -1)
                            .Select(x => x.Attribute.VersionEnd)
                            .Distinct();
            
            var vSpecific = GetMembers(null, rootType)
                            .Where(x => x.Attribute.Version != null)
                            .Select(x => (int)x.Attribute.Version)
                            .Distinct();

            if (vBegin.Any() || vEnd.Any())
            {
                minExplicitlyManagedVersion = vBegin.Concat(vEnd).Concat(vSpecific).Min();
                maxExplicitlyManagedVersion = vBegin.Concat(vEnd).Concat(vSpecific).Max();
            }

            //TODO: check for nonsense interval
            
            if (maxExplicitlyManagedVersion.HasValue)
                return Enumerable
                        .Range(minExplicitlyManagedVersion.Value, (maxExplicitlyManagedVersion.Value - minExplicitlyManagedVersion.Value) + 1)
                        .Cast<object>();
            else
                return Enumerable.Empty<object>();
        }

        /// <summary>
        /// Get the fields from a type for a given version.
        /// </summary>
        /// <param name="rootType">Type</param>
        /// <param name="version">Version</param>
        /// <returns>All the fields that match the given version</returns>
        static public IEnumerable<MemberElement> GetVersionSnapshot(IElement instance, Type rootType, object version)
        {
            return GetMembers(instance, rootType, version);
        }
        
        /// <summary>
        /// Get all fields from a type.
        /// </summary>
        /// <param name="rootType"></param>
        /// <returns>All the fields contained in a type</returns>
        static public IEnumerable<MemberElement> GetSerializableMembers(IElement instance, Type rootType)
        {
            return GetMembers(instance, rootType);
        }

        /// <summary>
        /// Extract the fields to deserialize.
        /// </summary>
        /// <param name="rootType">Context</param>
        /// <param name="version">Version used to filter fields.</param>
        static private IEnumerable<MemberElement> GetMembers(IElement instance, Type rootType, object version = null)
        {
            var attributes = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

            var fields = rootType
                            .GetFields(attributes)
                            .Where(x => x.GetCustomAttribute<ASIndexAttribute>(false) != null)
                            .OrderBy(x => x.GetCustomAttribute<ASIndexAttribute>(false).Index)
                            .Select(x=> new FieldElement(instance, x))
                            .Cast<MemberElement>();
            
            var properties = rootType
                            .GetProperties(attributes)
                            .Where(x => x.GetCustomAttribute<ASIndexAttribute>(false) != null)
                            .OrderBy(x => x.GetCustomAttribute<ASIndexAttribute>(false).Index)
                            .Select(x => new PropertyElement(instance, x))
                            .Cast<MemberElement>();

            var merged = fields.Concat(properties);

            if (version != null && version.GetType() == typeof(int))
            {
                merged = merged
                            .Where(x => x.Attribute.VersionBegin == -1 ||
                                         x.Attribute.VersionBegin <= (int)version ||
                                         (int)x.Attribute.Version == (int)version)
                            .Where(x => x.Attribute.VersionEnd == -1 ||
                                        x.Attribute.VersionEnd >= (int)version ||
                                        (int)x.Attribute.Version == (int)version)
                            .OrderBy(x => x.Attribute.Index);
            }

            return merged;
        }
    }
}

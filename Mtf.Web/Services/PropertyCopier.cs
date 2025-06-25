using Mtf.Web.Attributes;
using Mtf.Web.Interfaces;
using System;
using System.Linq;
using System.Reflection;

namespace LiveView.Web.Services
{
    public static class PropertyCopier
    {
        public static void CopyMatchingProperties<TSource, TTarget>(TSource source, TTarget target)
        {
            if (source == null || target == null)
            {
                return;
            }

            var sourceProps = typeof(TSource).GetProperties();
            var targetProps = typeof(TTarget).GetProperties();

            foreach (var sourceProp in sourceProps)
            {
                if (!sourceProp.CanRead)
                {
                    continue;
                }

                var targetProp = Array.Find(targetProps, p =>
                    p.Name == sourceProp.Name &&
                    p.CanWrite &&
                    p.PropertyType == sourceProp.PropertyType);

                if (targetProp != null)
                {
                    var value = sourceProp.GetValue(source);
                    targetProp.SetValue(target, value);
                }
            }
        }

        public static void CopyAllExceptExcluded<TSource, TTarget>(TSource source, TTarget target)
        {
            if (source == null || target == null)
            {
                return;
            }

            var sourceProps = typeof(TSource).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var targetProps = typeof(TTarget).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var sourceProp in sourceProps)
            {
                if (!sourceProp.CanRead || sourceProp.IsDefined(typeof(ExcludeAttribute), true))
                {
                    continue;
                }

                var targetProp = targetProps.FirstOrDefault(p =>
                    p.Name == sourceProp.Name &&
                    p.CanWrite &&
                    p.PropertyType == sourceProp.PropertyType);

                if (targetProp != null)
                {
                    var value = sourceProp.GetValue(source);
                    targetProp.SetValue(target, value);
                }
            }
        }

        public static void CopyOnlyIncluded<TSource, TTarget>(TSource source, TTarget target)
        {
            if (source == null || target == null)
            {
                return;
            }

            var sourceProps = typeof(TSource).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var targetProps = typeof(TTarget).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var sourceProp in sourceProps)
            {
                if (!sourceProp.CanRead || !sourceProp.IsDefined(typeof(IncludeAttribute), true))
                {
                    continue;
                }

                var targetProp = targetProps.FirstOrDefault(p =>
                    p.Name == sourceProp.Name &&
                    p.CanWrite &&
                    p.PropertyType == sourceProp.PropertyType);

                if (targetProp != null)
                {
                    var value = sourceProp.GetValue(source);
                    targetProp.SetValue(target, value);
                }
            }
        }

        public static void CopyWithMapping<TSource, TTarget>(TSource source, TTarget target, Func<PropertyInfo, bool> includeFilter = null)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            CopyProperties(source, target,
                includeFilter: includeFilter ?? (p => true),
                mapConverter: (srcProp, srcValue, tgtType) =>
                {
                    var mapAttr = srcProp.GetCustomAttribute<MapAttribute>(true);
                    if (mapAttr == null)
                    {
                        return (srcProp.Name, srcValue);
                    }

                    var converter = (IValueConverter)Activator.CreateInstance(mapAttr.ConverterType);
                    var converted = converter.Convert(srcValue);
                    return (mapAttr.TargetProperty, converted);
                });
        }

        private static void CopyProperties<TSource, TTarget>(
            TSource source,
            TTarget target,
            Func<PropertyInfo, bool> includeFilter,
            Func<PropertyInfo, object, Type, (string propName, object value)> mapConverter)
        {
            var sourceProps = typeof(TSource).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var targetProps = typeof(TTarget).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var sourceProp in sourceProps)
            {
                if (!sourceProp.CanRead || !includeFilter(sourceProp))
                {
                    continue;
                }

                var srcValue = sourceProp.GetValue(source);
                string name = sourceProp.Name;
                object value = srcValue;

                if (mapConverter != null)
                {
                    var result = mapConverter(sourceProp, srcValue, typeof(TTarget));
                    name = result.propName;
                    value = result.value;
                }

                var targetProperty = targetProps.FirstOrDefault(p =>
                    p.Name == name &&
                    p.CanWrite &&
                    p.PropertyType.IsAssignableFrom(value?.GetType() ?? p.PropertyType));

                targetProperty?.SetValue(target, value);
            }
        }
    }
}

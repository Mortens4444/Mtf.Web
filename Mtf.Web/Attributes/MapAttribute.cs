using System;

namespace Mtf.Web.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class MapAttribute : Attribute
    {
        public string TargetProperty { get; }
        public Type ConverterType { get; }

        public MapAttribute(string targetProperty, Type converterType)
        {
            TargetProperty = targetProperty;
            ConverterType = converterType;
        }
    }
}

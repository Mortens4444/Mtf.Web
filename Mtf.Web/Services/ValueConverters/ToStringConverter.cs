using Mtf.Web.Interfaces;
using System;

namespace Mtf.Web.Services.ValueConverters
{
    /// <summary>
    /// Example converter: converts any value to its string representation.
    /// </summary>
    public class ToStringConverter : IValueConverter
    {
        public object Convert(object value)
        {
            return value?.ToString() ?? String.Empty;
        }
    }
}

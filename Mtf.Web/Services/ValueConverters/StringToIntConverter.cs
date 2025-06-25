using Mtf.Web.Interfaces;
using System.Globalization;

namespace Mtf.Web.Services.ValueConverters
{
    /// <summary>
    /// Example converter: converts a string to an integer, or zero if invalid.
    /// </summary>
    public class StringToIntConverter : IValueConverter
    {
        public object Convert(object value)
        {
            return System.Convert.ToInt32(value, CultureInfo.InvariantCulture);
        }
    }
}

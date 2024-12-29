using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace LanguageModeSwitcherWpf.View;

public class GetDescriptionConverter : IValueConverter
{
    private string GetDescription(object obj)
    {
        FieldInfo fieldInfo = obj.GetType().GetField(obj.ToString());

        object[] attribArray = fieldInfo.GetCustomAttributes(false);

        if (attribArray.Length == 0)
        {
            return obj.ToString();
        }
        else
        {
            DescriptionAttribute attrib = attribArray[0] as DescriptionAttribute;
            return attrib.Description;
        }
    }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

using System.Collections.Generic;
using System.ComponentModel;

namespace CSE.Automation.Extensions
{
    public static class DictionaryHelper
    {
        public static IEnumerable<KeyValuePair<string, object>> GetValuesAsDictionary(object values)
        {
            if (values is IDictionary<string, object> valuesAsDictionary)
            {
                return valuesAsDictionary;
            }

            valuesAsDictionary = new Dictionary<string, object>();
            if (values != null)
            {
                PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(values);
                foreach (PropertyDescriptor prop in properties)
                {
                    object val = prop.GetValue(values);
                    valuesAsDictionary.Add(prop.Name, val);
                }
            }

            return valuesAsDictionary;
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReferenceConflictAnalyser.Utils
{
    public class EnumHelper
    {
        public static string GetDescription<T>(T value)
        {
            var type = typeof(T);
            var memInfo = type.GetMember(value.ToString());
            var attributes = memInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
            return attributes.Count() > 0
                ? ((DescriptionAttribute)attributes[0]).Description
                : value.ToString();
        }
    }
}

﻿using Common.Domain.Entities.Roles;
using Common.Domain.Entities.Users;
using System.ComponentModel;
using System.Reflection;

namespace Common.SharedKernel.Extensions
{
    public static class EnumExtension
    {
        public static string ToDescription<TEnum>(this TEnum EnumValue) where TEnum : struct
        {
            Type type = typeof(TEnum);

            MemberInfo[] memInfo = type.GetMember(EnumValue.ToString());

            if (memInfo != null && memInfo.Length > 0)
            {
                object[] attrs = memInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);

                if (attrs != null && attrs.Length > 0)
                {
                    return ((DescriptionAttribute)attrs[0]).Description;
                }
            }

            return EnumValue.ToString();
        }

        public static T GetEnumValueFromDescription<T>(string description) where T : Enum
        {
            foreach (var field in typeof(T).GetFields())
            {
                if (Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) is DescriptionAttribute attribute)
                {
                    if (attribute.Description.Equals(description, StringComparison.OrdinalIgnoreCase))
                    {
                        return (T)field.GetValue(null);
                    }
                }
                else
                {
                    // If the enum value doesn't have a Description attribute, match on the name
                    if (field.Name.Equals(description, StringComparison.OrdinalIgnoreCase))
                    {
                        return (T)field.GetValue(null);
                    }
                }
            }
            return default;
        }
    }
}
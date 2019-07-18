﻿using RapidCMS.Common.Models;
using RapidCMS.Common.Models.Config;

namespace RapidCMS.Common.Extensions
{
    internal static class PropertyConfigExtensions
    {
        public static Field ToField(this PropertyConfig property)
        {
            return new ExpressionField
            {
                Index = property.Index,

                Description = property.Description,
                Name = property.Name,
                Expression = property.Property,

                Readonly = true
            };
        }
    }
}
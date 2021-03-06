﻿using System;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace B2S.Web.Extensions
{
    public static class StringExtension
    {
        public static string Format(this string str, params Expression<Func<string, object>>[] args)
        {
            var parameters = args.ToDictionary(e => string.Format("{{{0}}}", e.Parameters[0].Name), e => e.Compile()(e.Parameters[0].Name));

            var sb = new StringBuilder(str);
            foreach (var kv in parameters)
            {
                sb.Replace(kv.Key, kv.Value != null ? kv.Value.ToString() : "");
            }

            return sb.ToString();
        }
    }
}

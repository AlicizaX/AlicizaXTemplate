using System;
using System.Collections.Generic;
using Luban;

namespace Game.Config
{
    internal static class ConfigLocalization
    {
        internal static Dictionary<string, string> Read(ByteBuf buffer)
        {
            int count = buffer.ReadSize();
            var values = new Dictionary<string, string>(count, StringComparer.Ordinal);
            for (int i = 0; i < count; i++)
            {
                string key = buffer.ReadString();
                values.Add(key, buffer.ReadBool() ? buffer.ReadString() : null);
            }
            return values;
        }
        internal static string Get(Dictionary<string, string> values, string key) =>
            key != null && values.TryGetValue(key, out var text) ? text ?? key : key;
    }
}

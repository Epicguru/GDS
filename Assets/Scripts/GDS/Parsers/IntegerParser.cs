using System;
using System.Collections.Generic;
using System.Xml;

namespace GDS.Parsers
{
    public class IntegerParser : NodeParser
    {
        private static readonly Dictionary<Type, (long min, ulong max)> typeRanges = new Dictionary<Type, (long min, ulong max)>()
        {
            {typeof(byte), (byte.MinValue, byte.MaxValue)},
            {typeof(sbyte), (sbyte.MinValue, (ulong)sbyte.MaxValue)},
            {typeof(short), (short.MinValue, (ulong)short.MaxValue)},
            {typeof(ushort), (ushort.MinValue, ushort.MaxValue)},
            {typeof(int), (int.MinValue, int.MaxValue)},
            {typeof(uint), (uint.MinValue, uint.MaxValue)},
            {typeof(long), (long.MinValue, long.MaxValue)},
            {typeof(ulong), ((long)ulong.MinValue, ulong.MaxValue)},
        };

        public override bool CanHandle(Type t) => typeRanges.ContainsKey(t);

        public override object TryParse(XmlNode node, Type expectedType, in ParserContext _)
        {
            if (!typeRanges.TryGetValue(expectedType, out var range))
                return null;

            string txt = node.InnerText?.Trim();
            if (string.IsNullOrWhiteSpace(txt))
            {
                Error($"No value provided for {expectedType.Name}.");
                return null;
            }

            if (txt[0] == '-')
            {
                if (!long.TryParse(txt, out var result))
                {
                    Error($"Failed to parse '{txt}' as a {expectedType.Name}.");
                    return null;
                }

                if (result < range.min)
                {
                    Error($"{result} is less than the minimum value of a {expectedType.Name} ({range.min}). Value will not be assigned.");
                    return null;
                }

                return Convert.ChangeType(result, expectedType);
            }
            else
            {
                if (!ulong.TryParse(txt, out var result))
                {
                    Error($"Failed to parse '{txt}' as a {expectedType.Name}.");
                    return null;
                }

                if (result > range.max)
                {
                    Error($"{result} is greater than the maximum value of a {expectedType.Name} ({range.max}). Value will not be assigned.");
                    return null;
                }

                return Convert.ChangeType(result, expectedType);
            }
        }
    }
}

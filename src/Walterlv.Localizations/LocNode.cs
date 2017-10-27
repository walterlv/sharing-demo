using System;

namespace Walterlv.Localizations
{
    public class LocNode
    {
        private readonly string _value;

        public LocNode()
        {
            _value = "";
        }

        public LocNode(string value)
        {
            _value = value ?? throw new ArgumentNullException(nameof(value));
        }

        public static implicit operator LocNode(string value)
        {
            return new LocNode(value);
        }

        public static implicit operator string(LocNode node)
        {
            return node._value;
        }
    }
}

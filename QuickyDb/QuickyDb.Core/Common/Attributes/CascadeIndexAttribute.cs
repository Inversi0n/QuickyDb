using System;
using System.Collections.Generic;
using System.Text;

namespace QuickyDb.Core.Common.Attributes
{

    [AttributeUsage(AttributeTargets.Property)]
    public sealed class CascadeIndexAttribute : Attribute
    {
        public string Name { get; set; }
        public uint Order { get; set; }
        public CascadeIndexAttribute(string name, uint order)
        {
            Name = name;
            Order = order;
        }
    }
}
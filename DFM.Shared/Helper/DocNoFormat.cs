using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFM.Shared.Helper
{
    public static class DocNoFormat
    {
        public static List<FormatMember> Formats = new List<FormatMember>
        {
            new FormatMember("[ເລກທີ]", "$docno"),
            new FormatMember("[ເລກທີ]/[ຕົວຫຍໍ້]", "$docno/$sn"),
            new FormatMember("[ເລກທີ]/[ປີ]", "$docno/$yyyy"),
            new FormatMember("[ເລກທີ]/[ຕົວຫຍໍ້]/[ປີ]", "$docno/$sn/$yyyy")
        };
    }
    public class FormatMember
    {
        public FormatMember(string? name, string? value)
        {
            Name = name;
            Value = value;
        }

        public string? Name { get; set; }
        public string? Value { get; set; }
    }
}

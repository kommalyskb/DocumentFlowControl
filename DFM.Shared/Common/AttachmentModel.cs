using Microsoft.AspNetCore.Components;
using Redis.OM.Modeling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFM.Shared.Common
{
    public class AttachmentModel
    {
        [Indexed]
        public string? Server { get; set; }
        [Indexed]
        public string? Bucket { get; set; }
        [Indexed]
        public string? FileName { get; set; }
        [Indexed]
        public string? Display { get; set; }
        [Indexed]
        public bool IsRemove { get; set; }
        [Indexed]
        public int Version { get; set; }
        [Indexed]
        public decimal FileSize { get; set; }
        [Indexed]
        public bool IsNewFile { get; set; }
        [Indexed]
        public string? FileFormat { get; set; }
        [Indexed]
        public string? MimeType { get; set; }
        

        public string? DisplayOnPage
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(Display))
                {
                    if (Display!.Length > 15)
                    {
                        return $"{Display!.Substring(0, 15)}...";
                    }
                    return Display;
                }
                return "";
            }
        }
    }
}

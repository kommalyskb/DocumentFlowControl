using DFM.Shared.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFM.Shared.DTOs
{
    public class MainReceiver
    {
        public string? Id { get; set; }
        public CommentModel? Comment { get; set; } = new();
    }
}

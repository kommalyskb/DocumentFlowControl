using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFM.Shared.Configurations
{
    public class EnvConf
    {
        public bool ByPassDepartment { get; set; }
        public string? LogoUrl { get; set; }
        public string? PageTitle { get; set; }
        public EmailEnum Option { get; set; }
        public bool EmailNotify { get; set; }

    }
    public enum EmailEnum
    {
        PURESMTP,
        SENDGRID
    }
}

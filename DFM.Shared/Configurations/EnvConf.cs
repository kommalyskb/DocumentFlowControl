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
        public bool EnableSendThroughParentOfficePrime { get; set; } // True = ແມ່ນໃນກໍລະນີ ຂາເຂົ້າ ບໍລິສັດສາມາດສົ່ງໃຫ້ Parent ຂອງ Office Prime ໃຫ້
        public string? LogoUrl { get; set; }
        public string? PageTitle { get; set; }
        public EmailEnum Option { get; set; }
        public bool EmailNotify { get; set; }
        public string? ContactPhone { get; set; }
        public string? ContactEmail { get; set; }
        public string? CopyRightCompany { get; set; }
    }
    public enum EmailEnum
    {
        PURESMTP,
        SENDGRID
    }
}

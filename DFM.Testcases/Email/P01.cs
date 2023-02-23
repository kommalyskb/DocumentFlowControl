using DFM.Shared.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace DFM.Testcases.Email
{
    public class P01
    {
        IEmailHelper emailHelper;
        public P01()
        {
            emailHelper = new EmailHelper(new Shared.Configurations.SMTPConf
            {
                Email = "doc.cse@csenergy.la",
                Password = "bcniqluyumopgbrb",
                Server = "smtp.office365.com",
                Port = 587
            }, new Shared.Configurations.SendGridConf(), new Shared.Configurations.EnvConf());
        }

        [Fact]
        public async Task Send()
        {
            await emailHelper.Send(new EmailProperty
            {
                Body = emailHelper.NotificationMailBody("https://codecamplao.com", "ຂາເຂົ້າຫ້ອງການ", "ແຈ້ງເຕືອນເອກະສານ"),
                To = new List<string> { "kommalyskb@gmail.com" },
                Subject = "ແຈ້ງເຕືອນເອກະສານ",
                From = "doc.cse@csenergy.la"
            });
        }
    }
}

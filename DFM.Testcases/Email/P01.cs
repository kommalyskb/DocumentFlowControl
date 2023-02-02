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
                Email = "docunent.cse2023@gmail.com",
                IsActivate = true,
                Password = "bcniqluyumopgbrb",
                Server = "smtp.gmail.com",
                Port = 587
            });
        }

        [Fact]
        public async Task Send()
        {
            await emailHelper.Send(new EmailProperty
            {
                Body = emailHelper.NotificationMailBody("https://codecamplao.com", "ຂາເຂົ້າຫ້ອງການ", "ແຈ້ງເຕືອນເອກະສານ"),
                To = new List<string> { "kommalyskb@gmail.com" },
                Subject = "ແຈ້ງເຕືອນເອກະສານ",
                From = "docunent.cse2023@gmail.com"
            });
        }
    }
}

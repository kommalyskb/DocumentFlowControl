using DFM.Shared.Configurations;
using DFM.Shared.Helper;
using SendGrid.Helpers.Mail;
using SendGrid;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Minio.DataModel;
using System.Net.Mail;
using System.Net;

namespace DFM.Testcases.Sendgrid
{
    public class TestSendMail
    {

        [Fact]
        public async Task SendAsync()
        {
            try
            {
                //var client = new SendGridClient("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");
                //var from = new EmailAddress("doc.cse@csenergy.la", "Sendgrid Notify");
                //List<EmailAddress> tos = new();
                //tos.Add(new EmailAddress("kommalyskb@gmail.com"));
                //var plainTextContent = "";
                string body = RegisterMailBody("KOMMALY SAYKHAMBAY","kommaly","888888");
                //var msg = MailHelper.CreateSingleEmailToMultipleRecipients(from, tos, "Demo Send email from send grid", plainTextContent, body);
                //var response = await client.SendEmailAsync(msg);
                //Log.Information($"Send email: {response.Body.ReadAsStringAsync().Result}");
                MailMessage message = new MailMessage();
                message.From = new MailAddress("doc.cse@csenergy.la");
                message.Subject = "CSE Document Notify";
                message.To.Add("kommalyskb@gmail.com");

                AlternateView htmlView = AlternateView.CreateAlternateViewFromString(
                  $"{body}",
                  null,
                  "text/html"
                );


                message.AlternateViews.Add(htmlView);
                using (SmtpClient smtp = new SmtpClient())
                {

                    smtp.Host = "smtp.office365.com";
                    smtp.Port = 587;
                    smtp.EnableSsl = true;

                    NetworkCredential netCre = new NetworkCredential("doc.cse@csenergy.la", "DCSE02@2023@");
                    smtp.UseDefaultCredentials = false;
                    smtp.Credentials = netCre;

                    await smtp.SendMailAsync(message);

                }
            }
            catch (Exception)
            {

                throw;
            }
        }
        public string RegisterMailBody(string name, string username, string password)
        {
            return $@"<!DOCTYPE html>
                    <html lang= 'en '>
                    <head>
                        <meta charset= 'UTF-8 '>
                        <meta http-equiv= 'X-UA-Compatible ' content= 'IE=edge '>
                        <meta name= 'viewport ' content= 'width=device-width, initial-scale=1.0 '>
                        <title>Welcome To Document Control Flow System</title>
                    </head>
                    <body style= 'background-color: #F1F1F1; padding-left: 28%;padding-right: 28%;'>

                        <div >
                            <div style= 'display: flex;justify-content: center; padding-top:2rem;padding-bottom:2rem;background-color: #20629b; '>
                                <h1 style='color: #FFFFFF; text-align: center'>Codecamp</h1>
                            </div>
                            <div  style= 'background-color: #FFFFFF; '>
       
                                    <div style='display: flex;padding:2rem 1rem 2rem 1rem;'>
                                        <h2 style= 'margin:0;font-weight: bold; '>ຍິນດີຕ້ອນຮັບເຂົ້າສູ່ ລະບົບຈໍລະຈອນເອກະສານ</h5>
                                    </div>
                                    <div style='display: flex;padding:0 1rem 1rem 1rem;'>
                                        <div style='padding-right: 1rem;'>ທ່ານ {name} ລົງທະບຽນສຳເລັດເລັດແລ້ວ ກະລຸນາໃຊ້ username: 
                                         <span style= 'font-size:1.2rem;font-weight: bold; '>{username}</span></span> ແລະລະຫັດຜ່ານຂອງທ່ານແມ່ນ 
                                         <span style= 'font-size:1.2rem;font-weight: bold; '>{password}</span></span>
                                        </div>
                                    </div>
                                    <div style='display: flex;padding: 0 1rem 1rem 1rem;'>
                                        <span>ກະລຸນາຮັກສາບັນຊີ ແລະ ລະຫັດທ່ານໃຫ້ປອດໄຟ</span>
                                    </div>
                                    <div style='display: flex;padding: 0 1rem 0.5rem 1rem;'>
                                            <h3 style= 'font-weight:bold;margin: 0;'>ລາຍລະອຽດການລົງທະບຽນ</h5>
                                    </div>
                                    <div style='display: flex;padding: 0 1rem 1rem 1rem;'>
                                            <form>
                                                <div style='display: block;'>
                                                    <span style= 'font-weight: bold; '>ຊື່ ແລະ ນາມສະກຸນ: </span>
                                                    <span style='padding-left: 0.75rem;'>{name}</span>
                                                </div>
                                                <div style='display: block;'>
                                                    <span style= 'font-weight: bold; '>Email: </span>
                                                    <span style='padding-left: 0.75rem;'>{username}</span>
                                                </div>
                                                <div style='display: block;'>
                                                    <span style= 'font-weight: bold; '>ວັນທີ່ລົງທະບຽນ: </span>
                                                    <span style='padding-left: 0.75rem;'>{DateTime.Now.ToString("dd/MM/yyyy")}</span>
                                                </div>
                                            </form>
                                        </div>
 
                                <div style='display: d-flex; border-bottom: 1px solid #192664;margin-left: 1rem;margin-right: 1rem;'></div>
                                
                                <div style='display: flex;justify-content: center;padding-block: 0.5rem;'>
                                    <span style=' color: #192664; padding-left: 1rem;padding-right: 1rem;margin-left: auto;margin-right: auto;'>ຕິດຕໍ່ພວກເຮົາໄດ້ທີ່</span>
                                </div>
                                <div style='display:flex;padding-bottom: 0.5rem;padding-top: 0.5rem;padding-left:1rem;padding-right:1rem;'>
                                    <img src= 'https://cdn-icons-png.flaticon.com/512/1384/1384005.png' style= 'width:30px;height:30px;cursor: pointer;margin-right:1rem;'/>
                                    <span style='margin-top:auto ;margin-bottom:auto;'>Facebook fanpage</span>
                                </div>
                                <div style='display:flex;padding-bottom: 0.5rem;padding-top: 0.5rem;padding-left:1rem;padding-right:1rem;'>
                                    <img src='https://cdn-icons-png.flaticon.com/512/1782/1782765.png' style= 'width:30px;height:30px;cursor: pointer;margin-right: 1rem;'/>
                                    <span style='margin-top:auto ;margin-bottom:auto;'>Codecamp</span>
                                </div>
                                <div style='display:flex;padding-bottom: 1.5rem;padding-top: 0.5rem;padding-left:1rem;padding-right:1rem;'>
                                    <img src= 'https://cdn-icons-png.flaticon.com/512/1384/1384007.png' style= 'width:30px;height:30px;cursor: pointer;margin-right: 1rem;'/>
                                    <span style='margin-top:auto ;margin-bottom:auto;'>Codecamp</span>
                                </div>

                                <div style='display: flex;justify-content:center;padding-top: 1rem;padding-bottom: 1rem;'>
                                    <span style='margin-left:auto;margin-right: auto;'>Copyright © {DateTime.Now.Year} Codecamp</span>
                                </div>

                        </div>
                    </body>
                    </html>
                    ";
        }
    }
}

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
using OpenTelemetry.Trace;

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
                string body = RegisterMailBody();
                //var msg = MailHelper.CreateSingleEmailToMultipleRecipients(from, tos, "Demo Send email from send grid", plainTextContent, body);
                //var response = await client.SendEmailAsync(msg);
                //Log.Information($"Send email: {response.Body.ReadAsStringAsync().Result}");
                MailMessage message = new MailMessage();
                message.From = new MailAddress("doc.cse@csenergy.la");
                message.Subject = "CSE Document Notify";
                message.To.Add("kommalyskb@gmail.com");
                message.IsBodyHtml = true;
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
                    smtp.DeliveryMethod = SmtpDeliveryMethod.Network;

                    await smtp.SendMailAsync(message);

                }
            }
            catch (Exception)
            {

                throw;
            }
        }
        public string RegisterMailBody()
        {
            return $@"<!DOCTYPE html>
                    <html lang= 'en '>
                    <head>
                        <meta charset= 'UTF-8 '>
                        <meta http-equiv= 'X-UA-Compatible ' content= 'IE=edge '>
                        <meta name= 'viewport ' content= 'width=device-width, initial-scale=1.0 '>
                        <title>Welcome To Codecamp Document Flow</title>
                    </head>
                    <body style= 'background-color: #F1F1F1; padding-left: 10%;padding-right: 10%;'>

                        <div>
                            <div style= 'display: flex;justify-content: center; padding-top:2rem;padding-bottom:2rem;background-color: #20629b; '>
                                <h1 style='color: #FFFFFF; text-align: center; margin-left:auto;margin-right: auto;'>Codecamp Workflow</h1>
                            </div>
                            <div  style='background-color: #FFFFFF; '>
       
                                    <div style='display: flex;padding:2rem 1rem 2rem 1rem;'>
                                        <h2 style= 'margin:0;font-weight: bold; '>ຍິນດີຕ້ອນຮັບເຂົ້າສູ່ ລະບົບຈໍລະຈອນເອກະສານ</h5>
                                    </div>
                                    <div style='display: flex;padding:0 1rem 1rem 1rem;'>
                                        <div style='padding-right: 1rem;'>ມີເອກະສານສົ່ງຫາທ່ານ ກະລຸນາເຂົ້າກວດສອບເອກະສານ
                                         <a href='https://document.cselao.la'>ກົດລິ້ງບ່ອນນີ້</a>
                                        </div>
                                    </div>
                                    <div style='display: flex;padding: 0 1rem 1rem 1rem;'>
                                        <span>ກະລຸນາຮັກສາບັນຊີ ແລະ ລະຫັດທ່ານໃຫ້ປອດໄຟ</span>
                                    </div>
                                    <div style='display: flex;padding: 0 1rem 0.5rem 1rem;'>
                                            <h3 style= 'font-weight:bold;margin: 0;'>ລາຍລະອຽດເອກະສານ</h5>
                                    </div>
                                    <div style='display: flex;padding: 0 1rem 1rem 1rem;'>
                                        <table>
                                            <tbody>
                                                <tr>
                                                    <td>
                                                        <span style= 'font-weight: bold; '>ສົ່ງຈາກ: </span>
                                                    </td>
                                                    <td>
                                                        <span style='padding-left: 0.75rem;'>ກົມມະລິ ສາຍຄຳໃບ</span>
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td>
                                                        <span style= 'font-weight: bold; '>ຫົວຂໍ້: </span>
                                                    </td>
                                                    <td>
                                                        <span style='padding-left: 0.75rem;'>ເອກະສານຂາເຂົ້າ</span>
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td>
                                                         <span style= 'font-weight: bold; '>ວັນທີສົ່ງເອກະສານ: </span>
                                                    </td>
                                                    <td>
                                                        <span style='padding-left: 0.75rem;'>{DateTime.Now.ToString("dd/MM/yyyy HH:mm")}</span>
                                                    </td>
                                                </tr>
                                            </tbody>
                                        </table>
                                            
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
                                    <span style='margin-top:auto ;margin-bottom:auto;'>doc.cse@csenergy.la</span>
                                </div>
                                <div style='display:flex;padding-bottom: 1.5rem;padding-top: 0.5rem;padding-left:1rem;padding-right:1rem;'>
                                    <img src= 'https://cdn-icons-png.flaticon.com/512/1384/1384007.png' style= 'width:30px;height:30px;cursor: pointer;margin-right: 1rem;'/>
                                    <span style='margin-top:auto ;margin-bottom:auto;'>02055555555</span>
                                </div>

                                <div style='display: flex;padding: 0 1rem 1rem 1rem;'>
                                    <span style='margin-left:auto;margin-right: auto;'>Copyright © {DateTime.Now.Year} Codecamp Sole Co.,LTD</span>
                                </div>

                        </div>
                    </body>
                    </html>
                    ";
        }
    }
}

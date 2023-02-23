using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using DFM.Shared.Configurations;
using Confluent.Kafka;
using System.Runtime.CompilerServices;
using Serilog;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Net.Mime;
using System.Net.Http;
using System.Drawing;

namespace DFM.Shared.Helper
{
    public class EmailProperty
    {
        public string? From { get; set; }
        public List<string>? To { get; set; }
        public string? Subject { get; set; }
        public string? Body { get; set; }
    }
    public enum MailType
    {
        Register,
        Forgot,
        Notification
    }
    public interface IEmailHelper
    {
        string NotificationMailBody(string link, string from, string title);
        string RegisterMailBody(string name, string username, string password);
        Task Send(EmailProperty emailProperty);
    }
    public class EmailHelper : IEmailHelper
    {
        private readonly SMTPConf smtpConf;
        private readonly SendGridConf sendGridConf;
        private readonly EnvConf envConf;

        public EmailHelper(SMTPConf smtpConf, SendGridConf sendGridConf, EnvConf envConf)
        {
            this.smtpConf = smtpConf;
            this.sendGridConf = sendGridConf;
            this.envConf = envConf;
        }
        public async Task Send(EmailProperty emailProperty)
        {
            if (envConf.Option == EmailEnum.PURESMTP)
            {
                MailMessage message = new MailMessage();
                message.From = new MailAddress(emailProperty.From!);
                message.Subject = emailProperty.Subject;
                foreach (var item in emailProperty.To!)
                {
                    message.To.Add(item);
                }
                message.IsBodyHtml = true;
                AlternateView htmlView = AlternateView.CreateAlternateViewFromString(
                  $"{emailProperty.Body}",
                  null,
                  MediaTypeNames.Text.Html
                );


                message.AlternateViews.Add(htmlView);

                try
                {
                    using (SmtpClient smtp = new SmtpClient())
                    {

                        smtp.Host = smtpConf.Server!;
                        smtp.Port = smtpConf.Port;
                        smtp.EnableSsl = true;

                        NetworkCredential netCre = new NetworkCredential(smtpConf.Email, smtpConf.Password);
                        smtp.UseDefaultCredentials = false;
                        smtp.Credentials = netCre;
                        smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                        await smtp.SendMailAsync(message);

                    }
                }
                catch (SmtpException)
                {
                    throw;
                }
            }
            else if (envConf.Option == EmailEnum.SENDGRID)
            {
                try
                {
                    var client = new SendGridClient(sendGridConf.APIKey);
                    var from = new EmailAddress(emailProperty.From!, "Document Notify");
                    List<EmailAddress> tos = new();
                    foreach (var item in emailProperty.To!)
                    {
                        tos.Add(new EmailAddress(item));
                    }
                    var plainTextContent = "";
                    var msg = MailHelper.CreateSingleEmailToMultipleRecipients(from, tos, emailProperty.Subject, plainTextContent, emailProperty.Body);
                    var response = await client.SendEmailAsync(msg);
                    Log.Information($"Send email: {response.Body.ReadAsStringAsync().Result}");
                }
                catch (Exception)
                {

                    throw;
                }
                
            }


            Log.Warning("No Email Option route was configured");
        }
        //<img src= 'https://erpstack.la/images/Logo_w.png' style='width:10rem;height:4rem;margin-left: auto;margin-right: auto;'/>

        public string RegisterMailBody(string name, string username, string password)
        {
            return $@"<!DOCTYPE html>
                    <html lang= 'en '>
                    <head>
                        <meta charset= 'UTF-8 '>
                        <meta http-equiv= 'X-UA-Compatible ' content= 'IE=edge '>
                        <meta name= 'viewport ' content= 'width=device-width, initial-scale=1.0 '>
                        <title>Welcome To {envConf.PageTitle}</title>
                    </head>
                    <body style='background-color: #F1F1F1; padding-left: 10%;padding-right: 10%;'>

                        <div>
                            <div style= 'display: flex;justify-content: center; padding-top:2rem;padding-bottom:2rem;background-color: #20629b; '>
                                <h1 style='color: #FFFFFF; text-align: center; margin-left:auto;margin-right: auto;'>{envConf.PageTitle}</h1>
                            </div>
                            <div  style= 'background-color: #FFFFFF; '>
       
                                    <div style='display: flex;padding:2rem 1rem 2rem 1rem;'>
                                        <h2 style= 'margin:0;font-weight: bold; '>ຍິນດີຕ້ອນຮັບເຂົ້າສູ່ ລະບົບຈໍລະຈອນເອກະສານ</h5>
                                    </div>
                                    <div style='display: flex;padding:0 1rem 1rem 1rem;'>
                                        <div style='padding-right: 1rem;'>ທ່ານ {name} ລົງທະບຽນສຳເລັດເລັດແລ້ວ ກະລຸນາໃຊ້ username: 
                                         <span style= 'font-size:1.2rem;font-weight: bold; '>{username}</span> ແລະ ລະຫັດຜ່ານຂອງທ່ານແມ່ນ 
                                         <span style= 'font-size:1.2rem;font-weight: bold; '>{password}</span>
                                        </div>
                                    </div>
                                    <div style='display: flex;padding: 0 1rem 1rem 1rem;'>
                                        <span>ກະລຸນາຮັກສາບັນຊີ ແລະ ລະຫັດທ່ານໃຫ້ປອດໄຟ</span>
                                    </div>
                                    <div style='display: flex;padding: 0 1rem 0.5rem 1rem;'>
                                            <h3 style= 'font-weight:bold;margin: 0;'>ລາຍລະອຽດການລົງທະບຽນ</h5>
                                    </div>
                                    <div style='display: flex;padding: 0 1rem 1rem 1rem;'>
                                        <table>
                                            <tbody>
                                                <tr>
                                                    <td>
                                                        <span style= 'font-weight: bold; '>ຊື່ ແລະ ນາມສະກຸນ: </span>
                                                    </td>
                                                    <td>
                                                        <span style='padding-left: 0.75rem;'>{name}</span>
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td>
                                                        <span style= 'font-weight: bold; '>Email: </span>
                                                    </td>
                                                    <td>
                                                        <span style='padding-left: 0.75rem;'>{username}</span>
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td>
                                                         <span style= 'font-weight: bold; '>ວັນທີ່ລົງທະບຽນ: </span>
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
                                    <span style='margin-top:auto ;margin-bottom:auto;'>{envConf.ContactEmail}</span>
                                </div>
                                <div style='display:flex;padding-bottom: 1.5rem;padding-top: 0.5rem;padding-left:1rem;padding-right:1rem;'>
                                    <img src= 'https://cdn-icons-png.flaticon.com/512/1384/1384007.png' style= 'width:30px;height:30px;cursor: pointer;margin-right: 1rem;'/>
                                    <span style='margin-top:auto ;margin-bottom:auto;'>{envConf.ContactPhone}</span>
                                </div>

                                <div style='display: flex;padding: 0 1rem 1rem 1rem;'>
                                    <span style='margin-left:auto;margin-right: auto;'>Copyright © {DateTime.Now.Year} {envConf.CopyRightCompany}</span>
                                </div>

                        </div>
                    </body>
                    </html>
                    ";
        }
        public string NotificationMailBody(string link, string from, string title)
        {
            return $@"<!DOCTYPE html>
                    <html lang= 'en '>
                    <head>
                        <meta charset= 'UTF-8 '>
                        <meta http-equiv= 'X-UA-Compatible ' content= 'IE=edge '>
                        <meta name= 'viewport ' content= 'width=device-width, initial-scale=1.0 '>
                        <title>Welcome To {envConf.PageTitle}</title>
                        
                    </head>
                    <body style= 'background-color: #F1F1F1; padding-left: 28%;padding-right: 28%;'>

                        <div>
                            <div style= 'display: flex;justify-content: center; padding-top:2rem;padding-bottom:2rem;background-color: #20629b; '>
                                <h1 style='color: #FFFFFF; text-align: center; margin-left:auto;margin-right: auto;'>{envConf.PageTitle}</h1>
                            </div>
                            <div  style= 'background-color: #FFFFFF; '>
       
                                    <div style='display: flex;padding:2rem 1rem 2rem 1rem;'>
                                        <h2 style= 'margin:0;font-weight: bold; '>ຍິນດີຕ້ອນຮັບເຂົ້າສູ່ ລະບົບຈໍລະຈອນເອກະສານ</h5>
                                    </div>
                                    <div style='display: flex;padding:0 1rem 1rem 1rem;'>
                                        <div style='padding-right: 1rem;'>ມີເອກະສານສົ່ງຫາທ່ານ ກະລຸນາເຂົ້າກວດສອບເອກະສານ
                                         <a href='{link}'>ກົດລິ້ງບ່ອນນີ້</a>
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
                                                        <span style='padding-left: 0.75rem;'>{from}</span>
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td>
                                                        <span style= 'font-weight: bold; '>ຫົວຂໍ້: </span>
                                                    </td>
                                                    <td>
                                                        <span style='padding-left: 0.75rem;'>{title}</span>
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
                                    <span style='margin-top:auto ;margin-bottom:auto;'>{envConf.ContactEmail}</span>
                                </div>
                                <div style='display:flex;padding-bottom: 1.5rem;padding-top: 0.5rem;padding-left:1rem;padding-right:1rem;'>
                                    <img src= 'https://cdn-icons-png.flaticon.com/512/1384/1384007.png' style= 'width:30px;height:30px;cursor: pointer;margin-right: 1rem;'/>
                                    <span style='margin-top:auto ;margin-bottom:auto;'>{envConf.ContactPhone}</span>
                                </div>

                                <div style='display: flex;padding: 0 1rem 1rem 1rem;'>
                                    <span style='margin-left:auto;margin-right: auto;'>Copyright © {DateTime.Now.Year} {envConf.CopyRightCompany}</span>
                                </div>

                        </div>
                    </body>
                    </html>
                    ";
        }
    }
}

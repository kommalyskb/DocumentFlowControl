using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using DFM.Shared.Configurations;

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
        private readonly SMTPConfig smtpConf;

        public EmailHelper(SMTPConfig smtpConf)
        {
            this.smtpConf = smtpConf;
        }
        public async Task Send(EmailProperty emailProperty)
        {
            SmtpClient smtp = new SmtpClient(smtpConf.Server, smtpConf.Port);
            smtp.EnableSsl = true;
            smtp.Credentials = new NetworkCredential(smtpConf.Username, smtpConf.Password);

           
            MailMessage message = new MailMessage();
            message.From = new MailAddress(emailProperty.From!);
            message.Subject = emailProperty.Subject;
            foreach (var item in emailProperty.To!)
            {
                message.To.Add(item);
            }
            
            AlternateView htmlView = AlternateView.CreateAlternateViewFromString(
              $"{emailProperty.Body}",
              null,
              "text/html"
            );


            message.AlternateViews.Add(htmlView);

            try
            {
                await smtp.SendMailAsync(message);
            }
            catch (SmtpException)
            {
                throw;
            }
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
                        <title>Welcome To Document Control Flow System</title>
                    </head>
                    <body style= 'background-color: #F1F1F1; padding-left: 28%;padding-right: 28%;'>

                        <div >
                            <div style= 'display: flex;justify-content: center; padding-top:2rem;padding-bottom:2rem;background-color: #192664; '>
                                <h1>Document System</h1>
                            </div>
                            <div  style= 'background-color: #FFFFFF; '>
       
                                    <div style='display: flex;padding:2rem 1rem 2rem 1rem;'>
                                        <h2 style= 'margin:0;font-weight: bold; '>ຍິນດີຕ້ອນຮັບເຂົ້າສູ່ ລະບົບຈໍລະຈອນເອກະສານ</h5>
                                    </div>
                                    <div style='display: flex;padding:0 1rem 1rem 1rem;'>
                                        <div style='padding-right: 1rem;'>ທ່ານ {username} ລົງທະບຽນສຳເລັດເລັດແລ້ວລະຫັດຜ່ານຂອງທ່ານແມ່ນ 
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
                                    <span style='margin-top:auto ;margin-bottom:auto;'>document.admin@dmf.la</span>
                                </div>
                                <div style='display:flex;padding-bottom: 1.5rem;padding-top: 0.5rem;padding-left:1rem;padding-right:1rem;'>
                                    <img src= 'https://cdn-icons-png.flaticon.com/512/1384/1384007.png' style= 'width:30px;height:30px;cursor: pointer;margin-right: 1rem;'/>
                                    <span style='margin-top:auto ;margin-bottom:auto;'>021xxxxxx</span>
                                </div>

                                <div style='display: flex;justify-content:center;padding-top: 1rem;padding-bottom: 1rem;'>
                                    <span style='margin-left:auto;margin-right: auto;'>Copyright © 2022 Comapny name</span>
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
                        <title>Welcome To Document Control Flow System</title>
                    </head>
                    <body style= 'background-color: #F1F1F1; padding-left: 28%;padding-right: 28%;'>

                        <div >
                            <div style= 'display: flex;justify-content: center; padding-top:2rem;padding-bottom:2rem;background-color: #192664; '>
                                <h1>Document System</h1>
                            </div>
                            <div  style= 'background-color: #FFFFFF; '>
       
                                    <div style='display: flex;padding:2rem 1rem 2rem 1rem;'>
                                        <h2 style= 'margin:0;font-weight: bold; '>ຍິນດີຕ້ອນຮັບເຂົ້າສູ່ ລະບົບຈໍລະຈອນເອກະສານ</h5>
                                    </div>
                                    <div style='display: flex;padding:0 1rem 1rem 1rem;'>
                                        <div style='padding-right: 1rem;'>ມີເອກະສານສົ່ງຫາທ່ານ ກະລຸນາເຂົ້າກວດສອບເອກະສານ
                                         <span style= 'font-size:1.2rem;font-weight: bold;'>{link}</span></span>
                                        </div>
                                    </div>
                                    <div style='display: flex;padding: 0 1rem 1rem 1rem;'>
                                        <span>ກະລຸນາຮັກສາບັນຊີ ແລະ ລະຫັດທ່ານໃຫ້ປອດໄຟ</span>
                                    </div>
                                    <div style='display: flex;padding: 0 1rem 0.5rem 1rem;'>
                                            <h3 style= 'font-weight:bold;margin: 0;'>ລາຍລະອຽດເອກະສານ</h5>
                                    </div>
                                    <div style='display: flex;padding: 0 1rem 1rem 1rem;'>
                                            <form>
                                                <div style='display: block;'>
                                                    <span style= 'font-weight: bold; '>ສົ່ງຈາກ: </span>
                                                    <span style='padding-left: 0.75rem;'>{from}</span>
                                                </div>
                                                <div style='display: block;'>
                                                    <span style= 'font-weight: bold; '>ຫົວຂໍ້: </span>
                                                    <span style='padding-left: 0.75rem;'>{title}</span>
                                                </div>
                                                <div style='display: block;'>
                                                    <span style= 'font-weight: bold; '>ວັນທີສົ່ງເອກະສານ: </span>
                                                    <span style='padding-left: 0.75rem;'>{DateTime.Now.ToString("dd/MM/yyyy HH:mm")}</span>
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
                                    <span style='margin-top:auto ;margin-bottom:auto;'>document.admin@dmf.la</span>
                                </div>
                                <div style='display:flex;padding-bottom: 1.5rem;padding-top: 0.5rem;padding-left:1rem;padding-right:1rem;'>
                                    <img src= 'https://cdn-icons-png.flaticon.com/512/1384/1384007.png' style= 'width:30px;height:30px;cursor: pointer;margin-right: 1rem;'/>
                                    <span style='margin-top:auto ;margin-bottom:auto;'>021xxxxxx</span>
                                </div>

                                <div style='display: flex;justify-content:center;padding-top: 1rem;padding-bottom: 1rem;'>
                                    <span style='margin-left:auto;margin-right: auto;'>Copyright © 2022 Comapny name</span>
                                </div>

                        </div>
                    </body>
                    </html>
                    ";
        }
    }
}

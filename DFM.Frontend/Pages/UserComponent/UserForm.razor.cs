using DFM.Frontend.Shared;
using DFM.Shared.DTOs;
using DFM.Shared.Entities;
using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;
using MyCouch.Contexts;

namespace DFM.Frontend.Pages.UserComponent
{
    public partial class UserForm
    {
        //private EmployeeModel? employee;
        //string? token = "";
        string? imageUri = "images/user.png";
        private IEnumerable<string> MaxCharacters(string ch)
        {
            if (!string.IsNullOrEmpty(ch) && 1000 < ch?.Length)
                yield return "ອັກສອນສູງສຸດ 1000 ອັກສອນ";
        }
        private IEnumerable<string> MediumCharacters(string ch)
        {
            if (!string.IsNullOrEmpty(ch) && 500 < ch?.Length)
                yield return "ອັກສອນສູງສຸດ 500 ອັກສອນ";
        }

        private IEnumerable<string> UserCharacters(string ch)
        {
            if (!string.IsNullOrEmpty(ch) && 36 < ch?.Length)
                yield return "ອັກສອນສູງສຸດ 36 ອັກສອນ";
        }
        private IEnumerable<string> PasswordCharacters(string ch)
        {
            if (!string.IsNullOrEmpty(ch) && 36 < ch?.Length)
                yield return "ອັກສອນສູງສຸດ 36 ອັກສອນ";
        }

        private async Task uploadFiles(InputFileChangeEventArgs e)
        {
            if (e.FileCount > 1)
            {
                await Notice.InvokeAsync("ບໍ່ສາມາດເລືອກ ເອກະສານໄດ້ພ້ອມກັນ ເກີນ 1  File");
                return;
            }
            string prefix = $"{DateTime.Now.ToString("yyyyMM")}-profile";
            foreach (var file in e.GetMultipleFiles())
            {
                var ext = Path.GetExtension(file.Name);
                var name = file.Name.Replace(ext, "");
                ProfileImage!.File = file;
                decimal fileSize = ProfileImage.File.Size / 1048576M;
                string? fileFormat = Icons.Custom.FileFormats.FileDocument;
                if (file.ContentType.Contains("word"))
                {
                    fileFormat = Icons.Custom.FileFormats.FileWord;
                }
                if (file.ContentType.Contains("pdf"))
                {
                    fileFormat = Icons.Custom.FileFormats.FilePdf;
                }
                if (file.ContentType.StartsWith("image"))
                {
                    fileFormat = Icons.Custom.FileFormats.FileImage;
                }
                if (file.ContentType.Contains("excel"))
                {
                    fileFormat = Icons.Custom.FileFormats.FileWord;
                }
                if (file.ContentType.Contains("audio"))
                {
                    fileFormat = Icons.Custom.FileFormats.FileMusic;
                }
                ProfileImage.Info = new()
                {
                    Bucket = prefix,
                    Display = file.Name,
                    FileName = $"{Path.GetRandomFileName()}({name}){ext}",
                    Version = 1,
                    FileSize = fileSize,
                    IsNewFile = true,
                    FileFormat = fileFormat,
                    MimeType = file.ContentType
                };

                // Render
                var image = await file.RequestImageFileAsync(file.ContentType, 200, 200);

                using Stream imageStream = image.OpenReadStream(1024 * 1024 * 10);

                using MemoryStream ms = new();
                //copy imageStream to Memory stream
                await imageStream.CopyToAsync(ms);

                //convert stream to base64
                imageUri = $"data:image/png;base64,{Convert.ToBase64String(ms.ToArray())}";
            }
            //TODO upload the files to the server

            await InvokeAsync(StateHasChanged);
        }

        private async Task previewImageProfile(string bucket, string fileName)
        {
            // Generate link
            var link = await minio.GenerateLink(bucket, fileName);
            if (link.IsSuccess)
            {
                imageUri = link.Response;
            }
            
        }

        protected override async Task OnInitializedAsync()
        {
            if (Employee != null)
            {
                if (Employee.ProfileImage != null)
                {
                    await previewImageProfile(Employee.ProfileImage.Bucket!, Employee.ProfileImage.FileName!);

                }
            }
            
        }
    }
}

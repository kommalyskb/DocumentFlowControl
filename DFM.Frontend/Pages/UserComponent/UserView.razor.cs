using DFM.Shared.Common;
using DFM.Shared.Entities;
using DFM.Shared.Extensions;
using Google.Protobuf.WellKnownTypes;
using HttpClientService;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;
using Newtonsoft.Json.Linq;

namespace DFM.Frontend.Pages.UserComponent
{
    public partial class UserView
    {
        EmployeeModel? Employee = new();
        string? imageUri = "";
        string? token;
        protected override async Task OnInitializedAsync()
        {
            Employee = await storageHelper.GetEmployeeProfileAsync();
            if (Employee.ProfileImage != null)
            {
                await previewImageProfile(Employee.ProfileImage.Bucket!, Employee.ProfileImage.FileName!);

            }
        }

        void AlertMessage(string message, string position, Severity severity)
        {
            Snackbar.Clear();
            Snackbar.Configuration.PositionClass = position;
            Snackbar.Add(message, severity);
        }

        private async Task uploadFiles(InputFileChangeEventArgs e)
        {
            if (e.FileCount > 1)
            {
                AlertMessage("ບໍ່ສາມາດເລືອກ ເອກະສານໄດ້ພ້ອມກັນ ເກີນ 1  File", Defaults.Classes.Position.BottomRight, Severity.Error);
                return;
            }
            onProcessing = true;

            string prefix = $"{DateTime.Now.ToString("yyyyMM")}-profile";
            foreach (var file in e.GetMultipleFiles())
            {
                var ext = Path.GetExtension(file.Name);
                var name = file.Name.Replace(ext, "");
                decimal fileSize = file.Size / 1048576M;
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
                var imageInfo = new AttachmentModel
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
                //convert to array for upload to server
                var buffer = ms.ToArray();

                //convert stream to base64

                imageUri = $"data:image/png;base64,{Convert.ToBase64String(ms.ToArray())}";

                await minio.PutObject(imageInfo.Bucket!, imageInfo.FileName!, buffer);
                
                // Update Profile
                string url = $"{endpoint.API}/api/v1/Employee/UpdateImageProfile/{Employee!.id}";
                if (string.IsNullOrWhiteSpace(token))
                {
                    token = await accessToken.GetTokenAsync();
                }

                var result = await httpService.Post<AttachmentModel, CommonResponse>(url, imageInfo, new AuthorizeHeader("bearer", token), cancellationToken: cts.Token);
                if (result.Success)
                {
                    AlertMessage("ອັບໂຫຼດຮູບຂອງທ່ານສຳເລັດ", Defaults.Classes.Position.BottomRight, Severity.Success);

                }
                else
                {
                    AlertMessage("ບໍ່ສາມາດອັບໂຫຼດຮູບໄດ້", Defaults.Classes.Position.BottomRight, Severity.Error);
                }
            }
            onProcessing = false;
            //TODO upload the files to the server
            await InvokeAsync(StateHasChanged);



        }
        private async Task previewImageProfile(string bucket, string fileName)
        {
            // get file obj
            var obj = await minio.GenerateLink(bucket, fileName);
            if (obj.IsSuccess)
            {
                //imageUri = link.Response.Replace(endpoint.MinioAPI, endpoint.StorageAPI);
                imageUri = obj.Response;//$"data:image/png;base64,{obj.ByteStream.ToBase64()}";
            }

        }
    }
}

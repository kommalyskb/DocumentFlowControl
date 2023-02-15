using DFM.Shared.Entities;
using DFM.Shared.Extensions;
using Google.Protobuf.WellKnownTypes;

namespace DFM.Frontend.Pages.UserComponent
{
    public partial class UserView
    {
        EmployeeModel? Employee = new();
        string? imageUri = "";
        protected override async Task OnInitializedAsync()
        {
            Employee = await storageHelper.GetEmployeeProfileAsync();
            if (Employee.ProfileImage != null)
            {
                await previewImageProfile(Employee.ProfileImage.Bucket, Employee.ProfileImage.FileName);

            }
        }

        private async Task previewImageProfile(string bucket, string fileName)
        {
            // get file obj
            var obj = await minio.GetObject(bucket, fileName);
            if (obj.IsSuccess)
            {
                //imageUri = link.Response.Replace(endpoint.MinioAPI, endpoint.StorageAPI);
                imageUri = $"data:image/png;base64,{obj.ByteStream.ToBase64()}";
            }

        }
    }
}

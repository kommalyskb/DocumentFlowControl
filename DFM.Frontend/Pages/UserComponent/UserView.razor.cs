using DFM.Shared.Entities;
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
            // Generate link
            var link = await minio.GenerateLink(bucket, fileName);
            if (link.IsSuccess)
            {
                imageUri = link.Response;
            }

        }
    }
}

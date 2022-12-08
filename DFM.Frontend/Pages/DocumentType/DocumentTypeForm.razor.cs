using DFM.Shared.Entities;
using Newtonsoft.Json.Linq;
using System.Threading.Channels;

namespace DFM.Frontend.Pages.DocumentType
{
    public partial class DocumentTypeForm
    {
        private EmployeeModel? employee;
        string? token = "";

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

    }
}

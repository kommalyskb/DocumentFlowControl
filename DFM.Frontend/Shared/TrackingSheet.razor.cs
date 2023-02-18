using DFM.Shared.Common;
using DFM.Shared.Entities;

namespace DFM.Frontend.Shared
{
    public partial class TrackingSheet
    {
        private string searchString = "";
        
        private bool FilterFunc(Reciepient element)
        {
            if (string.IsNullOrWhiteSpace(searchString))
                return true;
            if (element.RecipientInfo.Fullname.Name.Eng!.Contains(searchString, StringComparison.OrdinalIgnoreCase))
                return true;
            if (element.RecipientInfo.Fullname.Name.Local!.Contains(searchString, StringComparison.OrdinalIgnoreCase))
                return true;
            if (element.RecipientInfo.Position.Eng!.Contains(searchString, StringComparison.OrdinalIgnoreCase))
                return true;
            if (element.RecipientInfo.Position.Local!.Contains(searchString, StringComparison.OrdinalIgnoreCase))
                return true;
            return false;
        }
        
        private string isRead(bool status, string? readDate)
        {
            if (status)
            {
                return readDate!;
            }
            return "ຍັງບໍ່ທັນເປີດອ່ານ";
        }
    }
}

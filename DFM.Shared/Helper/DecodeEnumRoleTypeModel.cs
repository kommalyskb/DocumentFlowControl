using DFM.Shared.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFM.Shared.Helper
{
    public static class DecodeEnumRoleTypeModel
    {
        public static string decodeRoleType(RoleTypeModel role)
        {
            switch (role)
            {
                case RoleTypeModel.Prime:
                    return "ປະທານບໍລິສັດ";
                case RoleTypeModel.DeputyPrime:
                    return "ຮອງປະທານບໍລິສັດ";
                case RoleTypeModel.PrimeSecretary:
                    return "ເລຂາປະທານບໍລິສັດ";
                case RoleTypeModel.DeputyPrimeSecretary:
                    return "ເລຂາຮອງປະທານບໍລິສັດ";
                case RoleTypeModel.Director:
                    return "ຜູ້ອຳນວຍການ";
                case RoleTypeModel.DeputyDirector:
                    return "ຮອງຜູ້ອຳນວຍການ";
                case RoleTypeModel.OfficePrime:
                    return "ຫົວຫນ້າຫ້ອງການ";
                case RoleTypeModel.DeputyOfficePrime:
                    return "ຮອງຫົວຫນ້າຫ້ອງການ";
                case RoleTypeModel.General:
                    return "ຫົວຫນ້າຝ່າຍ";
                case RoleTypeModel.DeputyGeneral:
                    return "ຮອງຫົວຫນ້າຝ່າຍ";
                case RoleTypeModel.OfficeGeneral:
                    return "ຫົວຫນ້າຫ້ອງການຝ່າຍ";
                case RoleTypeModel.DeputyOfficeGeneral:
                    return "ຮອງຫົວຫນ້າຫ້ອງການຝ່າຍ";
                case RoleTypeModel.Division:
                    return "ຫົວຫນ້າພະແນກ";
                case RoleTypeModel.DeputyDivision:
                    return "ຮອງພະແນກ";
                case RoleTypeModel.Department:
                    return "ຫົວຫນ້າພາກສ່ວນ";
                case RoleTypeModel.DeputyDepartment:
                    return "ຮອງພາກສ່ວນ";
                case RoleTypeModel.Employee:
                    return "ພະນັກງານວິຊາການ";
                case RoleTypeModel.Contract:
                    return "ສັນຍາຈ້າງ";
                case RoleTypeModel.Volunteer:
                    return "ອາສາສະຫມັກ";
                case RoleTypeModel.InboundPrime:
                    return "ຂາເຂົ້າບໍລິສັດ";
                case RoleTypeModel.InboundOfficePrime:
                    return "ຂາເຂົ້າຫ້ອງການ";
                case RoleTypeModel.InboundGeneral:
                    return "ຂາເຂົ້າຝ່າຍ";
                case RoleTypeModel.OutboundPrime:
                    return "ຂາອອກບໍລິສັດ";
                case RoleTypeModel.OutboundOfficePrime:
                    return "ຂາອອກຫ້ອງການ";
                case RoleTypeModel.OutboundGeneral:
                    return "ຂາອອກຝ່າຍ";
                default:
                    return "";
            }
        }
    }
}

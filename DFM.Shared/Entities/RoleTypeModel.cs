using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFM.Shared.Entities
{
    public enum RoleTypeModel
    {
        Prime, // ປະທານ
        DeputyPrime, // ຮອງປະທານ

        PrimeSecretary, // ເລຂາປະທານ
        DeputyPrimeSecretary, // ເລຂາຮອງປະທານ

        Director, // ອຳນວຍການ
        DeputyDirector, // ຮອງອຳນວຍການ

        OfficePrime, // ຫົວຫນ້າຫ້ອງການ ບໍລິສັດ
        DeputyOfficePrime, // ຮອງຫ້ອງການ ບໍລິສັດ

        General, // ຫົວຫນ້າຝ່າຍ
        DeputyGeneral, // ຮອງຫົວຫນ້າຝ່າຍ

        OfficeGeneral, // ຫົວຫນ້າຫ້ອງການຝ່າຍ
        DeputyOfficeGeneral, // ຮອງຫ້ອງການຝ່າຍ

        Division, // ຫົວຫນ້າພະແນກ
        DeputyDivision, // ຮອງພະແນກ

        Department, // ຫົວຫນ້າຂະແໜງ
        DeputyDepartment, // ຮອງຂະແໜງ

        Employee, // ວິຊາການ
        Contract, // ສັນຍາຈ້າງ
        Volunteer, // ອາສາສະໝັກ

        InboundPrime, // ຂາເຂົ້າ ບໍລິສັດ
        InboundOfficePrime, // ຂາເຂົ້າ ຫ້ອງການ
        InboundGeneral, // ຂາເຂົ້າ ຝ່າຍ

        OutboundPrime, // ຂາອອກບໍລິສັດ
        OutboundOfficePrime, // ຂາອອກຫ້ອງການ
        OutboundGeneral // ຂາອອກຝ່າຍ

    }
}

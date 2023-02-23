using DFM.Shared.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFM.Shared.Helper
{
    public static class DecodeEnumStatus
    {
        public static string decodeOperationType(OperationType operationType)
        {
            switch (operationType)
            {
                case OperationType.Main:
                    return "ຜູ້ແກ້ໄຂຫຼັກ";
                case OperationType.CoProccess:
                    return "ຜູ້ຮ່ວມແກ້ໄຂ";
                case OperationType.Follower:
                    return "ຜູ້ຕິດຕາມ";
                case OperationType.NoProcess:
                    return "ບໍ່ມີການດຳເນີນການ";
                default:
                    return "ບໍ່ມີການດຳເນີນການ";
            }
        }
        public static string decodeTraceStatus(TraceStatus traceStatus, bool isRead = true)
        {
            switch (traceStatus)
            {
                case TraceStatus.Draft:
                    return "ສະບັບຮ່າງ";
                case TraceStatus.InProgress:
                    if (isRead)
                    {
                        return "ກຳລັງດຳເນີນການ";
                    }
                    else
                    {
                        return "ລໍຖ້າກຳເນີນງານ";
                    }
                   
                case TraceStatus.Completed:
                    return "ສຳເລັດແລ້ວ";
                case TraceStatus.Revoked:
                    return "ຖອນຄືນ";
                case TraceStatus.Terminated:
                    return "ຈົບການຈໍລະຈອນ";
                case TraceStatus.Following:
                    return "ຕິດຕາມ";
                case TraceStatus.CoProccess:
                    if (isRead)
                    {
                        return "ກຳລັງດຳເນີນການ";
                    }
                    else
                    {
                        return "ລໍຖ້າກຳເນີນງານ";
                    }
                case TraceStatus.Rejected:
                    return "ຍົກເລີກ";
                case TraceStatus.Trash:
                    return "ຖັງຂີ້ເຫຍື່ອ";
                case TraceStatus.Remove:
                    return "ລຶບຖາວອນ";
                default:
                    return "";
            }
        }
    }
}

using DFM.Shared.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFM.Shared.Helper
{
    public static class DecodeEnumModuleType
    {
        public static string decodeModule(ModuleType moduleType)
        {
            switch (moduleType)
            {
                case ModuleType.DocumentInbound:
                    return "ການສົ່ງເອກະສານ ຂາເຂົ້າ";
                case ModuleType.DocumentOutbound:
                    return "ການສົ່ງເອກະສານ ຂາອອກ";
                case ModuleType.TaskManagement:
                    return "ການໄຫຼຂອງລະບົບ Task Management";
                case ModuleType.Leave:
                    return "ການໄຫຼຂອງລະບົບລາພັກ";
                default:
                    return "";
            }
        }
    }
}

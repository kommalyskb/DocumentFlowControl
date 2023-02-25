using DFM.Shared.Common;
using DFM.Shared.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFM.Shared.Helper
{
    public static class DecodeEnumMenu
    {
        public static string decodeMenuLink(MenuLink menu)
        {
            switch (menu)
            {
                case MenuLink.MAIN_PAGE:
                    return "ຫນ້າຫລັກ";
                case MenuLink.USER_INFO:
                    return "ຂໍ້ມູນຜູ້ໃຊ້";
                case MenuLink.IB_INBOX:
                    return "ອິນບ໋ອກ";
                case MenuLink.IB_COPROCESS:
                    return "ຕິດຕາມ";
                case MenuLink.IB_DRAFT:
                    return "ສະບັບຮ່າງ";
                case MenuLink.IB_COMOMPLETED:
                    return "ສຳເລັດແລ້ວ";
                case MenuLink.IB_TRASH:
                    return "ຖັງຂີ້ເຫຍື່ອ";
                case MenuLink.OB_INBOX:
                    return "ເອກະສານສົ່ງອອກ";
                case MenuLink.OB_COPROCESS:
                    return "ຕິດຕາມ";
                case MenuLink.OB_DRAFT:
                    return "ສະບັບຮ່າງ";
                case MenuLink.OB_COMOMPLETED:
                    return "ສຳເລັດແລ້ວ";
                case MenuLink.OB_TRASH:
                    return "ຖັງຂີ້ເຫຍື່ອ";
                case MenuLink.MON_IB:
                    return "ລາຍງານແອັດມິນເອກະສານຂາເຂົ້າ";
                case MenuLink.MON_OB:
                    return "ລາຍງານແອັດມິນເອກະສານຂາອອກ";
                case MenuLink.IB_FOLDER:
                    return "ແຟ້ມຂາເຂົ້າ";
                case MenuLink.OB_FOLDER:
                    return "ແຟ້ມຂາອອກ";
                case MenuLink.ORG_CHART:
                    return "ແຜນວາດອົງກອນ";
                case MenuLink.USER_MGR:
                    return "ຜູ້ໃຊ້ລະບົບ";
                case MenuLink.DOC_TYPE:
                    return "ປະເພດເອກະສານ";
                case MenuLink.SEC_LEVEL:
                    return "ລະດັບຄວາມປອດໄພເອກະສານ";
                case MenuLink.URGENT_LEVEL:
                    return "ລະດັບຄວາມເລັ່ງດ່ວນ";
                case MenuLink.RULE_MENU:
                    return "ກຳໜົດສິດຂອງຜູ້ໃຊ້";
                case MenuLink.FREE_FLOW:
                    return "ກຳໜົດການສົ່ງເອກະສານ";
                case MenuLink.REPORT_IB:
                    return "ລາຍງານເອກະສານຂາເຂົ້າ";
                case MenuLink.REPORT_OB:
                    return "ລາຍງານເອກະສານຂາອອກ";
                default:
                    return "";
            }
        }
    }
}

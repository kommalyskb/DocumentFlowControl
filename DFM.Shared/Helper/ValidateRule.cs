using DFM.Shared.Entities;
using DFM.Shared.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFM.Shared.Helper
{
    public static class ValidateRule
    {
        public static bool isInRole(IEnumerable<RuleMenu> rules, string link)
        {
            if (rules.IsNullOrEmpty())
            {
                return true;
            }
            var menu = convertFromLinkToMenu(link);

            var isContain = rules.Any(x => x.Menu == menu);

            return isContain;
        }

        private static MenuLink convertFromLinkToMenu(string link)
        {
            switch (link)
            {
                case "/pages/home":
                    return MenuLink.MAIN_PAGE;
                case "/pages/personal/account":
                    return MenuLink.USER_INFO;
                case "/pages/doc/inbound/inbox/none":
                    return MenuLink.IB_INBOX;
                case "/pages/doc/inbound/coprocess/none":
                    return MenuLink.IB_COPROCESS;
                case "/pages/doc/inbound/draft/none":
                    return MenuLink.IB_DRAFT;
                case "/pages/doc/inbound/completed/none":
                    return MenuLink.IB_COMOMPLETED;
                case "/pages/doc/inbound/bin/none":
                    return MenuLink.IB_TRASH;
                case "/pages/doc/outbound/inbox/none":
                    return MenuLink.OB_INBOX;
                case "/pages/doc/outbound/coprocess/none":
                    return MenuLink.OB_COPROCESS;
                case "/pages/doc/outbound/draft/none":
                    return MenuLink.OB_DRAFT;
                case "/pages/doc/outbound/completed/none":
                    return MenuLink.OB_COMOMPLETED;
                case "/pages/doc/outbound/bin/none":
                    return MenuLink.OB_TRASH;
                case "/pages/monitor/inbound":
                    return MenuLink.MON_IB;
                case "/pages/monitor/outbound":
                    return MenuLink.MON_OB;
                case "/pages/folder/inbound":
                    return MenuLink.IB_FOLDER;
                case "/pages/folder/outbound":
                    return MenuLink.OB_FOLDER;
                case "/pages/org/chart":
                    return MenuLink.ORG_CHART;
                case "/pages/org/user":
                    return MenuLink.USER_MGR;
                case "/pages/doctype":
                    return MenuLink.DOC_TYPE;
                case "/pages/security":
                    return MenuLink.SEC_LEVEL;
                case "/pages/urgent":
                    return MenuLink.URGENT_LEVEL;
                case "/pages/rulemanager":
                    return MenuLink.RULE_MENU;
                case "/pages/freeflow":
                    return MenuLink.FREE_FLOW;
                case "/pages/report/inbound":
                    return MenuLink.REPORT_IB;
                case "/pages/report/outbound":
                    return MenuLink.REPORT_OB;
                default:
                    return MenuLink.MAIN_PAGE;
            }
        }
    }
}

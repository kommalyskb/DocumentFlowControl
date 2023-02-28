using CouchDBService;
using DFM.Shared.Common;
using DFM.Shared.Configurations;
using DFM.Shared.Entities;
using DFM.Shared.Extensions;
using DFM.Shared.Interfaces;
using DFM.Shared.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace DFM.Testcases.DocumentOperation
{
    public class P01
    {
        IDocumentTransaction documentService;
        public P01()
        {
            ICouchContext couchContext = new CouchContext();
            DBConfig dBConfig = new DBConfig()
            {
                Writer = new DBInfo
                {
                    Scheme = "http",
                    Password = "1qaz2wsx",
                    Port = 5984,
                    SrvAddr = "localhost",
                    Username = "admin"
                },
                Reader = new DBInfo
                {
                    Scheme = "http",
                    Password = "1qaz2wsx",
                    Port = 5984,
                    SrvAddr = "localhost",
                    Username = "admin"
                }

            };
            IRedisConnector redisConnector = new RedisConnector(new RedisConf()
            {
                Server = "localhost",
                Port = 6379
            });
            IOrganizationChart organizationChart = new OrganizationChart(couchContext, dBConfig, redisConnector);
            IRoleManager roleManager = new RoleManager(couchContext, dBConfig, redisConnector, organizationChart);
            documentService = new DocumentTransaction(couchContext, dBConfig, redisConnector, roleManager);
        }
        [Fact(DisplayName = "ສ້າງເອກະສານໃຫມ່ ໄວ້ໃນສະຖານະ ສະບັບຮ່າງ ອີງຕາມການດຳເນີນເອກະສານ ຂາເຂົ້າ-ຂາອອກ")]
        public async Task NC1()
        {
            var result = await documentService.NewDocument(new DocumentModel
            {
                id = Guid.NewGuid().ToString("N"),
                OrganizationID = "776dca2568194b77a8fc9c7c7f377e08",
                RawDatas = new List<RawDocumentData>
                {
                    new RawDocumentData
                    {
                        ExternalDocID = "114/PPO",
                        Title = "PR Doc",
                        DocDate = DateTime.Now.ToString("yyyy-MM-dd")
                    }
                },
                
                
                Recipients = new List<Reciepient>
                {
                    new Reciepient
                    {
                        UId = Guid.NewGuid().ToString("N"),
                        IsRead = false,
                        ReadDate = DateTime.Now.ToString("yyyy-MM-dd"),
                        ReceiveDate = DateTime.Now.ToString("yyyy-MM-dd"),
                        DocStatus = TraceStatus.Draft,
                        ParentID = "0",
                        SendDate = DateTime.Now.ToString("yyyy-MM-dd"),
                        ReceiveRoleType = OperationType.Main,
                        SendRoleType = OperationType.Main,
                        RecipientInfo = new RoleTraceModel
                        {
                            Position = new MultiLanguage
                            {
                                Eng = "CTO",
                                Local = "ຮອງຜູ້ອຳນວຍການ ຝ່າຍ ເຕັກນິກ"
                            },
                            RoleID = "bcd52dd6296d4a44ba2b93733a699b48",
                            RoleType = RoleTypeModel.DeputyPrime,
                            Fullname = new ShortEmpInfo
                            {
                                EmployeeID = "776dca2568194b77a8fc9c7c7f377e08",
                                Name = new MultiLanguage
                                {
                                    Eng = "Phonethip Saykhambay",
                                    Local = "ພອນທິບ ສາຍຄຳໃບ"
                                }
                            }
                        },
                        InboxType = InboxType.Outbound
                    }
                }
            });

            Assert.True(result.Success);
        }

        [Fact(DisplayName = "ແກ້ໄຂເອກະສານ ອີງຕາມການດຳເນີນເອກະສານ ຂາເຂົ້າ-ຂາອອກ")]
        public async Task NC2()
        {
            var result = await documentService.EditDocument(new DocumentModel
            {
                id = "d5bb036149774650a0d41d9978bd2566",
                OrganizationID = "776dca2568194b77a8fc9c7c7f377e08",
                RawDatas = new List<RawDocumentData>
                {
                    new RawDocumentData
                    {
                        ExternalDocID = "114/PPO",
                        Title = "PR Doc",
                        DocDate = DateTime.Now.ToString("yyyy-MM-dd")
                    }
                },
                Recipients = new List<Reciepient>
                {
                    new Reciepient
                    {
                        UId = Guid.NewGuid().ToString("N"),
                        IsRead = true,
                        ReadDate = DateTime.Now.ToString("yyyy-MM-dd"),
                        ReceiveDate = DateTime.Now.ToString("yyyy-MM-dd"),
                        DocStatus = TraceStatus.Draft,
                        ParentID = "0",
                        SendDate = DateTime.Now.ToString("yyyy-MM-dd"),
                        ReceiveRoleType = OperationType.Main,
                        SendRoleType = OperationType.Main,
                        RecipientInfo = new RoleTraceModel
                        {
                            Position = new MultiLanguage
                            {
                                Eng = "CTO",
                                Local = "ຮອງຜູ້ອຳນວຍການ ຝ່າຍ ເຕັກນິກ"
                            },
                            RoleID = "bcd52dd6296d4a44ba2b93733a699b48",
                            RoleType = RoleTypeModel.DeputyPrime,
                            Fullname = new ShortEmpInfo
                            {
                                EmployeeID = "776dca2568194b77a8fc9c7c7f377e08",
                                Name = new MultiLanguage
                                {
                                    Eng = "Phonethip Saykhambay",
                                    Local = "ພອນທິບ ສາຍຄຳໃບ"
                                }
                            }
                        },
                        InboxType = InboxType.Outbound,
                    }
                }

            });

            Assert.True(result.Success);
        }

        [Fact(DisplayName = "ສະແດງ ເອກະສານ info ຕາມ DocID")]
        public async Task NC3()
        {
            var result = await documentService.GetDocument("cec5e5ba87ce4de38a4592c3803299e2");

            Assert.True(result.Response.Success);
        }

        [Fact(DisplayName = "ສົ່ງຕໍ່ເອກະສານ ຫຼື ເປັນການເພີ່ມ ຜູ້ຮັບເຂົ້າໄປໃນການໄຫຼຂອງເອກະສານ")]
        public async Task NC4()
        {
            var result = await documentService.SendDocument("cec5e5ba87ce4de38a4592c3803299e2", new List<Reciepient>
            {
                new Reciepient
                {

                }
            });

            Assert.True(result.Success);
        }
        [Fact(DisplayName = "ສະແດງ ເສັ້ນທາງການໄຫຼຂອງເອກະສານ")]
        public async Task NC5()
        {

        }
        [Fact(DisplayName = "ໃຫ້ຄຳເຫັນຕາມຕຳແໜ່ງທີ່ໄດ້ຮັບ")]
        public async Task NC6()
        {

        }
        [Fact(DisplayName = "ອັບເດດຂໍ້ມູນຕາມຕຳແໜ່ງທີ່ໄດ້ຮັບ")]
        public async Task NC7()
        {

        }
        [Fact(DisplayName = "ລຶບ ເອກະສານ ອີງຕາມການດຳເນີນເອກະສານ ຂາເຂົ້າ-ຂາອອກ")]
        public async Task NC8()
        {

        }
        [Fact(DisplayName = "ສະແດງ ເອກະສານ ທັງຫມົດ ຕາມ RoleID ຕາມ Document Window")]
        public async Task NC9()
        {
            var result = await documentService.GetDocumentByRoleId("bcd52dd6296d4a44ba2b93733a699b48", InboxType.Outbound, TraceStatus.Draft);
            Assert.NotEqual(0, result.RowCount);
        }
    }
}

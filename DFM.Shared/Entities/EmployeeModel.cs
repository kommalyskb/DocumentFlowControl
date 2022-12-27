using DFM.Shared.Common;
using DFM.Shared.DTOs;
using Redis.OM.Modeling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFM.Shared.Entities
{
    [Document(StorageType = StorageType.Json, Prefixes = new[] { "EmployeeModel" })]
    public class EmployeeModel : HeaderModel
    {
        [Indexed(CascadeDepth = 1)]
        public MultiLanguage Name { get; set; } = new();
        [Indexed(CascadeDepth = 1)]
        public MultiLanguage FamilyName { get; set; } = new();
        [Indexed]
        public Gender Gender { get; set; }
        [Indexed]
        public string? Dob { get; set; }
        [Indexed(CascadeDepth = 1)]
        public Address CurrentAddress { get; set; } = new();
        [Indexed(CascadeDepth = 1)]
        public Address BornAddress { get; set; } = new();
        [Indexed(CascadeDepth = 1)]
        public Contact Contact { get; set; } = new();
        [Indexed(CascadeDepth = 1)]
        public PersonalIdentity PersonalIdentity { get; set; } = new();
        [Indexed(CascadeDepth = 1)]
        public NationalIdentity NationalIdentity { get; set; } = new();
        [Indexed(CascadeDepth = 1)]
        public FamilyInfo FamilyInfo { get; set; } = new();
        [Indexed]
        public string? UserID { get; set; } // ແມ່ນ ID ທີ່ໄດ້ມາຈາກ Identity Server
        [Indexed]
        public string? OrganizationID { get; set; }
        [Indexed]
        public string? EmployeeID { get; set; }
        [Indexed]
        public string? Username { get; set; }
        [Indexed]
        public string? Password { get; set; }
    }
    
}

﻿using DFM.Shared.DTOs;
using Minio.DataModel;
using Redis.OM.Modeling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFM.Shared.Entities
{
    [Document(StorageType = StorageType.Json, Prefixes = new[] { "DynamicFlowModel" })]
    public class DynamicFlowModel : HeaderModel
    {
        [Indexed(CascadeDepth = 1)]
        public List<DynamicItem>? RoleTypeItems { get; set; } = new();
       
    }
    public class DynamicItem
    {
        [Indexed]
        public RoleTypeModel RoleSource { get; set; }
        [Indexed]
        public List<RoleTypeModel>? RoleTargets { get; set; }
        [Indexed]
        public ModuleType ModuleType { get; set; }
        [Indexed]
        public bool CrossInternal { get; set; }
        [Indexed]
        public bool CrossExternal { get; set; } = false;
    }
    public enum ModuleType
    {
        DocumentInbound,
        DocumentOutbound,
        TaskManagement,
        Leave

    }
}

﻿using DFM.Shared.Common;
using DFM.Shared.DTOs;
using DFM.Shared.Entities;
using DFM.Shared.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OpenTelemetry.Resources;
using System.Security.Cryptography;

namespace DFM.API.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Authorize]
    public class OrganizationController : ControllerBase
    {
        private readonly IEmployeeManager employeeManager;
        private readonly IOrganizationChart organizationChart;
        private readonly IRoleManager roleManager;

        public OrganizationController(IEmployeeManager employeeManager, IOrganizationChart organizationChart, IRoleManager roleManager)
        {
            this.employeeManager = employeeManager;
            this.organizationChart = organizationChart;
            this.roleManager = roleManager;
        }

        [HttpGet("GetRole")]
        //[AllowAnonymous]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(typeof(IEnumerable<TabItemDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommonResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetRoleV1(CancellationToken cancellationToken = default(CancellationToken))
        {

            // Get UserID
            var userId = "";

            if (User.Claims.FirstOrDefault(x => x.Type == "sub") != null)
            {
                userId = User.Claims.FirstOrDefault(x => x.Type == "sub")!.Value;

            }

            // Get User Profile
            var userProfile = await employeeManager.GetProfile(userId, cancellationToken);

            if (!userProfile.Response.Success)
            {
                return BadRequest(userProfile.Response);
            }

            // Get role on organization
            var result = await organizationChart.GetRoles(userProfile.Content.OrganizationID!, userProfile.Content.id!, cancellationToken);
            if (result.Response.Success)
            {
                return Ok(result.Content);

            }
            return BadRequest(result.Response);
        }

        [HttpGet("GetRole/{userId}")]
        //[AllowAnonymous]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(typeof(IEnumerable<TabItemDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommonResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetRoleWithUserIDV1(string? userId, CancellationToken cancellationToken = default(CancellationToken))
        {

            // Get User Profile
            var userProfile = await employeeManager.GetProfile(userId!, cancellationToken);

            if (!userProfile.Response.Success)
            {
                return BadRequest(userProfile.Response);
            }

            // Get role on organization
            var result = await organizationChart.GetRoles(userProfile.Content.OrganizationID!, userProfile.Content.id!, cancellationToken);
            if (result.Response.Success)
            {
                return Ok(result.Content);

            }
            return BadRequest(result.Response);
        }

        [HttpGet("GetItem/{id}")]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(typeof(IEnumerable<RoleTreeModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommonResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetItemV1(string id, CancellationToken cancellationToken = default(CancellationToken))
        {
            var result = await organizationChart.GetChartByID(id, cancellationToken);
            if (result.Response.Success)
            {
                return Ok(result.Contents);

            }
            return BadRequest(result.Response);
        }

        [HttpGet("GetItem/{id}/{roleId}/{link}")]
        [AllowAnonymous]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(typeof(IEnumerable<RoleTreeModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommonResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetItemV1(string id, string roleId, string link, CancellationToken cancellationToken = default(CancellationToken))
        {
            ModuleType documentModule = ModuleType.DocumentInbound;
            if (link == "inbound")
            {
                documentModule = ModuleType.DocumentInbound;
            }
            else
            {
                documentModule = ModuleType.DocumentOutbound;
            }
            var result = await organizationChart.GetChartFrom(id, roleId, documentModule, cancellationToken);
            if (result.Response.Success)
            {
                return Ok(result.Contents);

            }
            return BadRequest(result.Response);
        }


        [HttpGet("GetSupervisors/{orgId}")]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(typeof(IEnumerable<RoleTreeModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommonResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetSupervisorV1(string orgId, CancellationToken cancellationToken = default(CancellationToken))
        {
            var result = await organizationChart.GetSupervisorRolesPosition(orgId, cancellationToken);
            if (result.Response.Success)
            {
                return Ok(result.Contents);
            }

            return BadRequest(result.Response);

        }

        [HttpGet("GetPublisher/{id}/{roleId}")]
        //[AllowAnonymous]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(typeof(CommonResponseId), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommonResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetPublisherV1(string id, string roleId, CancellationToken cancellationToken = default(CancellationToken))
        {
            var result = await organizationChart.GetPublisher(id, roleId, cancellationToken);
            if (result.Success)
            {
                return Ok(result);

            }
            return BadRequest(result);
        }

        [HttpPost("NewItem")]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(typeof(CommonResponseId), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommonResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> NewItemV1([FromBody] NewOrganizationRequest request, CancellationToken cancellationToken = default(CancellationToken))
        {
            var result = await organizationChart.NewOrganization(request.Name, request.Attachment, cancellationToken);

            return Ok(result);
        }
        [HttpPost("SavePosition/{orgId}")]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(typeof(CommonResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommonResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> SavePositionV1(string orgId, [FromBody] RoleTreeModel request, CancellationToken cancellationToken = default(CancellationToken))
        {
            // Find employee profile
            var profile = await employeeManager.GetProfile(request.Employee.UserID!, cancellationToken);
            if (!profile.Response.Success)
            {
                return BadRequest(profile.Response);
            }

            if (string.IsNullOrWhiteSpace(request.Role.RoleID))
            {
                request.Role.RoleID = Guid.NewGuid().ToString("N");
                // Create new Role
                var roleResult = await roleManager.NewRolePosition(new RoleManagementModel
                {
                    id = request.Role.RoleID,
                    Display = request.Role.Display,
                    OrganizationID = orgId,
                    RoleType = request.RoleType,
                }, cancellationToken);
                if (!roleResult.Success)
                {
                    return BadRequest(roleResult);
                }

                
            }
            if (string.IsNullOrWhiteSpace(request.ParentID))
            {
                request.ParentID = "0";
            }
            var result = await organizationChart.AddRoleAndEmployee(orgId, new RoleTreeModel
            {
                RoleType = request.RoleType,
                ParentID = request.ParentID,
                Employee = new PartialEmployeeProfile
                {
                    Name = profile.Content.Name,
                    FamilyName = profile.Content.FamilyName,
                    Gender = profile.Content.Gender,
                    UserID = profile.Content.id
                },
                Role = new PartialRole
                {
                    RoleID = request.Role.RoleID,
                    RoleType = request.RoleType,
                    Display = request.Role.Display
                },
                Publisher = request.Publisher
            }, cancellationToken);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);

        }

        [HttpGet("RemovePosition/{orgId}/{roleId}")]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(typeof(CommonResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommonResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RemovePositionV1(string orgId, string roleId, CancellationToken cancellationToken = default(CancellationToken))
        {
            var result = await organizationChart.RemoveRoleAndEmployee(orgId, roleId, cancellationToken);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpGet("GetDynamicFlow/{moduleType}/{orgId}")]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(typeof(IEnumerable<DynamicItem>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommonResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetDynamicFlowV1(ModuleType moduleType, string orgId, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                var result = await organizationChart.GetDynamicFlowByID(orgId, moduleType, cancellationToken);

                
                if (!result.Response.Success)
                {
                    return BadRequest(result.Response);
                }

                return Ok(result.Roles);
            }
            catch (Exception)
            {

                throw;
            }
        }

        [HttpPost("SaveDynamicFlow/{orgId}")]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(typeof(CommonResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommonResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> SaveDynamicFlowV1(string orgId, [FromBody] DynamicItem request, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                var result = await organizationChart.SaveDynamicFlow(orgId, request.RoleSource, request.RoleTargets!, request.ModuleType, cancellationToken: cancellationToken);


                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}

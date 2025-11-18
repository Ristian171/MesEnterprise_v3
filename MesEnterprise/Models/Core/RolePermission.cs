using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using MesEnterprise.Models.Core;

namespace MesEnterprise.Models.Core
{
    public class RolePermission
    {
        public int RoleId { get; set; }

        public int PermissionId { get; set; }

        // Navigation properties
        public Role? Role { get; set; }
        public Permission? Permission { get; set; }
    }
}
using RedLineLanka_Enterprise.Common;
using RedLineLanka_Enterprise.Common.DB;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Web;

namespace RedLineLanka_Enterprise.Areas.Admin.Models
{
    [Serializable]
    public class UserRoleVM : IModel<UserRole, UserRoleVM>
    {
        public UserRoleVM()
        {
            mappings = new ObjMappings<UserRole, UserRoleVM>();
            mappings.Add(x => x.Role == null ? "-" : x.Role.Name, x => x.RoleName);
        }
        public UserRoleVM(UserRole obj)
            : this()
        {
            this.SetEntity(obj);
        }

        public ObjMappings<UserRole, UserRoleVM> mappings { get; set; }

        public int UserRoleID { get; set; }
        public int UserID { get; set; }
        [DisplayName("Role")]
        public int RoleID { get; set; }

        [DisplayName("Role")]
        public string RoleName { get; set; }
    }
}

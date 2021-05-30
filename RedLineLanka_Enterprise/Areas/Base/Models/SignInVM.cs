using RedLineLanka_Enterprise.Common;
using RedLineLanka_Enterprise.Common.DB;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace RedLineLanka_Enterprise.Areas.Base.Models
{
    [Serializable]
    public class SignInVM : IModel<User, SignInVM>
    {
        public SignInVM()
        {
            mappings = new ObjMappings<User, SignInVM>();
            mappings.Add(x => x.Password, x => x.PassWord.Encrypt());
            mappings.Add(x => x.Password.Decrypt(), x => x.PassWord);
        }
        public SignInVM(User obj)
            : this()
        {
            this.SetEntity(obj);
        }

        public ObjMappings<User, SignInVM> mappings { get; set; }
        
        public int UserID { get; set; }
        public string UserName { get; set; }
        public string PassWord { get; set; }
        public Nullable<int> EmployeeID { get; set; }
        public bool Active { get; set; }
        public string CreatedBy { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public Nullable<System.DateTime> ModifiedDate { get; set; }
        
        public bool RememberMe { get; set; }
        [DisplayName("New Password")]
        public string NewPassword { get; set; }
        [DisplayName("Confirm Password")]
        public string ConfirmPassword { get; set; }
    }
}
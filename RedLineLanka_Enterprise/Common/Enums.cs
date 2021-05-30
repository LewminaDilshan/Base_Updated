using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace RedLineLanka_Enterprise.Common
{
    public static class UserRoles
    {
        public const string Admin = "Admin";
        public const string User = "User";
        public const string HrUser = "HrUser";
        public const string ProgOfficeUser = "ProgOfficeUser";
        public const string LecturerUser = "LecturerUser";
        public const string AdminUser = "AdminUser";
        public const string FinanceUser = "FinanceUser";
        public const string ExamUser = "ExamUser";
    }

    public enum Log4NetMsgType
    {
        Error = 0,
        Warning = 1,
        Info = 2
    }

    public enum SysPara
    {
        DG_EmployeeID = 1,
        HRM_AO = 2,
        HRM_DD = 3,
        HRM_MA = 4,
        Intake_Start_Date = 5,
        Intake_End_Date = 6,
        Lec_Subsistence_Allow = 7,
        Exec_Subsistence_Allow = 8,
        Otr_Subsistence_Allow = 9,
        Lec_Transport_Col_KaMa = 10,
        Lec_Transport_Col_KuGl = 11,
        Lec_Transport_Gl_Ma = 12,
        Lec_Transport_Ka_Ku = 13,
        ExecOtr_Transport_Col_KaMa = 14,
        ExecOtr_Transport_Col_KuGl = 15,
        Leave_Setup_Completed_Year = 16,
        Chairman_EmployeeID = 17,
        BorderlineApprover_MIS = 20,
        BorderlineApprover_PMD = 21,
        InsuClaimStartDate = 22,
        InsuClaimEndDate = 23,
        InsuLimit = 24,
        Evaluation_User = 25,
        Viva_User = 26,
        ActingDG_EmpID = 27,
        ActingDG = 28,
        HRM_OTTimeTracker = 29,
        ComBillUser = 30,
        MaintenanceOfficer = 31,
        MaintenanceHead = 32,
        DOU_Coordinator = 33,
        ID_Printer = 34,
        LastBOCGateway = 35,
        LastSeylanGateway = 36
    }

    public enum ActiveState
    {
        [Description("Active")]
        Active = 1,
        [Description("Inactive")]
        Inactive = 0
    }

    public enum Gender
    {
        [Description("Male")]
        Male = 0,
        [Description("Female")]
        Female = 1
    }
}

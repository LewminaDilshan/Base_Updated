using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RedLineLanka_Enterprise.Areas.Base.Models
{
    [Serializable]
    public class ReportParameterVM
    {
        [DisplayName("Branch")]
        public int? BranchID { get; set; }
        [DisplayName("Department")]
        public int? DepartmentID { get; set; }
        [DisplayName("Course")]
        public int? CourseID { get; set; }
        [DisplayName("Batch")]
        public int? BatchID { get; set; }
        [DisplayName("From Date")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? FromDate { get; set; }
        [DisplayName("To Date")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? ToDate { get; set; }
        [DisplayName("General Code")]
        public int? NumericPara1 { get; set; }
        public int? NumericPara2 { get; set; }
        [DisplayName("Employee")]
        public int? EmployeeID { get; set; }
        [DisplayName("Vehicle No")]
        public int? VehicleID { get; set; }
        [DisplayName("Lecturer")]
        public int? SubLecID { get; set; }
        [DisplayName("Registration No")]
        public string RegistrationNo { get; set; }
        [DisplayName("Exam Schedule")]
        public int? ExamScheduleID { get; set; }
        [DisplayName("List Start Number")]
        public int? ListStart { get; set; } = 1;
        [DisplayName("List End Number")]
        public int? ListEnd { get; set; }
        [DisplayName("Total Candidates")]
        public int? NoOfCandidates { get; set; }
        public int? NoOfRepeaters { get; set; }
        [DisplayName("Hall Name")]
        public string HallName { get; set; }
        [DisplayName("Hall Max Capacity")]
        public string HallExamCapacity { get; set; }
        [DisplayName("Select All Employees")]
        public bool SelectAllEmploye { get; set; }
        [DisplayName("Select All Vehicles")]
        public bool SelectAllVehicles { get; set; }

        [DisplayName("From Date 2")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? FromDate2 { get; set; }
        [DisplayName("To Date 2")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? ToDate2 { get; set; }

        [DisplayName("Is Special")]
        public bool IsSpecial { get; set; }
        [DisplayName("Duration From")]
        public int? KpiDateID { get; set; }

        [DisplayName("Duration From")]
        public int? VisitKpiDateID { get; set; }
        public string StringPara1 { get; set; }
        public ICollection<int> BranchIDs { get; set; }

        public Dictionary<int, List<DateTime>> FingerPrintDictionary { get; set; }

        public bool IsLecturer { get; set; }

        [DisplayName("Branch")]
        public string Branch { get; set; }
        [DisplayName("Department")]
        public string Department { get; set; }
        public bool AdminDG { get; set; }
        public bool evaluationUser { get; set; }
        [DisplayName("Batch Start")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? BatchStart { get; set; }
        [DisplayName("Batch End")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? BatchEnd { get; set; }
        [DisplayName("Sub Department")]
        public Nullable<int> SubDeptID { get; set; }
        [DisplayName("Sub Department")]
        public string SubDepartmentDesc { get; set; }

        public ICollection<RedLineLanka_Enterprise.Areas.Base.Models.ReportParameterVM> EnvelopeList { get; set; }

        public bool EnvGenerate { get; set; }
        [DisplayName("Sponsor")]
        public Nullable<int> SponsorStudID { get; set; }
        public string IndexNo { get; set; }
        public string StudentName { get; set; }
        public string Address { get; set; }
        public int CourseRegID { get; set; }
        public int Duration { get; set; }

        [DisplayName("Last Certificate Number")]
        public int LastCertificateNumber {get;set;}
        public string Prefix { get; set; }
        [DisplayName("Printed Date")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? PrintedDate { get; set; }
        [DisplayName("CSI End Date")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? CSIEndDate { get; set; }

        [DisplayName("Subject")]
        public int? SubjectID { get; set; }

        [DisplayName("BatchSubject")]
        public int? BatchSubjectID { get; set; }
        public string CertificateNo { get; set; }
        public int? NumericPara3 { get; set; }
        public int? NumericPara4 { get; set; }
        public int? NumericPara5 { get; set; }
        public decimal DecimalPara1 { get; set; }
        public decimal DecimalPara2 { get; set; }
        public decimal DecimalPara3 { get; set; }
        public decimal DecimalPara4 { get; set; }
        public decimal DecimalPara5 { get; set; }
        public decimal DecimalPara6 { get; set; }
        public decimal DecimalPara7 { get; set; }
        public decimal DecimalPara8 { get; set; }
        public string StringPara2 { get; set; }
        [DataType(DataType.MultilineText)]
        public string StringMultiPara1 { get; set; }
        public bool BoolPara { get; set; }
    }
}
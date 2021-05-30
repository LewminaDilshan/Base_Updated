using RedLineLanka_Enterprise.Common;
using RedLineLanka_Enterprise.Common.DB;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RedLineLanka_Enterprise.Areas.Base.Models
{
    [Serializable]
    public class DashBoardVM
    {
        public bool HideMenu { get; set; } = false;
        public ChartConfig IntakeEnrolBudVsActTotal { get; set; }
        public ChartConfig FulltimeStudents { get; set; }
        public ChartConfig PartTimeStudents { get; set; }
        public ChartConfig IntakeEnrolBudVsActCol { get; set; }
        public ChartConfig IntakeEnrolBudVsActKan { get; set; }
        public ChartConfig IntakeEnrolBudVsActKur { get; set; }
        public ChartConfig IntakeEnrolBudVsActGal { get; set; }
        public ChartConfig IntakeEnrolBudVsActMat { get; set; }

        public ChartConfig IntakeRevBudVsActTotal { get; set; }
        public ChartConfig IntakeRevBudVsActCol { get; set; }
        public ChartConfig IntakeRevBudVsActKan { get; set; }
        public ChartConfig IntakeRevBudVsActKur { get; set; }
        public ChartConfig IntakeRevBudVsActGal { get; set; }
        public ChartConfig IntakeRevBudVsActMat { get; set; }

        public ChartConfig DropoutTotal { get; set; }
        public ChartConfig DropoutCol { get; set; }
        public ChartConfig DropoutKan { get; set; }
        public ChartConfig DropoutGal { get; set; }
        public ChartConfig DropoutMat { get; set; }
        public ChartConfig DropoutKur { get; set; }

        public ChartConfig NewEnrollmentsAllProg { get; set; }
        public ChartConfig TotalOngoingActiveStudCount { get; set; }
        public ChartConfig NewEnrollmentCol { get; set; }
        public ChartConfig NewEnrollmentKan { get; set; }
        public ChartConfig NewEnrollmentGal { get; set; }
        public ChartConfig NewEnrollmentKur { get; set; }
        public ChartConfig NewEnrollmentMat { get; set; }
        public ChartConfig OngoingActDivWise { get; set; }

        public ChartConfig HomeTodayIncome { get; set; }
        public ChartConfig HomeTodayEnroll { get; set; }
        public ChartConfig HomeTodayPublishedResults { get; set; }
        public ChartConfig HomeTodayExam { get; set; }
        public ChartConfig HomeTodayInquiries { get; set; }
        public ChartConfig HomeTodayLectureSessions { get; set; }
        public int TotalOngoingActiveStuds { get; set; }
        public int TotalOngoingActiveStuds_FT { get; set; }
        public int TotalOngoingActiveStuds_PT { get; set; }
        public int TotalNewActivStuds { get; set; }
        public int TotalNewActivStuds_FT { get; set; }
        public int TotalNewActivStuds_PT { get; set; }

        public ChartConfig IntakeEnrollmentsBudVsAct { get; set; }
        public ChartConfig IntakeRevenueBudVsAct { get; set; }
        public ChartConfig IntakeEnrolFullTime { get; set; }
        public ChartConfig IntakeEnrolPartTime { get; set; }
        public ChartConfig IntakeInquiries { get; set; }
        public ChartConfig IntakeStudentsByAware { get; set; }
        public ChartConfig IntakeStudentsByGender { get; set; }
        public ChartConfig IntakeEnrollmentsByMonth { get; set; }
        public ChartConfig IntakeProgramCommencements { get; set; }
        public ChartConfig IntakeEnrolByCourseCategory { get; set; }
        public int IntakeEnrollmentsTotal { get; set; }
        public string IntakeRevenueTotal { get; set; }

        public ChartConfig EnrollmentsBudVsAct { get; set; }
        public ChartConfig RevenueBudVsAct { get; set; }
        public ChartConfig EnrolmentsByCourseType { get; set; }
        public ChartConfig ProgramsByLectureMode { get; set; }
        public ChartConfig TotalDropouts { get; set; }
        public ChartConfig StudentsByAware { get; set; }
        public ChartConfig TotalDropoutsByMonth { get; set; }
        public ChartConfig EnrollmentsByMonth { get; set; }
        public ChartConfig ProgramCommencements { get; set; }
        public ChartConfig EnrolByCourseCategory { get; set; }
        public ChartConfig ConductedLectures { get; set; }
        public ChartConfig ExamResultsPublished { get; set; }
        public ChartConfig ExamsHeld { get; set; }
        public int EnrollmentsTotal { get; set; }
        public string RevenueTotal { get; set; }

        public int TotalDropoutsCount { get; set; }

        [DisplayName("From date")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime StartPeriod { get; set; } = new DateTime(DateTime.Now.Year, 1, 1);
        [DisplayName("To date")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime EndPeriod { get; set; } = new DateTime(DateTime.Now.Year, 12, 31);
    }

    public class ChartConfig
    {
        public string type { get; set; }
        public object data { get; set; }
        public object options { get; set; }
        public object scales { get; set; }
    }
}
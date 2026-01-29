using System;
using System.Collections.Generic;
using WebsiteQL_Testcase.Models.Enums; // Giữ lại nếu cần dùng Enum ở chỗ khác

namespace WebsiteQL_Testcase.ViewModels
{
    public class DashboardIndexViewModel
    {
        // 1. Các chỉ số thống kê (Giữ nguyên)
        public int TotalCases { get; set; }
        public int PassCount { get; set; }
        public int FailCount { get; set; }
        public double PassRate { get; set; }
        public double FailRate { get; set; }

        // 2. Hai danh sách dữ liệu (Mình đã sửa lại để khớp với View mới)
        public List<LatestTestCaseViewModel> LatestTestCases { get; set; } = new();
        public List<LatestProjectViewModel> LatestProjects { get; set; } = new();
    }

    // Class con cho Test Case (Sửa để khớp giao diện)
    public class LatestTestCaseViewModel
    {
        public Guid Id { get; set; }

        // View cần "Name" và "Status", mình ánh xạ từ Title và Result sang
        public string Name { get; set; } = "";
        public string Status { get; set; } = ""; // "Pass", "Fail", "Draft"...

        // Các trường cũ (có thể giữ lại phòng khi cần dùng sau này)
        public string Code { get; set; } = "";
        public string TestSuiteName { get; set; } = "";
    }

    // Class con cho Project (Sửa để khớp giao diện)
    public class LatestProjectViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = "";

        // View cần hiển thị ngày cập nhật
        public DateTime UpdatedDate { get; set; }
    }
}
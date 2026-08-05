namespace SchoolCRM.Shared.Constants;

public static class Roles
{
    public const string SuperAdmin = "SuperAdmin";
    public const string SchoolAdmin = "SchoolAdmin";
    public const string Principal = "Principal";
    public const string VicePrincipal = "VicePrincipal";
    public const string Teacher = "Teacher";
    public const string ClassTeacher = "ClassTeacher";
    public const string Accountant = "Accountant";
    public const string Receptionist = "Receptionist";
    public const string Librarian = "Librarian";
    public const string Student = "Student";
    public const string Parent = "Parent";

    public static readonly string[] AllRoles = new[]
    {
        SuperAdmin, SchoolAdmin, Principal, VicePrincipal, Teacher,
        ClassTeacher, Accountant, Receptionist, Librarian, Student, Parent
    };

    public static readonly string[] AdminRoles = new[] { SuperAdmin, SchoolAdmin, Principal, VicePrincipal };
    public static readonly string[] TeachingRoles = new[] { Teacher, ClassTeacher };
    public static readonly string[] StaffRoles = new[] { Teacher, ClassTeacher, Accountant, Receptionist, Librarian };
}

public static class Permissions
{
    public const string StudentsView = "Students.View";
    public const string StudentsCreate = "Students.Create";
    public const string StudentsEdit = "Students.Edit";
    public const string StudentsDelete = "Students.Delete";
    public const string StudentsExport = "Students.Export";
    public const string StudentsImport = "Students.Import";

    public const string TeachersView = "Teachers.View";
    public const string TeachersCreate = "Teachers.Create";
    public const string TeachersEdit = "Teachers.Edit";
    public const string TeachersDelete = "Teachers.Delete";

    public const string ParentsView = "Parents.View";
    public const string ParentsCreate = "Parents.Create";
    public const string ParentsEdit = "Parents.Edit";
    public const string ParentsDelete = "Parents.Delete";

    public const string EmployeesView = "Employees.View";
    public const string EmployeesCreate = "Employees.Create";
    public const string EmployeesEdit = "Employees.Edit";
    public const string EmployeesDelete = "Employees.Delete";

    public const string AttendanceView = "Attendance.View";
    public const string AttendanceMark = "Attendance.Mark";
    public const string AttendanceEdit = "Attendance.Edit";

    public const string ExamsView = "Exams.View";
    public const string ExamsCreate = "Exams.Create";
    public const string ExamsEdit = "Exams.Edit";
    public const string ExamsDelete = "Exams.Delete";
    public const string ExamsPublish = "Exams.Publish";

    public const string FeesView = "Fees.View";
    public const string FeesCollect = "Fees.Collect";
    public const string FeesManage = "Fees.Manage";

    public const string LibraryView = "Library.View";
    public const string LibraryIssue = "Library.Issue";
    public const string LibraryReturn = "Library.Return";

    public const string ReportsView = "Reports.View";
    public const string ReportsExport = "Reports.Export";

    public const string SettingsView = "Settings.View";
    public const string SettingsManage = "Settings.Manage";
    public const string RolesManage = "Roles.Manage";
    public const string PermissionsManage = "Permissions.Manage";

    public const string DashboardView = "Dashboard.View";
    public const string NotificationsView = "Notifications.View";

    public static readonly string[] AllPermissions = new[]
    {
        StudentsView, StudentsCreate, StudentsEdit, StudentsDelete, StudentsExport, StudentsImport,
        TeachersView, TeachersCreate, TeachersEdit, TeachersDelete,
        ParentsView, ParentsCreate, ParentsEdit, ParentsDelete,
        EmployeesView, EmployeesCreate, EmployeesEdit, EmployeesDelete,
        AttendanceView, AttendanceMark, AttendanceEdit,
        ExamsView, ExamsCreate, ExamsEdit, ExamsDelete, ExamsPublish,
        FeesView, FeesCollect, FeesManage,
        LibraryView, LibraryIssue, LibraryReturn,
        ReportsView, ReportsExport,
        SettingsView, SettingsManage, RolesManage, PermissionsManage,
        DashboardView, NotificationsView
    };
}

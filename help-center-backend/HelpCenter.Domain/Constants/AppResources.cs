namespace HelpCenter.Domain.Constants;

public static class AppResources
{
    public const string Dashboard = "Dashboard";
    public const string Requests = "Requests";
    public const string Companies = "Companies";
    public const string Users = "Users";
    public const string FAQ = "FAQ";
    public const string Guide = "Guide";
    public const string Roles = "Roles";
    public const string Modules = "Modules";
    public const string Projects = "Projects";
    public const string Subjects = "Subjects";
    public const string Statuses = "Statuses";
    public const string Customers = "Customers";
    public const string CustomerRequests = "CustomerRequests";
    public const string AssignedRequests = "AssignedRequests";
    public const string OrganizationSettings = "OrganizationSettings";
}

public static class DashboardWidgets
{
    // Metric cards
    public const string TotalRequests = "totalRequests";
    public const string ActiveRequests = "activeRequests";
    public const string CompletedRequests = "completedRequests";
    public const string TotalMessages = "totalMessages";
    public const string TotalCustomers = "totalCustomers";
    public const string TotalCompanies = "totalCompanies";

    // Page components
    public const string TicketFlow = "ticketFlow";
    public const string WeeklyChart = "weeklyChart";
    public const string RecentTable = "recentTable";
    public const string DonutChart = "donutChart";
    public const string SystemHealth = "systemHealth";
    public const string QuickActions = "quickActions";

    // Report components
    public const string StatusDistribution = "statusDistribution";
    public const string ModuleDistribution = "moduleDistribution";
    public const string CompanyTopN = "companyTopN";
    public const string DailyTrend = "dailyTrend";
    public const string AvgResolutionMinutes = "avgResolutionMinutes";
    public const string AgentPerformance = "agentPerformance";
    public const string ReopenRate = "reopenRate";

    public static readonly string[] All =
    [
        TotalRequests, ActiveRequests, CompletedRequests,
        TotalMessages, TotalCustomers, TotalCompanies,
        TicketFlow, WeeklyChart, RecentTable, DonutChart, SystemHealth, QuickActions,
        StatusDistribution, ModuleDistribution, CompanyTopN,
        DailyTrend, AvgResolutionMinutes, AgentPerformance, ReopenRate
    ];
}

public static class PermissionActions
{
    // Standard CRUD and general operations
    public const string Read = "Read";
    public const string Create = "Create";
    public const string Update = "Update";
    public const string Delete = "Delete";
    public const string Export = "Export";
    public const string Print = "Print";
    public const string Approve = "Approve";
    public const string Reject = "Reject";
    public const string Assign = "Assign";
    public const string ChangeStatus = "ChangeStatus";

    // Contextual (sub-scope) actions — permission to manage other resources under a given resource
    public const string ManageMembers = "ManageMembers";   // Projects, Companies → management of associated users
    public const string ManageModules = "ManageModules";   // Projects → module assignment
    public const string ManageExperts = "ManageExperts";   // Modules → expert assignment
    public const string ManageProjects = "ManageProjects";  // Companies → project assignment
    public const string LookupSelect = "LookupSelect";    // General picker/dropdown permission
}

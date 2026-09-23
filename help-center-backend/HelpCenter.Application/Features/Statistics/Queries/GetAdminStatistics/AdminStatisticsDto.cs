namespace HelpCenter.Application.Features.Statistics.Queries.GetAdminStatistics;

public class AdminStatisticsDto
{
    public int? TotalRequests { get; set; }
    public int? ActiveRequests { get; set; }
    public int? CompletedRequests { get; set; }
    public int? TotalMessages { get; set; }
    public int? TotalCustomers { get; set; }
    public int? TotalCompanies { get; set; }
    public List<string>? AllowedFields { get; set; }

    public void SetMetric(string metricKey, int value)
    {
        switch (metricKey.ToLowerInvariant())
        {
            case "totalrequests": TotalRequests = value; break;
            case "activerequests": ActiveRequests = value; break;
            case "completedrequests": CompletedRequests = value; break;
            case "totalmessages": TotalMessages = value; break;
            case "totalcustomers": TotalCustomers = value; break;
            case "totalcompanies": TotalCompanies = value; break;
        }
    }
}

namespace MultitenancyApp.DatabaseContext;

public class TenantInfo
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public string Data { get; set; }
}
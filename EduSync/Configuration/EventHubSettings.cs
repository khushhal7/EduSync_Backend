// In Configuration/EventHubSettings.cs (Backend Project)
namespace EduSync.Configuration // Or your project&#39;s configuration/settings namespace
{
    public class EventHubSettings
    {
        public string? ConnectionString { get; set; }
        public string? EventHubName { get; set; }
    }
}
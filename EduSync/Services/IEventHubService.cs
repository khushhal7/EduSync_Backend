// In Services/IEventHubService.cs (Backend Project)
using System.Threading.Tasks;

namespace EduSync.Services // Or your project's namespace
{
    public interface IEventHubService
    {
        /// <summary>
        /// Sends an event (as a JSON string) to the configured Azure Event Hub.
        /// </summary>
        /// <param name="eventDataJson">The event data serialized as a JSON string.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task SendEventAsync(string eventDataJson);

        // You can add more specific methods later if needed, e.g.:
        // Task SendQuizAttemptEventAsync(QuizAttemptEventData eventData);
    }
}

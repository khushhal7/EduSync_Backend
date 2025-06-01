// In Services/EventHubService.cs (Backend Project)
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using Microsoft.Extensions.Options;
using System;
using System.Text;
using System.Threading.Tasks;
using EduSync.Configuration; // Or EduSync.Settings, depending on where EventHubSettings.cs is

namespace EduSync.Services // Or your project's namespace
{
    public class EventHubService : IEventHubService, IAsyncDisposable
    {
        private readonly EventHubSettings _eventHubSettings;
        private readonly EventHubProducerClient _producerClient;

        public EventHubService(IOptions<EventHubSettings> eventHubSettings)
        {
            _eventHubSettings = eventHubSettings.Value;

            if (string.IsNullOrEmpty(_eventHubSettings.ConnectionString))
            {
                throw new InvalidOperationException("Event Hub ConnectionString is not configured.");
            }
            if (string.IsNullOrEmpty(_eventHubSettings.EventHubName))
            {
                throw new InvalidOperationException("Event Hub Name is not configured.");
            }

            // Create a producer client that you can use to send events to an event hub
            _producerClient = new EventHubProducerClient(
                _eventHubSettings.ConnectionString,
                _eventHubSettings.EventHubName
            );
        }

        /// <summary>
        /// Sends an event (as a JSON string) to the configured Azure Event Hub.
        /// </summary>
        /// <param name="eventDataJson">The event data serialized as a JSON string.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task SendEventAsync(string eventDataJson)
        {
            if (string.IsNullOrEmpty(eventDataJson))
            {
                // Or handle this as a non-critical error, depending on requirements
                throw new ArgumentException("Event data cannot be null or empty.", nameof(eventDataJson));
            }

            try
            {
                // Create a batch of events 
                using EventDataBatch eventBatch = await _producerClient.CreateBatchAsync();

                var eventData = new EventData(Encoding.UTF8.GetBytes(eventDataJson));

                if (!eventBatch.TryAdd(eventData))
                {
                    // If the single event is too large for a batch (unlikely for typical JSON strings, but possible)
                    // Or if the batch is full after adding other events (if we were sending multiple)
                    throw new Exception($"Event data for {eventDataJson.Substring(0, Math.Min(eventDataJson.Length, 50))}... is too large for a single batch.");
                }

                // Use the producer client to send the batch of events to the Event Hub
                await _producerClient.SendAsync(eventBatch);

                System.Diagnostics.Debug.WriteLine($"Event sent to Event Hub '{_eventHubSettings.EventHubName}'. Data: {eventDataJson.Substring(0, Math.Min(eventDataJson.Length, 100))}...");
            }
            catch (Exception ex)
            {
                // Log the exception (using System.Diagnostics.Debug for now, replace with ILogger in a real app)
                System.Diagnostics.Debug.WriteLine($"Error sending event to Event Hub: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                // Rethrow or handle as appropriate for your application
                throw;
            }
        }

        // Implement IAsyncDisposable to properly dispose of the producer client
        public async ValueTask DisposeAsync()
        {
            if (_producerClient != null)
            {
                await _producerClient.DisposeAsync();
            }
            GC.SuppressFinalize(this);
        }
    }
}

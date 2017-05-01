using System;

namespace PacodelaCruz.PollingConsumer.FunctionApp
{
    /// <summary>
    /// This class will help us to wrap the PollingWatermarkEntity and make it more user friendly. By using the constructor, we are naming the PartitionKey as SourceSystem, and the RowKey as Entity. Additionally, we are returning another property called NextWatermark that is to be used as the upper bound when querying the source system and when updating the Polling Watermark after we have successfully polled the source system
    /// </summary>
    public class PollingWatermark
    {
        public string SourceSystem { get; set; }
        public string Entity { get; set; }
        public DateTime Watermark { get; set; }
        public DateTime NextWatermark { get; set; }
        public PollingWatermark()
        { }

        public PollingWatermark(PollingWatermarkEntity pollingWatermarkEntity)
        {
            SourceSystem = pollingWatermarkEntity.PartitionKey;
            Entity = pollingWatermarkEntity.RowKey;

            DateTime currentPollingDateTime = DateTime.Now.ToUniversalTime();
            Watermark = pollingWatermarkEntity.Watermark;
            NextWatermark = currentPollingDateTime;
        }
    }
}
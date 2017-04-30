using System;

namespace PacodelaCruz.PollingConsumer.FunctionApp
{
    public class PollingWatermark
    {
        public string SourceSystem { get; set; }
        public string Entity { get; set; }
        public DateTime Watermark { get; set; }
        public DateTime NextWatermark { get; set; }
        public PollingWatermark()
        {

        }

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
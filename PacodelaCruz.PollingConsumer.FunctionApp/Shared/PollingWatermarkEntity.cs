using System;
using Microsoft.WindowsAzure.Storage.Table;

namespace PacodelaCruz.PollingConsumer.FunctionApp
{
    public class PollingWatermarkEntity : TableEntity
    {
        public PollingWatermarkEntity() { }

        public PollingWatermarkEntity(string sourceSystem, string entity)
        {
            this.PartitionKey = sourceSystem;
            this.RowKey = entity;
        }

        public DateTime Watermark { get; set; }
    }
}
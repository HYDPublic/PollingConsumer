using System;
using Microsoft.WindowsAzure.Storage.Table;

namespace PacodelaCruz.PollingConsumer.FunctionApp
{
    /// <summary>
    /// Class to handle the entities on the Azure Table Storage called ‘PollingWatermark'. 
    /// If you want to learn more about using Azure Storage Tables with C# have a look at this documentation
    /// https://docs.microsoft.com/en-us/azure/storage/storage-dotnet-how-to-use-tables
    /// </summary>
    public class PollingWatermarkEntity : TableEntity
    {
        public PollingWatermarkEntity() { }

        /// <summary>
        /// We are storing the SourceSystem in the PartitionKey and the Entity in the RowKey
        /// </summary>
        /// <param name="sourceSystem"></param>
        /// <param name="entity"></param>
        public PollingWatermarkEntity(string sourceSystem, string entity)
        {
            this.PartitionKey = sourceSystem;
            this.RowKey = entity;
        }

        public DateTime Watermark { get; set; }
    }
}
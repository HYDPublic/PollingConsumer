using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PacodelaCruz.PollingConsumer.FunctionApp
{
    public class GetPollingWatermark
    {
        /// <summary>
        /// Http Task to be run as an Azure Function. To be called with the GET method. 
        /// It returns a JSON object containing a DateTime Polling Watermark which is stored on an Azure Storage Table based on the provided 'sourceSystem' and 'entity' query parameters. 
        /// </summary>
        /// <param name="req">HttpRequestMessage using the GET method which should contain 2 query params 'sourceSystem' and 'entity'</param>
        /// <param name="log">Used for Azure Function logging</param>
        /// <returns>JSON object containing the stored DateTime Polling Watermark and the NextWatermark as shown below:
        /// 
        /// { 
        ///     "SourceSystem": "HRSystem",
        ///     "Entity": "EmployeeUpdate",
        ///     "Watermark": "2017-05-01T08:00:00.000Z",
        ///     "NextWatermark": "2017-05-01T08:30:00.000Z"
        /// }
        /// 
        /// </returns>
        public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, TraceWriter log)
        {
            log.Info($"GetPollingWatermark Function is processing a request. RequestUri={req.RequestUri}");

            // parse query parameters
            string sourceSystem = req.GetQueryNameValuePairs()
                .FirstOrDefault(q => string.Compare(q.Key, "sourceSystem", true) == 0)
                .Value;

            string entity = req.GetQueryNameValuePairs()
                .FirstOrDefault(q => string.Compare(q.Key, "entity", true) == 0)
                .Value;

            // If required query parameters were not provided, return BadRequest 
            if (sourceSystem == null || entity == null)
            {
                return req.CreateResponse(HttpStatusCode.BadRequest, "Please provide SourceSystem and Entity parameters in the query string.");
            }

            // Get Connection String
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
    CloudConfigurationManager.GetSetting("PollingWatermarkStorage"));

            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            CloudTable table = tableClient.GetTableReference("PollingWatermark");

            TableOperation retrieveOperation = TableOperation.Retrieve<PollingWatermarkEntity>(sourceSystem, entity);
            TableResult retrievedResult = table.Execute(retrieveOperation);

            // Custom JSON formatter which uses ISO 8601 DateTime format with explicit 3 digit milliseconds and in UTC
            var jsonFormatter = new JsonMediaTypeFormatter();
            jsonFormatter.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
            IsoDateTimeConverter dateConverter = new IsoDateTimeConverter
            {
                DateTimeFormat = "yyyy'-'MM'-'dd'T'HH':'mm':'ss.fff'Z'"
            };
            jsonFormatter.SerializerSettings.Converters.Add(dateConverter);

            if (retrievedResult.Result != null)
            {
                return req.CreateResponse(HttpStatusCode.OK, new PollingWatermark((PollingWatermarkEntity)retrievedResult.Result), jsonFormatter);
            }
            else
            {
                // If Polling Watermark for the provided sourceSystem and entity is not found, return NotFound
                return req.CreateResponse(HttpStatusCode.NotFound, string.Format("Polling watermark not found. SourceSystem: '{0}', Entity: '{1}'", sourceSystem, entity));
            }
        }
    }
}
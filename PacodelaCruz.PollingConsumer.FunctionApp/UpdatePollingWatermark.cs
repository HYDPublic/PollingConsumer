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
using Newtonsoft.Json.Serialization;

namespace PacodelaCruz.PollingConsumer.FunctionApp
{

    /// <summary>
    /// Http Task to be run as an Azure Function. To be called with the PATCH method. 
    /// It updates a DateTime PollingWatermark stored on an Azure Storage Table based on the payload sent as the body in the JSON format as shown below. The JSON payload is the one returned by the GetPollingWatermark function. It uses the 'NextWatermark' property as the new value. 
    ///         
    /// { 
    ///     "SourceSystem": "HRSystem",
    ///     "Entity": "EmployeeUpdate",
    ///     "Watermark": "2017-05-01T08:00:00.000Z",
    ///     "NextWatermark": "2017-05-01T08:30:00.000Z"
    /// }
    /// 
    /// </summary>
    /// <param name="req">HttpRequestMessage using the PATCH method which should contain a body as detailed above</param>
    /// <param name="log">Used for Azure Function logging</param>
    /// <returns> HttpStatusCode </returns>
    public class UpdatePollingWatermark
    {
        public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, TraceWriter log)
        {
            log.Info($"GetPollingWatermark Function is processing a request.");

            try
            {
                // Get request body
                PollingWatermark currentPollingWatermark = await req.Content.ReadAsAsync<PollingWatermark>();

                // If required properties not provided correctly in the JSON body, return BadRequest
                if (currentPollingWatermark.SourceSystem == null || currentPollingWatermark.Entity == null || currentPollingWatermark.NextWatermark == DateTime.MinValue)
                {
                    return req.CreateResponse(HttpStatusCode.BadRequest, "Invalid request body");
                }

                //Retrieve the current PollingWatermark Entity based on the provided SourceSystem and Entity properties
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
        CloudConfigurationManager.GetSetting("PollingWatermarkStorage"));
                CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
                CloudTable table = tableClient.GetTableReference("PollingWatermark");
                TableOperation retrieveOperation = TableOperation.Retrieve<PollingWatermarkEntity>(currentPollingWatermark.SourceSystem, currentPollingWatermark.Entity);
                TableResult retrievedResult = table.Execute(retrieveOperation);
                PollingWatermarkEntity pollingWatermarkToUpdate = (PollingWatermarkEntity)retrievedResult.Result;

                if (pollingWatermarkToUpdate != null)
                {
                    // Update the Entity
                    pollingWatermarkToUpdate.Watermark = currentPollingWatermark.NextWatermark;
                    TableOperation updateOperation = TableOperation.Replace(pollingWatermarkToUpdate);
                    table.Execute(updateOperation);
                    return req.CreateResponse(HttpStatusCode.OK);
                }
                else
                {
                    // If Polling Watermark to update for the provided SourceSystem and Entity is not found, return NotFound
                    return req.CreateResponse(HttpStatusCode.NotFound, string.Format("Polling watermark to update not found. SourceSystem: '{0}', Entity: '{1}'.", currentPollingWatermark.SourceSystem, currentPollingWatermark.Entity));
                }
            }
            catch (Exception ex)
            {
                return req.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }
    }
}
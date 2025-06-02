
using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text.Json;

using Microsoft.Extensions.Logging;

using Keyfactor.Orchestrators.Common.Enums;
using Keyfactor.Orchestrators.Extensions;
using Keyfactor.Logging;

namespace Keyfactor.Extensions.Orchestrator.SampleCustomJobType
{
    public class SampleCustomJob : ICustomJobExtension
    {
        private readonly ILogger _logger;

        public SampleCustomJob()
        {
            _logger = LogHandler.GetClassLogger<SampleCustomJob>();
        }

        public string ExtensionName => "";

        public JobResult ProcessJob(JobConfiguration config, SubmitCustomUpdate submitCustomUpdate)
        {
            _logger.MethodEntry();

            try
            {

                string? crqId = config.JobProperties["crqid"].ToString();
                string? cert = config.JobProperties["cert"].ToString();
                string? pin = config.JobProperties["pin"].ToString();
                string? pk = config.JobProperties["pk"].ToString();
                //*** End Change 2 ***

                if (string.IsNullOrWhiteSpace(crqId))
                    throw new ArgumentException("Falta el parámetro CrqId.");

                // Obtener el token sin usar async
                string token = GetAccessToken();

                var bodyObject = new
                {
                    Id = 4,
                    Metadata = new
                    {
                        Propietario = crqId
                    }
                };

                string jsonBody = JsonSerializer.Serialize(bodyObject);
                _logger.LogInformation($"Payload enviado: {jsonBody}");

                        var response = CallApi(jsonBody, token);
                _logger.LogInformation($"Respuesta de API: {response}");

                return new JobResult
                {
                    Result = OrchestratorJobStatusJobResult.Success
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante la ejecución del Custom Job.");
                return new JobResult
                {
                    Result = OrchestratorJobStatusJobResult.Failure,
                    FailureMessage = ex.Message
                };
            }
            finally
            {
                _logger.MethodExit();
            }
        }

        private string GetAccessToken()
        {
            using var client = new HttpClient();

            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("client_id", "keyfactor-ctti-internet"),
                new KeyValuePair<string, string>("client_secret", ""),
                new KeyValuePair<string, string>("grant_type", "client_credentials")
            });

            try
            {
                var response = client.PostAsync(TokenUrl, content).Result;
                response.EnsureSuccessStatusCode();

                string responseBody = response.Content.ReadAsStringAsync().Result;
                using var jsonDoc = JsonDocument.Parse(responseBody);
                return jsonDoc.RootElement.GetProperty("access_token").GetString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el token");
                throw;
            }
        }

        private string CallApi(string jsonBody, string token)
        {
            using var client = new HttpClient();

            var content = new StringContent(jsonBody, Encoding.UTF8, "application/json-patch+json");
            var request = new HttpRequestMessage(HttpMethod.Put, ApiUrl)
            {
                Content = content
            };

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Headers.Add("x-keyfactor-api-version", "1.0");
            request.Headers.Add("x-keyfactor-requested-with", "APIClient");
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));

            var response = client.SendAsync(request).Result;
            string responseContent = response.Content.ReadAsStringAsync().Result;

            if (!response.IsSuccessStatusCode)
                throw new ApplicationException($"Error en API: {response.StatusCode} - {responseContent}");

            return responseContent;
        }
    }
}

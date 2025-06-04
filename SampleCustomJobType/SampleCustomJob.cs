
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

                string paramString = config.JobProperties["ParamString"].ToString();
                int paramInt = Convert.ToInt32(config.JobProperties["ParamInt"]);
                DateTime paramDate = Convert.ToDateTime(config.JobProperties["ParamDate"]);
                bool paramBool = Convert.ToBoolean(config.JobProperties["ParamBool"]);

                _logger.LogInformation($"***** Job History ID: {config.JobHistoryId} *****");
                _logger.LogInformation($"ParamString: {paramString}");
                _logger.LogInformation($"ParamInt: {paramInt.ToString()}");
                _logger.LogInformation($"ParamDate: {paramDate.ToString()}");
                _logger.LogInformation($"ParamBool: {paramBool.ToString()}");

                submitCustomUpdate.Invoke($"{paramString}, {paramInt.ToString()}, {paramDate.ToString()}, {paramBool.ToString()}");

                return new JobResult
                {
                    Result = OrchestratorJobStatusJobResult.Success
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error: {ex.Message}  Stack Trace: {ex.StackTrace}");
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
    }
}

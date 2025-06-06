
using System;

using Microsoft.Extensions.Logging;

//1 From NuGet Package Keyfactor.Orchestrators.IOrchestratorJobExtensions
using Keyfactor.Orchestrators.Common.Enums;
using Keyfactor.Orchestrators.Extensions;
//2 From NuGet Package Keyfactor.Logging
using Keyfactor.Logging;

namespace Keyfactor.Extensions.Orchestrator.SampleCustomJobType
{
    //3 Must implement Keyfactor.Orchestrator.Extensions.ICustomJobExtension
    public class SampleCustomJob : ICustomJobExtension
    {
        private readonly ILogger _logger;

        public SampleCustomJob()
        {
            _logger = LogHandler.GetClassLogger<SampleCustomJob>();
        }

        //4 Needs to be here, but can be blank
        public string ExtensionName => "";

        //5 Single method from ICustomJobExtension that MUST be implemented
        public JobResult ProcessJob(JobConfiguration config, SubmitCustomUpdate submitCustomUpdate)
        {
            _logger.MethodEntry();

            try
            {
                
                //6 Inputs representing the 4 allowed data types of string, int, DateTime, and bool
                string paramString = config.JobProperties["ParamString"].ToString();
                int paramInt = Convert.ToInt32(config.JobProperties["ParamInt"]);
                DateTime paramDate = Convert.ToDateTime(config.JobProperties["ParamDate"]);
                bool paramBool = Convert.ToBoolean(config.JobProperties["ParamBool"]);

                //7 Log the Job History ID (for use with subsequent API calls if desired), pass back a concatenated string of the input parameters that will be persisted in Command, and report back Success.  This block of code
                // is what will be replaced with your own custom logic.
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
            //8 Error handling that can be replaced by custom logic.  Report back failure.
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

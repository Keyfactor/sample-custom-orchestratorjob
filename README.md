## Overview

The Keyfactor Command platform allows for the creation of custom orchestrator extensions known as Custom Job Types.  These custom extensions allow users to write code that can run on a Keyfactor Universal Orchestrator server and make use of the Keyfactor Command job scheduling architecture without being tied to specific capabilities such as Inventory and Management that are built into "normal" orchestrator extensions that are widely available on the GitHub platform.  Also, Custom Job Types have the advantage of not requiring the creation of a Keyfactor Command Certificate Store Type and one or more Keyfactor Command Certificate Stores.

Custom Job Types have the following advantages:
- Custom job inputs can be configured with the data type options of type string, int, DateTime, and bool.
- The extension can be written without being tied to any particular pre-defined function (inventorying certificates, adding certificates to locations, etc).
- Data can be passed back from the job and persisted in the Keyfactor Command database for use in follow up API endpoint calls.

Typical use cases of a Custom Job Type:
- Follow up processing that needs to be performed based on the successful (or unsuccessful) completion of a separate Orchestrator Extension job.  Perhaps a server needs to be reobooted after a Management-Add job runs to renew a certificate.  If this tasks needs to run from a Universal Orchestrator located behind a client's firewall, a [Keyfactor Command Completion Handler](https://github.com/Keyfactor/keyfactor-sample-jobcompletionhandler) could be set up to schedule a custom job that could perform this task.
- A hosted workflow is run and needs a step to execute a command on a server behind the customer's firewall.  The workflow could then call the API endpoint to schedule a custom job (see step 4 under "Getting Started" below) that would then run on a Universal Orchestrator within the client's firewall.


The Custom Job Type Extension in this repository defines 4 input parameters, one of each allowed type: string, int, DateTime, and bool.  The extension then writes the values out to the orchestrator log and returns a concatenated string value of all 4 inputs.  The returned value is persisted in the Keyfactor Command database and can be accessed via API endpoint (see Step 5 under "Getting Started" below).


## Getting Started

To use this sample you will need:
- An understanding of the Keyfactor Command platform, API endpoints, orchestrators and orchestrator extensions
- An understanding of C# and .NET development
- A working Keyfactor Command instance and administrative access to the server it is running on
- An installed, configured, and approved Universal Orchestrator Framework instance
- Visual Studio (or other development environment that can build a .NET C# assembly)

#### 1 - Create the `Sample Custom JobType` job type used in this example by calling the `[POST] /JobTypes/Custom` endpoint
<pre>
curl --location 'https://<b><i>{BaseURL}</b></i>/Keyfactorapi/JobTypes/Custom' \
--header 'X-Keyfactor-Requested-With: APIClient' \
--header 'x-keyfactor-api-version: 1.0' \
--header 'Content-Type: application/json' \
--header 'Authorization: Basic <b><i>{Base64 encoded credentials}</b></i>' \
--data '{
  "JobTypeName": "SampleCustomJobType",
  "Description": "Sample Custom JobType",
  "JobTypeFields": [
    {
      "Name": "ParamString",
      "Type": 1,
      "DefaultValue": "Hello",
      "Required": true
    },
    {
      "Name": "ParamInt",
      "Type": 2,
      "DefaultValue": 1,
      "Required": true
    }, 
    {
      "Name": "ParamDate",
      "Type": 3,
      "DefaultValue": "2025-01-01",
      "Required": true
    }, 
    {
      "Name": "ParamBool",
      "Type": 4,
      "DefaultValue": true,
      "Required": true
    }
  ]
}'
</pre>

#### 2 - Install the `Sample Custom JobType` job type and register it on Keyfactor Command
Follow these steps to install and register the `Sample Custom Job Type` custom extension:
- Stop the Keyfactor Universal Orchestrator service on the Universal Orchestrator server on which you plan on installing this extension
- Download the source code of this sample custom job type extension, and compile using Microsoft Visual Studio or equivalent.
- Compile the source code
- Copy the binaries created from the compilation into {Keyfactor Unviversal Orchestartor installation path}/extensions/{folder name you create}.
- Restart the Universal Orchestrator service
- In Keyfactor Command, navigate to `Orchestrators => Management`.  You should see the status of the Universal Orchestrator you are attempting to install the extension on changed to "New".  You should also see a new `SampleCustomJobType` capability appear under Capabilities.
- Click on the target orchestrator, and click the `Approve` button.  The orchestrator status should now appear as "Approved".

#### 3 - Retrieve the applicable Agent ID by calling the `[GET] /Agents` endpoint (necessary for step 4)
<pre>
curl --location 'https://<b><i>{BaseURL}</b></i>/Keyfactorapi/Agents' \
--header 'X-Keyfactor-Requested-With: APIClient' \
--header 'x-keyfactor-api-version: 1.0' \
--header 'Authorization: Basic <b><i>{Base64 encoded credentials}</b></i>' \
--data ''
</pre>

In the response, find the agent (orchestrator) you installed the custom job type extension on, and make note of the corresponding `AgentId`.  You will need this for step 4.

#### 4 - Schedule a job for your `Sample Custom JobType` extension
<pre>
curl --location 'https://<b><i>{BaseURL}</b></i>/Keyfactorapi/OrchestratorJobs/Custom' \
--header 'X-Keyfactor-Requested-With: APIClient' \
--header 'x-keyfactor-api-version: 1.0' \
--header 'Content-Type: application/json' \
--header 'Authorization: Basic <b><i>{Base64 encoded credentials}</b></i>' \
--data '{
  "AgentId": "<b><i>{AgentId from step 3}</b></i>",
  "JobTypeName": "SampleCustomJobType",
  "Schedule": {
    "Immediate": true
  },
  "JobFields": {
    "ParamString": "Desired string value",
    "ParamInt": 1,
    "ParamDate": "2025-01-01T08:10:55",
    "ParamBool": true
 }
}'
</pre>

Modify the JobField values above as desired.  These will be the inputs into your scheduled job.  The API call above schedules the job to run immediately, but this can be altered to run at various intervals.  Please reference the Keyfactor Command API documentation for more details.

Once the job is scheduled, you can navigate in Keyfactor Command to `Orchestrators => Jobs` to view the status of the job.  Once it runs, you should be able to see the 4 values you passed in as input appear in the Orchestrator log located on the Keyfactor Universal Orchestrator server at `{UO installation path}/logs/Log.txt`.  Also, the passed in values will be persisted in the Keyfactor Command database, accessible via the API call in step 5.

#### 5 - Retrieve the data passed back from the `Sample Custom JobType` job
<pre>
curl --location 'https://<b><i>{BaseURL}</b></i>/Keyfactorapi/OrchestratorJobs/JobStatus/Data?jobHistoryId=<b><i>{job history id from job run in step 4}</b></i>' \
--header 'X-Keyfactor-Requested-With: APIClient' \
--header 'x-keyfactor-api-version: 1.0' \
--header 'Authorization: Basic <b><i>{Base64 encoded credentials}</b></i>' \
--data ''
</pre>

This call will retrieve the concatenated parameter values passed back from the executed job in step 4 and persisted in the Keyfactor Command database.  Passing contextualized data back from the job to Command can be useful in a case where further processing needs to occur after the job completed based on the results of the job.  A [completion handler](https://github.com/Keyfactor/keyfactor-sample-jobcompletionhandler) would be an example of such a use case.

The `JobHistoryId` needed to execute this API endpoint can be retrieved from the orchestrator log itself.  The `Sample Custom JobType` extension writes the job history id to the orchestrator log prefixed by `***** Job History ID`.  If you search for this string in the log, you will find the relevant numeric `Job History Id`.


## Understanding the Sample

As stated previously, this sample performs the basic task of logging the data passed into it and then sending that data (in concatenated string form) back to Keyfactor Command to be persisted.  It consists of one code file, SampleCustomJob.cs, which is commented with numbered comments.  Those numbers correspond to the notes below.

1. NuGet packages Keyfactor.Orchestrators.IOrchestratorJobExtensions and Keyfactor.Logging are necessary for this sample along with all other Custom Job Type extensions.  The package source for these packages is `https://nuget.pkg.github.com/Keyfactor/index.json` residing on GitHub's package server within the Keyfactor organization.
2. See #1
3. All Custom Job Type extensions will implement the Keyfactor.Orchestrator.Extensions.ICustomJobExtension interface.  This interface contains one method that needs to be implemented - `ProcessJob(JobConfiguration config, SubmitCustomUpdate submitCustomUpdate)`.
4. The ExtensionName property needs to be implemented as well, but the value can be left blank.
5. The ProcessJob method is the one method from ICustomJobExtension that needs to be implemented.  ProcessJob has 2 arguments - JobConfiguration and SubmitCustomUpdate.  JobConfiguration contains all input information about the job, including and most importantly, the 4 input parameter values passed into the job.  SubmitCustomUpdate contains an `Invoke` method that allows a string value to be passed back to Keyfactor Command and persisted.  This method is where the business logic of your custom job type extension will reside.
6. This section retrieves the 4 parameter values from the JobProperties Dictionary passed into the job from the API call in step 4 above.
7. The 4 passed parameter values are written to the orchestrator log found at `{Keyfactor Universal Orchestrator installation folder}/logs/Log.txt`.  Job then reports back "Success" to Keyfactor Command.
8. Error handling logic as well as reporting back "Failure" to Keyfactor Command.
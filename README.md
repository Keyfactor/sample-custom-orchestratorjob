## Overview

The Keyfactor Command platform allows for the creation of custom orchestrator extensions known as Custom Job Types.  These custom extensions allow the user to create code that can run on a Keyfactor Universal Orchestrator server and make use of the Keyfactor Command job scheduling architecture without being tied to specific capabilities such as Inventory and Management that are built into "normal" orchestrator extensions that are widely available on the GitHub platform.  Also, Custom Job Types have the advantage of not requiring the creation of a Keyfactor Command Certificate Store Type and one or more Keyfactor Command Certificate Stores.

Custom Job Types can be created with:
- one to many input parameters of type string, int, DateTime, and bool
- custom processing logic not tied to any particular function
- the ability to pass data back to Keyfactor Command for it to be persisted and accessible via an endpoint call

Typical use cases of a Custom Job Type:
- Follow up processing that needs to be performed based on the successful (or unsuccessful) completion of a separate Orchestrator Extension job.  Perhaps a server needs to be reobooted after a Management-Add job runs to renew a certificate.  If this tasks needs to run from a Universal Orchestratore located behind a firewall, a [Keyfactor Command Completion Handler](https://github.com/Keyfactor/keyfactor-sample-jobcompletionhandler) could be set up to schedule a custom job that could perform this task.
- A hosted workflow is run and needs a step to execute a command on a server behind the customer's firewall.  The workflow could call the API endpoint to schedule (see below) a custom job that would then run on a Universal Orchestrator inside the client's firewall.


The Custom Job Type Extension in this repository defines 4 input parameters, one of each allowed type: string, int, DateTime, and bool.  The extension then writes the values out to the orchestrator log and returns a concatenated string value of all 4 inputs.  The returned value is persisted in the Keyfactor Command database and can be accessed via API endpoint (see below).


## Getting Started

To use this sample you will need:
- An understanding of the Keyfactor Command platform, API endpoints, orchestrators and orchestrator extensions
- An understanding of C# and .NET development
- A working Keyfactor Command instance and administrative access to the server it is running on
- An installed, configured, and approved Universal Orchestrator Framework instance
- Visual Studio (or other development environment that can build a .NET C# assembly)

#### 1 - Create the `Sample Custom Job Type` job type used in this example by calling the `[POST] /JobTypes/Custom endpoint
````
curl --location 'https://{BaseURL}/Keyfactorapi/JobTypes/Custom' \
--header 'X-Keyfactor-Requested-With: APIClient' \
--header 'x-keyfactor-api-version: 1.0' \
--header 'Content-Type: application/json' \
--header 'Authorization: Basic {Base64 encoded credentials}' \
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
````
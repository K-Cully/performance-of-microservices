# Processor

## Description
A Processor is a named entity that is called via the service API (e.g. https://contoso.com:8008/api/ServiceName/ProcessorName) and organises the flow of control for a given service request.
Multiple processors may be defined for a service to facilitate different API requests yielding different service actions.
A processor allows the configuration of simulated latency, response sizes and an ordered list of steps to execute.

A processor will always return one of the following:

| Response Code | Reason        |
| ------------- |:------------- |
| 200           | All steps succeeded or were set to ignore errors in dependencies |
| 400           | A requested configuration does not exist in the service | 
| 418           | A simulated error occurred |
| 500           | Something unexpected occurred or a dependency failed |


## Implementation
Each processor in the service list is referenced by name so these must be unique within a service.
A processor in the list takes the form "ProcessorName": { Configuration Object }

#### errorSize
The size in bytes of an error response.

#### successSize
The size in bytes of a success response.

####

### Setting configuration
```json
"processors": {
                "DoStuff": {
                    "errorSize": 100,
                    "latency": 0,
                    "steps": [ "LoadA", "RequestA" ],
                    "successSize": 20 
                }
            },
```

### Direct service setting configuration
```xml
<Section Name="Processors">
    <Parameter Name="DoStuff" Value="{ errorSize : 100, steps : [ 'LoadA', 'RequestA' ], successSize : 20, latency : 0 }" />
</Section>
```

__See also__<br/>
[Application and service configuration](../ApplicationAndServices.md)
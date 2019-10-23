# Request Processor

## Description
A Request Processor organises the flow of control for a given service request.<br/>
It is a named entity that is called via the service API (e.g. https://contoso.com:8008/api/ServiceName/ProcessorName).<br/>
Multiple request processors may be defined for a service to facilitate different API requests yielding different service actions.<br/>
A processor allows the configuration of simulated latency, response sizes and an ordered list of steps to execute.

### Processor response codes

| Response Code | Reason        |
| ------------- |:------------- |
| 200           | All steps succeeded or were set to ignore errors in dependencies |
| 400           | A requested configuration does not exist in the service | 
| 418           | A simulated error occurred |
| 500           | Something unexpected occurred or a dependency failed |


## Implementation
Each processor in the service list is referenced by name so these must be unique within a service.<br/>
A processor in the list takes the form "ProcessorName": { Configuration Object }

#### type
__RequestProcessor__ (The typename of the processor to create.)

#### value
An object containing required configuration parameters.

##### errorSize
The size in bytes of an error response.

##### successSize
The size in bytes of a success response.

##### latency
Simulated latency in milliseconds to add to all requests.


### Setting configuration
```json
"processors": {
    "DoStuff": {
        "type": "RequestProcessor",
        "value": {
            "errorSize": 100,
            "latency": 0,
            "successSize": 20,
            <common parameters>
        }
    }
},
```

### Direct service setting configuration
```xml
<Section Name="Processors">
    <Parameter Name="DoStuff" Value="{ type : 'RequestProcessor', value : { errorSize : 100, successSize : 20, latency : 0, <common parameters> } }" />
</Section>
```

__See also__<br/>
[Processor configuration](./Processor.md)<br/>
[Step Configuration](../Steps/Step.md)<br/>
[Application and service configuration](../ApplicationAndServices.md)
# Processor

## Description
A Processor is a named entity that is called via the service API (e.g. https://contoso.com:8008/api/ServiceName/ProcessorName) and organises the flow of control for a given service request.
Multiple processors may be defined for a service to facilitate different API requests yielding different service actions.
A processor allows the configuration of simulated latency, response sizes and an ordered list of steps to execute.


## Implementation



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
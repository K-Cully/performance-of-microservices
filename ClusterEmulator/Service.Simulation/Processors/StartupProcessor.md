# Startup Processor

## Description
A Startup Processor enables execution of emulated business actions on service initialization.<br/>
A startup processor may be ran synchronously or asynchronously.


## Implementation
Each processor in the service list is referenced by name so these must be unique within a service.<br/>
A processor in the list takes the form "ProcessorName": { Configuration Object }

#### type
__StartupProcessor__ (The typename of the processor to create.)

#### value
An object containing required configuration parameters.

##### asynchronous
Whether the processor should run asynchronously or not.


### Setting configuration
```json
"processors": {
    "DoStuff": {
        "type": "StartupProcessor",
        "value": {
            "asynchronous": true,
            <common parameters>
        }
    }
},
```

### Direct service setting configuration
```xml
<Section Name="Processors">
    <Parameter Name="DoStuff" Value="{ type : 'StartupProcessor', value : { asynchronous : true, <common parameters> } }" />
</Section>
```

__See also__<br/>
[Processor configuration](./Processor.md)<br/>
[Step Configuration](../Steps/Step.md)<br/>
[Application and service configuration](../ApplicationAndServices.md)
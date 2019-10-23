# Processor

## Description
A processor connects emulation steps to allow representaiton of complex business processes.<br/>
Each type of processor defines its own configuration parameters but steps are common.

### Types of Step
| Type          | Descritpsion  |
| ------------- |:------------- |
| [Startup Processor](./StartupProcessor.md) | Runs on service start |
| [Request Processor](./RequestProcessor.md) | Triggered by a service request |


## Implementation
Each process in the service list is referenced by name so these must be unique within a service.<br/>
A step in the list takes the form "processName": { Configuration Object }

#### type
The typename of the process to create.

#### value
An object containing configuration parameters.

##### steps
A list of step names to execute as part of the processor.<br/>
Steps will be executed in left-to-right order and a single step can be repeated multiple times, if required.


### Setting configuration
```json
"processors": {
    "ProcessorA": { 
        "type": "ProcessorType",
        "value": {
            "steps": [ "LoadA", "RequestA" ],
            <ProcessorType parameters>
        }
    }
},
```

### Direct service setting configuration
```xml
<Section Name="ProcessorA">
    <Parameter Name="StepA" Value="{ type : 'ProcessorType', value : { steps : [ LoadA, RequestA ], <ProcessorType parameters> } }" />
</Section>
```

__See also__<br/>
[Application and service configuration](../ApplicationAndServices.md)
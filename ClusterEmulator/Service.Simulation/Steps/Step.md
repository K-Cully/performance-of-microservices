# Simulation Step

## Description
A simulation step encapsulates configurable execution logic that emulates or simulates some aspect of service business logic to create a realistic performance profile.<br/>
Steps can be chained via a processor or executed in parallel to emulate many service behaviours.<br/>
Each type of step defines its own configuration parameters but some are common.

### Types of Step
| Type          | Descritpsion  |
| ------------- |:------------- |
| [Error Step](./ErrorStep.md) | Generates errors at a set rate |
| [Load Step](./LoadStep.md) | Generates specefic system load |
| [Request Step](./RequestStep.md) | Makes http requests |


## Implementation
Each step in the service list is referenced by name so these must be unique within a service.<br/>
A step in the list takes the form "StepName": { Configuration Object }

#### type
The typename of the step to create.

#### step
An object containing optional common configuration parameters and required type configuration parameters.

##### parallelCount (Optional)
The number of parallel execution of the step logic to run each time the step is started.

##### parallelError (Optional)
Specifies the clause for how errors from parallel executions affect the overall step result.<br>
Valid values are __Any__, __All__ and __None__. (Defaults to Any behaviour if unspecified).

| Clause        | Behaviour     |
| ------------- |:------------- |
| Any           | One or more failures during parallel execution results in step failure |
| All           | All parallel executions must fail to result in step failure | 
| None          | Always returns success. Errors will be ignored |


### Setting configuration
```json
"steps": {
    "StepA": { 
        "type": "StepType",
        "step": {
            "parallelCount": 2,
            "parallelError": "Any",
            <StepType parameters>
        }
    }
},
```

### Direct service setting configuration
```xml
<Section Name="Steps">
    <Parameter Name="StepA" Value="{ type : 'StepType', step : { parallelCount : 2, parallelError : 'Any', <StepType parameters> } }" />
</Section>
```

__See also__<br/>
[Application and service configuration](../ApplicationAndServices.md)
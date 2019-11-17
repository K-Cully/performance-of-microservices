# Delay Step

## Description
A delay step allows waiting a set period of time between processor steps with no operations.<br/>
This can be used to model delays due to an external dependency that is not included.<br/>
The delay time can be configured.

## Implementation
#### type
__DelayStep__ (The typename of the step to create.)

#### value
An object containing required configuration parameters and optionally, common configuration parameters.

##### time
A positive delay time, in seconds


### Setting configuration
```json
"StepA": { 
    "type": "DelayStep",
    "value": {
        "time": 15.5,
        <optional common parameter>
    }
}
```

### Direct service setting configuration
```xml
<Parameter Name="StepA" Value="{ type : 'DelayStep', value : { time: 15.5, <optional common parameters> } }" />
```

__See also__<br/>
[Step configuration](./Step.md)
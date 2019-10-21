# Load Step

## Description
A load step provides a mechanism to generate a specific CPU and memory load for a set period of time.

## Implementation
#### type
__LaodStep__ (The typename of the step to create.)

#### step
An object containing required configuration parameters and optionally, common configuration parameters.

##### time
A non-neagative time value to execute the step for. 

##### percent
The percentage of processor time that should be consumed, as a positive integer.

##### processors (Optional)
A maximum number of processors to load.<br/>
If 0 or more than the number of available processors is specified, the number of processers available are loaded.

##### bytes (Optional)
The amount of memory, in bytes, to consume.

### Setting configuration
```json
"StepA": { 
    "type": "LoadStep",
    "step": {
        "bytes": 0,
        "time" : 7.2,
        "percent" : 20,
        "processors" : 2,
        <optional common parameter>
    }
}
```

### Direct service setting configuration
```xml
<Parameter Name="StepA" Value="{ type : 'LoadStep', step : { bytes: 0, time: 7.2, percent: 20, processors: 2, <optional common parameters> } }" />
```

__See also__<br/>
[Step configuration](./Step.md)
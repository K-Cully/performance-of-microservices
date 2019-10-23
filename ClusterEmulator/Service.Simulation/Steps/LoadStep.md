# Load Step

## Description
A load step provides a mechanism to generate a specific CPU and memory load for a set period of time.

## Implementation
#### type
__LoadStep__ (The typename of the step to create.)

#### value
An object containing required configuration parameters and optionally, common configuration parameters.

##### bytes
The amount of memory, in bytes, to consume.

##### time
The length of time, in seconds, to execute the step for.<br/>
Negative values will result in the step runnign for the lifetime of the executing process.

##### percent
The percentage of processor time that should be consumed, in the range 0 to 100.<br/>

##### processors (Optional)
A maximum number of processors to load.<br/>
If 0 or more than the number of available processors is specified, the number of processers available are loaded.


### Setting configuration
```json
"StepA": { 
    "type": "LoadStep",
    "value": {
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
<Parameter Name="StepA" Value="{ type : 'LoadStep', value : { bytes: 0, time: 7.2, percent: 20, processors: 2, <optional common parameters> } }" />
```

__See also__<br/>
[Step configuration](./Step.md)

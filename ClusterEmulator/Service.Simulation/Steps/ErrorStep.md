# Error Step

## Description
An error step provides a mechanisism to emulate a particular error rate from a service.<br/>
This is done by configuring an error probability.<br/>
Each run of the step then has the configured probability of returning a failure response.

## Implementation
#### type
__ErrorStep__ (The typename of the step to create.)

#### step
An object containing required configuration parameters and optionally, common configuration parameters.

##### probability
A probability in the range 0 (0%) to 1 (100%) of an error being returned by the step.


### Setting configuration
```json
"StepA": { 
    "type": "ErrorStep",
    "step": {
        "probability": 0.5,
        <optional common parameter>
    }
}
```

### Direct service setting configuration
```xml
<Section Name="Steps">
    <Parameter Name="StepA" Value="{ type : 'StepType', step : { probability: 0.5, <optional common parameters> } }" />
  </Section>
```

__See also__<br/>
[Application and service configuration](./Step.md)
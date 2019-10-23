# Circuit Breaker Config

## Description
Circuit breaker config provides options to add a configurable [Circuit Breaker](https://github.com/App-vNext/Polly/wiki/Circuit-Breaker) policy around outbound service requests.<br/>
The break duration and acceptable fault volume are configurable.  

## Implementation
#### type
__CircuitBreakerConfig__ (The typename of the policy configuration.)

#### value
A dictionary of required configuration parameters.

##### duration
The duration, in seconds, that a broken circuit should be left open before checking if it should be closed again.

##### tolerance
The number of failed responses to allow before triggering a break in the circuit.

### Setting configuration
```json
"PolicyA": {
    "type": "CircuitBreakerConfig",
    "value": {
        "duration" : 5.2,
        "tolerance" : 3
    }
}
```

### Direct service setting configuration
```xml
<Parameter Name="PolicyA" Value="{ type : 'CircuitBreakerConfig', value : { duration : 5.2, tolerance : 3 } }" />
```

__See also__<br/>
[Policy configuration](./Policy.md)
# Advanced Circuit Breaker Config

## Description
Advanced circuit breaker config provides options to add a configurable [Advanced Circuit Breaker](https://github.com/App-vNext/Polly/wiki/Advanced-Circuit-Breaker) policy around outbound service requests.<br/>
The break duration, failure rate threshold, sampling timeframe and minimum throughput to trigger a break are all configurable.  

## Implementation
#### type
__AdvancedCircuitBreakerConfig__ (The typename of the policy configuration.)

#### policy
A dictionary of required configuration parameters.

##### breakDuration
The duration, in seconds, that a broken circuit should be left open before checking if it should be closed again.

##### threshold
The failure threshold in range 0 (0%) to 1 (100%) which should trigger a break in the circuit.

##### samplingDuration
The time window, in seconds, that the failure threshold is calculated over.

##### throughput
The minimum number of requests that must pass through the circuit during the sampling period before the error threshold is valid and the circuit is allowed to break.

### Setting configuration
```json
"PolicyA": {
    "type": "AdvancedCircuitBreakerConfig",
    "policy": {
        "breakDuration" : 5.2,
        "threshold" : 0.75,
        "samplingDuration" : 30.5,
        "throughput" : 20
    }
}
```

### Direct service setting configuration
```xml
<Parameter Name="PolicyA" Value="{ type : 'AdvancedCircuitBreakerConfig', policy : { breakDuration : 5.2, threshold : 0.75, samplingDuration : 30, throughput : 20 } }" />
    
```

__See also__<br/>
[Policy configuration](./Policy.md)
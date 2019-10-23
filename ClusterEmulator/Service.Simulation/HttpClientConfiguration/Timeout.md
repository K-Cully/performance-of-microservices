# Timeout Config

## Description
Timeout config provides options to add a configurable [Timeout](https://github.com/App-vNext/Polly/wiki/Timeout) policy around outbound service requests.<br/>
The timeout wait length and how co-operative cancellation is managed are configurable.

## Implementation
#### type
__TimeoutConfig__ (The typename of the policy configuration.)

#### value
A dictionary of required configuration parameters.

##### cancelDelegates
Whether to enact co-operative cancellation with delegates (notify delegates via cancellation token) or to perform a hard timeout without notifying delegates, when the time period is expired.   

##### time
The time in seconds to wait for execution to complete before timeing out the request and returning a fault.

### Setting configuration
```json
"PolicyA": {
    "type": "TimeoutConfig",
    "value": {
        "cancelDelegates" : true,
        "time" : 15.2
    }
}
```

### Direct service setting configuration
```xml
<Parameter Name="PolicyA" Value="{ type : 'TimeoutConfig', value : { cancelDelegates : true, time : 15.2 } }" />
```

__See also__<br/>
[Policy configuration](./Policy.md)
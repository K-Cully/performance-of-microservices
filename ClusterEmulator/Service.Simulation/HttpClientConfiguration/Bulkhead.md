# Bulkhead Config

## Description
Bulkhead config provides options to add a configurable [Bulkhead](https://github.com/App-vNext/Polly/wiki/Bulkhead) policy around outbound service requests.<br/>
The maximum number of concurrent requests through the bulkhead and queuing at the bulkhead are configurable.  

## Implementation
#### type
__BulkheadConfig__ (The typename of the policy configuration.)

#### value
A dictionary of required configuration parameters.

##### bandwidth
The maximum number of concurrent requests allowed through the bulkhead at one time.

##### queueLength (Optional)
The maximum number of requests to allow to queue at the bulkhead when bandwith has been reached.<br/>
If this is not set, no queuing of requests is allowed causing requests exceeding bandwidth to be faulted.


### Setting configuration
```json
"PolicyA": {
    "type": "BulkheadConfig",
    "value": {
        "bandwidth" : 10,
        "queueLength" : 5
    }
}
```

### Direct service setting configuration
```xml
<Parameter Name="PolicyA" Value="{ type : 'BulkheadConfig', value : { bandwidth : 10, queueLength : 5 } }" /> 
```

__See also__<br/>
[Policy configuration](./Policy.md)
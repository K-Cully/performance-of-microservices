# Retry Config

## Description
Retry config provides options to add a configurable [Retry](https://github.com/App-vNext/Polly/wiki/Retry) policy around service requests.<br/>
Retry count, retry delays, and jitter ranges are configurable.<br/>
Configuration options provided allow for various retry behaviours such as; immediate retry n-times, retry forver, wait and retry, retry n-times with exponential backoff and/or to apply jitter to retry wait times.

## Implementation
#### type
__RetryConfig__ (The typename of the policy configuration.)

#### value
A dictionary of required configuration parameters.

##### retries
The number of retry attempts to make.<br/>
Values less than 1 indicate retry forever.

##### delays
An ordered list of delays, measured in seconds, to apply to each retry attempt.<br/>
If the number of retries exceeds the list length, the last value in the list will be used for all retries thereafter.<br/>
A list containing a single value of -1.0 will configure exponential backoff.

##### jitter (Optional)
A positive value, in milliseconds, specifying the maximum jitter to apply to apply to retry delays.

### Setting configuration
```json
"PolicyA": {
    "type": "RetryConfig",
    "value": {
        "retries" : 0,
        "delays" : [ -1.0 ],
        "jitter" : 300
    }
},
"PolicyB": {
    "type": "RetryConfig",
    "value": {
        "retries" : 3,
        "delays" : [ 1.0, 5.0 ]
    }
}
```

### Direct service setting configuration
```xml
<Parameter Name="PolicyA" Value="{ type : 'RetryConfig', value : { retries : 0, delays : [ -1.0 ], jitter : 300 } }" />
<Parameter Name="PolicyB" Value="{ type : 'RetryConfig', value : { retries : 3, delays : [ 1.0, 5.0 ] } }" />
```

__See also__<br/>
[Policy configuration](./Policy.md)
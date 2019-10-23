# Fallback Config

## Description
Fallback config provides options to add a configurable [Fallback](https://github.com/App-vNext/Polly/wiki/Fallback) policy around outbound service requests.<br/>
The only fallback recourse allowed is to respond with a specific message when an error occurs.<br/>
The status code, reason phrase and response content are configurable.

## Implementation
#### type
__FallbackConfig__ (The typename of the policy configuration.)

#### value
A dictionary of required configuration parameters.

##### content (Optional)
The content for the fallback response.

##### reason (Optional)
The reason phrase to set in the fallback response.

##### statusCode
The status code for the fallback response.

### Setting configuration
```json
"PolicyA": {
    "type": "FallbackConfig",
    "value": {
        "statusCode" : 200,
        "reason" : "fallback triggered",
        "content" : "who knows?"
    }
}
```

### Direct service setting configuration
```xml
<Parameter Name="PolicyA" Value="{ type : 'FallbackConfig', value : { statusCode : 200, reason : 'fallback triggered', content : 'who knows?' } }" />
```

__See also__<br/>
[Policy configuration](./Policy.md)
# Policy

## Description
Allows configuration of [Polly](https://github.com/App-vNext/Polly "Polly resillience library Github") policies to wrap calls through HttpClient instances resolved from the default .Net Core HttpClientFactory or created from a custom http client factory which manages socket lifetime independently.<br/>
Each type of policy defines its own configuration parameters.

### Types of Policy
| Type          | Descritpsion  |
| ------------- |:------------- |
| [Advanced Circuit Breaker](./AdvancedCircuitBreaker.md) | Temporarily breaks connectivity based on QoS monitoring |
| [Bulkhead](./Bulkhead.md) | Restricts maximum concurrency |
| [Cache](./Cache.md) | Caches responses for matching requests |
| [Circuit Breaker](./CircuitBreaker.md) | Temporarily breaks connectivity when the last n responses fail |
| [Fallback](./Fallback.md) | Returns a specific response if a dependency fails |
| [Retry](./Retry.md) | Retries a failed request |
| [Timeout](./Timeout.md) | Sets a maximum wait time for a request |


## Implementation
Each policy in the service list is referenced by name so these must be unique within a service.<br/>
A policy in the list takes the form "PolicyName": { Configuration Object }

#### type
The typename of the policy configuration.

#### policy
An object containing type configuration parameters.


### Setting configuration
```json
"policies": {
    "PolicyA": {
        "type": "PolicyType",
        "policy": {
            <PolicyType parameters>
        }
    }
}
```

### Direct service setting configuration
```xml
<Section Name="Policies">
    <Parameter Name="PolicyA" Value="{ type : 'PolicyType', policy : { <PolicyType parameters> } }" />
</Section>
```

__See also__<br/>
[Client configuration](./Client.md)
[Application and service configuration](../ApplicationAndServices.md)
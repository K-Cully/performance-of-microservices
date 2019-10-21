# Request Step

## Description
A request step makes http requests to a sub-path of any URI defined within a configured client, handles responses and manages request lifetime.

## Implementation
#### type
__RequestStep__ (The typename of the step to create.)

##### cacheId (Optional)
An id to use as a base for cache key isolation, if a caching policy is enabled.<br/>
The final cache key will be of the form "cacheId-<randomUniquenessValue>-method", where randomUniquenessValue is based off cacheKeyUniqueness.

##### cacheKeyUniqueness (Optional)
A uniqueness factor to apply to cache key generation.<br/>
A value of 1,000 will result in a key collision ratio of 1 in 1,000.

##### client
The name of the http client configuration to use for requests.

##### method
The http verb/method applicable to the request type.

##### path
The path to append to the client's base address for requests.

##### size
The request size in bytes.<br/>
Only has any effect for request methods that send a request payload.

##### reuseSockets
A value indicating whether the step should try to reuse sockets for multiple requests or dispose of sockets after each request.

##### trueAsync
A value indicating whether requests should be fire-and-forget or their responses should be awaited.

### Setting configuration
```json
"StepA": {
    "type": "RequestStep",
    "step": {
        "cacheId": "ContosoA",
        "cacheKeyUniqueness": 50,
        "client": "ContosoClient",
        "method": "get",
        "path": "/sub/path",
        "size" : 128,
        "reuseSockets": true,
        "trueAsync" : false,
        <optional common parameter>
    }
}
```

### Direct service setting configuration
```xml
<Parameter Name="RequestA" Value="{ type : 'RequestStep', step : { client : 'ContosoClient', size : 128, trueAsync : false, method : 'get', cacheKeyUniqueness : 50, reuseSockets : true, path : '/sub/path', cacheId : 'ContosoA' } }" />
```

__See also__<br/>
[Step configuration](./Step.md)<br/>
[Client configuration](../HttpClientConfiguration/Client.md)
# Client

## Description
A shared configuration for http clients connecting to a specific domain.<br/>
The URI base address, default requeset headers and resilliency policies can all be configured.

## Implementation
Each client in the service list is referenced by name so these must be unique within a service.<br/>
A client in the list takes the form "ClientName": { Configuration Object }

#### baseAddress
The absolute base address of the URI the client will connect to.

#### policies
An ordered list of resilliency policies to apply to requests using the http client.<br/>
Resiliency policies are combined into a [Polly Policy Wrap](https://github.com/App-vNext/Polly/wiki/PolicyWrap "policy wrap Wiki") wrapping async [HttpClient](https://github.com/App-vNext/Polly/wiki/Polly-and-HttpClientFactory#applying-multiple-policies "policy flow chart for http client factory") requests.<br/>
Refer to Polly documentation for details on the most appropriate ordering of policies.

#### headers (Optional)
A list of key-value pairs of headers to apply to all request through the client.


### Setting configuration
```json
"clients": {
    "ContosoClient": {
        "baseAddress": "https://contoso.com",
        "policies": [ "SlidingCache", "ThreeRetries", "ThreeSecondTimeout" ], 
        "headers": { "Accept": "*/*" }
    }
},
```

### Direct service setting configuration
```xml
<Section Name="Clients">
    <Parameter Name="ContosoClient" Value="{ headers : { Accept : '*/*' }, policies : [ 'SlidingCache', 'ThreeRetries', 'ThreeSecondTimeout' ], baseAddress : 'https://contoso.com' }" />
</Section>
```

__See also__<br/>
[Policy configuration](./Policy.md)
[Request Step Configuration](../Steps/RequestStep.md)
[Application and service configuration](../ApplicationAndServices.md)
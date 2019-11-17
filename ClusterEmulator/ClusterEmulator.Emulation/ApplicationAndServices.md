# Application and Services
## Description
The applciation is configured via a JSON file listing any appsettings.json overrides, an optional Application Insights key and a list of services.
Each service specifies a name, the port it should be exposed upon and configration for processers, steps, clients and policies used by the service.

If using the provided generation and deployment scripts, after the initial provisioning of infrastructure has completed, an option will be presented to the user to update this file with a key from the newly provisioned Application Insights resource.
Adding the Application Insights key (GUID) as the "aiKey" value will result in logging for each service beign piped to Application Insights. This is achieved by replacing the instrumentationKey value in the appsettings.json file during service generation. 

Alternatively, the path to a complete replacement appsettings.json file can be provided as the "appsettingsPath" value. This file must contain a complete Serilog configuration (See the [Serilog Github repository](https://github.com/serilog/serilog-settings-configuration "Serilog Settings Configuration") for more information). As this completely replaces the appsettings.json file, it superseeds the "aiKey" value.


## Implementation
#### appsettingsPath (Optional)
An absolute path to an appsettings.json file containing a complete Serilog configuration.
This will replace all service appsettings.json files.

#### aiKey (Optional)
A GUID value for an Application Insights instance to connect the services to.
This is superseeded by appsettingsPath.

#### services
A list of service configurations, containing:
- __port__ : A unique port value
- __processors__ : A list of uniquely named [Processor configurations](./Processors/Processor.md). 
- __steps__ : A list of uniquely named [Step configurations](./Steps/Step.md).
- __clients__ (Optional) : A list of uniquely named [Http Client configurations](./HttpClientConfiguration/Client.md).
- __policies__ (Optional) : A list of uniquely named [Policy configurations](./HttpClientConfiguration/Policy.md)

### Sample JSON
The following JSON defines an application composed of two services.
The genration step will result in two configured services, using the services' default appsettings.json files, without Application Insights configured.

```json
{
    "appsettingsPath": "",
    "aiKey": "",
    "services": {
        "Sample1": {
            "port": 8001,
            "processors": {
                "DoStuff": {
                    "errorSize": 100,
                    "latency": 0,
                    "steps": [ "LoadA", "RequestA" ],
                    "successSize": 20 
                }
            },
            "steps": {
                "LoadA": { 
                    "type": "LoadStep",
                    "step": {
                        "bytes": 100,
                        "time": 0.01,
                        "percent": 1
                    }
                },
                "RequestA": {
                    "type": "RequestStep",
                    "step": {
                        "cacheId": "ContosoA",
                        "cacheKeyUniqueness": 50,
                        "client": "ContosoClient",
                        "method": "get",
                        "path": "/",
                        "size" : 128,
                        "reuseSockets": true,
                        "trueAsync" : false 
                    }
                }
            },
            "clients": {
                "ContosoClient": {
                    "baseAddress": "https://contoso.com",
                    "policies": [ "SlidingCache", "ThreeRetries", "ThreeSecondTimeout" ], 
                    "headers": { "Accept": "*/*" }
                }
            },
            "policies": {
                "ThreeRetries": {
                    "type": "RetryConfig",
                    "policy": {
                        "retries": 3,
                        "delays": [ 1, 5 ]
                    }
                },
                "SlidingCache": {
                    "type": "CacheConfig",
                    "policy": {
                        "time": { "minutes": 3 },
                        "slidingExpiration": true
                    }
                },
                "ThreeSecondTimeout": {
                    "type": "TimeoutConfig",
                    "policy": {
                        "cancelDelegates" : false,
                        "time" : 3.0
                    }
                }
            }
        },
        "Sample2": {
            "port": 8002,
            "processors": {
                "GenerateLoad": {
                    "errorSize": 100,
                    "latency": 0,
                    "steps": [ "CpuLoad" ],
                    "successSize": 20
                }
            },
            "steps": {
                "CpuLoad": {
                    "type": "LoadStep",
                    "step": {
                        "bytes": 512000,
                        "time": 3,
                        "percent" : 90
                    }
                }
            }
        }
    }
}
```



### Resultant Service Setting Files 
The Settings.xml files that are generated for each service.


#### Sample1 Settings.xml
```xml
<?xml version="1.0" encoding="utf-8" ?>
<Settings xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns="http://schemas.microsoft.com/2011/01/fabric">
  <Section Name="Processors">
    <Parameter Name="DoStuff" Value="{ errorSize : 100, steps : [ 'LoadA', 'RequestA' ], successSize : 20, latency : 0 }" />
  </Section>
  <Section Name="Steps">
    <Parameter Name="LoadA" Value="{ type : 'LoadStep', step : { time : 0.01, bytes : 100, percent : 1 } }" />
    <Parameter Name="RequestA" Value="{ type : 'RequestStep', step : { client : 'ContosoClient', size : 128, trueAsync : false, method : 'get', cacheKeyUniqueness : 50, reuseSockets : true, path : '/', cacheId : 'ContosoA' } }" />
  </Section>
  <Section Name="Clients">
    <Parameter Name="ContosoClient" Value="{ headers : { Accept : '*/*' }, policies : [ 'SlidingCache', 'ThreeRetries', 'ThreeSecondTimeout' ], baseAddress : 'https://contoso.com' }" />
  </Section>
  <Section Name="Policies">
    <Parameter Name="SlidingCache" Value="{ policy : { time : { minutes : 3 }, slidingExpiration : true }, type : 'CacheConfig' }" />
    <Parameter Name="ThreeRetries" Value="{ policy : { retries : 3, delays : [ 1, 5 ] }, type : 'RetryConfig' }" />
    <Parameter Name="ThreeSecondTimeout" Value="{ policy : { cancelDelegates : false, time : 3 }, type : 'TimeoutConfig' }" />
  </Section>
</Settings>
```

#### Sample2 Settings.xml
```xml
<?xml version="1.0" encoding="utf-8" ?>
<Settings xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns="http://schemas.microsoft.com/2011/01/fabric">
  <Section Name="Processors">
    <Parameter Name="GenerateLoad" Value="{ errorSize : 100, steps : [ 'CpuLoad' ], successSize : 20, latency : 0 }" />
  </Section>
  <Section Name="Steps">
    <Parameter Name="CpuLoad" Value="{ type : 'LoadStep', step : { time : 3, bytes : 512000, percent : 90 } }" />
  </Section>
  <Section Name="Clients">
    <!--Clients_Placeholder-->
  </Section>
  <Section Name="Policies">
    <!--Policies_Placeholder-->
  </Section>
</Settings>
```
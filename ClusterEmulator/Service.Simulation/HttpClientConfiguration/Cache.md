# Cache Config

## Description
Cache config provides options to add a configurable [Cache](https://github.com/App-vNext/Polly/wiki/Cache) policy around service requests.<br/>
An in-memory cache is used when the cache is initailized.<br/>
Cache invalidation time, wheteher invalidation time is absolute or not and whether the invalidtion time should be reset on every access or not are configurable.

## Implementation
#### type
__CacheConfig__ (The typename of the policy configuration.)

#### time
The cache invalidation time as an object of the form:
```json
{
    "days": 0,
    "hours": 0,
    "minutes": 0,
    "seconds": 0
}
```
Where invalidation time is absolute, this time is added to 00:00 on the day a cache entry is created.<br/>
When the time has already passed on cache emtry creation, a day is added to the absolute time.<br/>
For all other cache expiration types, this is added the time of cache entry creation.<br/>

##### absoluteTime (Optional)
Whether the invalidation time should be taken as an absolute time or not.<br/>
Defaults to false (offset from request/response time) if unspecified.

##### slidingExpiration (Optional)
Whether the invalidation time should get reset every time it is accessed or not.<br/>
Defaults to false (set once and invalidate after invalidation time) if unspecified.

### Setting configuration
```json
"PolicyA": {
    "type": "CacheConfig",
    "policy": {
        "time" : { "minutes" : 3 },
        "absoluteTime" : false,
        "slidingExpiration" : true
    }
}
```

### Direct service setting configuration
```xml
<Parameter Name="PolicyA" Value="{ type : 'CacheConfig', policy : { time : { minutes : 3 }, slidingExpiration : true } }" />
```

__See also__<br/>
[Policy configuration](./Policy.md)
##Dapr pub sub testing...



#### Dapr run

`dapr run --app-id testapp --dapr-http-port 62329 --app-port 7040 --components-path ./dapr/components --config ./dapr/config/config.yaml -- dotnet run -p ./WebApplication/WebApplication.csproj --urls http://localhost:7040`

#### Subscribe endpoint

http://localhost:7040/dapr/subscribe

```json
[
    {
    "topic": "topic1",
        "pubsubName": "pubsub",
        "routes": {
            "default": "default-route",
            "rules": [
                {
                    "match": "event.type == \"event.v1\"",
                    "path": "matched-route"
                }
            ]
        }
    }
]
```

#### Publish message with type event.v1 will be routed to matched route

```text
curl --request POST 'http://localhost:62329/v1.0/publish/pubsub/topic1' \
--header 'Content-Type: application/cloudevents+json' \
--data-raw '{
"type": "event.v1",
"source": "demo",
"data" : {"hello": "world", "foo":"bar"}
}'
```

#### Subscriber
`== APP ==       MATCHED HANDLER INCOMING MESSAGE: {"hello":"world","foo":"bar"}`

#### Publish message with random type will be routed to default route

```text
curl --request POST 'http://localhost:62329/v1.0/publish/pubsub/topic1' \
--header 'Content-Type: application/cloudevents+json' \
--data-raw '{
"type": "xyz",
"source": "demo",
"data" : {"hello": "world", "foo":"bar"}
}'
```

#### Subscriber
`== APP ==       DEFAULT HANDLER INCOMING MESSAGE: {"hello":"world","foo":"bar"}`

## Code change

#### Enable raw payload for at least one endpoint

```cs
endpoints.MapPost("matched-route", RouteMatchedHandler).WithMetadata(
    new TopicAttribute(
        "pubsub", "topic1", true,  "event.type == \"event.v1\"", 1
    )
);
endpoints.MapPost("default-route", RouteDefaultHandler).WithMetadata(
    new TopicAttribute(
        "pubsub", "topic1", true
    )
);
```

#### Failure: Publish message with type event.v1 will be routed to Default route

```text
curl --request POST 'http://localhost:62329/v1.0/publish/pubsub/topic1' \
--header 'Content-Type: application/cloudevents+json' \
--data-raw '{
"type": "event.v1",
"source": "demo",
"data" : {"hello": "world", "foo":"bar"}
}'
```

#### Subscriber
`== APP ==       DEFAULT HANDLER INCOMING MESSAGE: {"data":{"hello":"world","foo":"bar"},"traceid":"00-ebcfdc8cb05606bec3ce580bb42a7c9a-b96d6a31865045c1-01","topic":"topic1","pubsubname":"pubsub","specversion":"1.0","type":"event.v1","source":"demo"}`

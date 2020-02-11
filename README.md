# Metrics as a part of an insfrastucture for .net core 3.1

This project demonstrates a way to provide the metrics for aspnet core 3.1 app as a part of an ifrastructure.

## Project structure

### Metrics Producer

See [src/Metrics](src/Metrics).

It collects the diagnostic events for
 - http requests
 - entity framework core queries
 - masstransit consuming
 - custom activity consuming

and sends it as metrics to statsd.

Configuration:
```
STATSD__PORT: "9125" # statsd port
STATSD__SERVER: "statsd" # statsd server
customtracking_metrics__enabled: "true" # enable custom activity metrics
customtracking_metrics__name: "samplecustomtracking" # custom activity metric name
entityframeworkcore_metrics__enabled: "true" # enable entityframework core metrics
entityframeworkcore_metrics__name: "sampleef" # entityframework core metric name
healthcheck_metrics__enabled: "true" # enable healthchecks metrics
healthcheck_metrics__name: "samplehc" # healthchecks metrics name
http_metrics__enabled: "true" # enable http request metrics
http_metrics__name: "samplehttp" # http request metric name
masstransit_metrics__enabled: "true" # enable masstransit metrics
masstransit_metrics__name: "samplemasstransit" # masstransit metric name
service__name: ""sampleapp" # service name to include into metrics
```

### Extensions for MassTransit

See [Metrics.Extensions.MassTransit](src/Metrics.Extensions.MassTransit).

Usage
```c#
cfg.AddPipeSpecification(new TrackConsumingSpecification<ConsumeContext>());
```

Because the current implementation of masstransit doesn't provide the events about failures
extension to collect this information were introduced.

### Extensions for Custom Actibity Tracking

See [Metrics.Extensions.MassTransit](src/Metrics.Extensions.Tracking).

Usage
```c#
services.AddTransient<ICustomTracker, CustomTracker>();
```

### Base image

As a sample howto inject the metrics project the [src/EmptyConsole](src/EmptyConsole) and `Dockerfile.baseimage` were created.

To create the base image run
```
yarn baseimage
```

as a result the `baseimage:3.1` will be crated.

### Sample

In order to demonstrate how it works the following things were created:
 - [A sample project](sample/SampleApp) is a aspnet core 3.1 app where `Metrics.Extensions.MassTransit` is referencing.
 - `Dockerfile.sampleapp` to build an image on top of `baseimage:3.1`.
 - [docker-compose](sample/docker-compose.yml) to bring it all together.

How to run the sample:
```
# build the sample apps
yarn images
# build prometheus, grafana and ELK
yarn infra
# run the docker compose
cd sample
docker-compose up -d
```

It will create an app running on `http://localhost:5000` with the following endpoints:
 - api/values -> produces one http metrics with success and one entity framework core metric with success
 - api/values/bad -> produces one http metrics with success and one entity framework core metric with an error
 - api/values/publish -> produces one http metrics with success and one masstransit metric with an error
 - api/values/exception -> produces one http metrics with an error
 - api/values/hc -> produces sample healthcheck metrics*
 - api/values/trackok -> tracks an successful activity
 - api/values/trackexception -> tracks an activity with an exception

The metrics can be seen in statsd at `http://localhost:9102/metrics`.

## TBD
 - documentation
 - unit tests

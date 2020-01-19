# Metrics as a part of an insfrastucture for .net core 2.1

This project demonstrates a way to provide the metrics for aspnet core 2.1 app as a part of an ifrastructure.

## Project structure

### Metrics Producer

See [src/Metrics](src/Metrics).

It collects the diagnostic events for
 - http requests
 - entity framework core queries
 - masstransit consuming

and sends it as metrics to statsd.

Configuration:
```
STATSD__PORT: "9125" # statsd port
STATSD__SERVER: "statsd" # statsd server
entityframeworkcore_metrics__enabled: "true" # enable entityframework core metrics
entityframeworkcore_metrics__name: "sampleef" # entityframework core metric name
http_metrics__enabled: "true" # enable http request metrics
http_metrics__name: "samplehttp" # http request metric name
masstransit_metrics__enabled: "true" # enable masstransit metrics
masstransit_metrics__name: "samplemasstransit" # masstransit metric name
```

### Extensions for MassTransit

See [Metrics.Extensions.MassTransit](src/Metrics.Extensions.MassTransit).

Because the current implementation of masstransit doesn't provide the events about failures
extension to collect this information were introduced.

### Base image

As a sample howto inject the metrics project the [src/EmptyConsole](src/EmptyConsole) and `Dockerfile.baseimage` were created.

To create the base image run
```
yarn baseimage
```

as a result the `baseimage:2.1` will be crated.

### Sample

In order to demonstrate how it works the following things were created:
 - [A sample project](sample/SampleApp) is a aspnet core 2.1 app where `Metrics.Extensions.MassTransit` is referencing.
 - `Dockerfile.sampleapp` to build an image on top of `baseimage:2.1`.
 - [docker-compose](sample/docker-compose.yml) to bring it all together.

How to run the sample:
```
# build a base image
yarn baseimage
# build the sample app
yarn sampleappimage
# run the docker compose
cd sample
docker-compose up -d
```

It will create an app running on `http://localhost:5000` with the following endpoints:
 - api/values -> produces one http metrics with success and one entity framework core metric with success
 - api/values/bad -> produces one http metrics with success and one entity framework core metric with an error
 - api/values/publish -> produces one http metrics with success and one masstransit metric with an error
 - api/values/exception -> produces one http metrics with an error

The metrics can be seen in statsd at `http://localhost:9102/metrics`.

## Further steps

 - Create prometheus alers
 - Create grafana dashboards
 - Add healthcheck metrics

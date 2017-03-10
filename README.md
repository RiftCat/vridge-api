# Getting started

## General requirements 
* ZeroMQ (or any lib that communicate with ZMQ sockets)
* JSON serializer
* API-enabled version of RiftCat (use "**api**" update code in RiftCat update settings)

VRidge API is accessible over TCP, abstracted by ZeroMQ sockets. This allows you to use API any any language that you choose. See [ZeroMQ language bindings](http://zeromq.org/bindings:_start) page and get library for language of your choice.

## Requirements for this example

This repository contains example implementation of API client in .NET. You can use it in your .NET projects by simply building and referencing the APIClient project.

C# project requires:

* NetMQ
* Newtonsoft.Json

Both are referenced as NuGet packages in .csproj and will be restored on pre-build.

# API channels

## Control endpoint

This endpoint provides two functions:

- You can find out what API endpoints are available and wether they are currently in use or not.

- You can connect to specific API endpoint by sending a packet with requested endpoint name and your version number. This will open a listener on the server side compatible with your requested version. Response message will contain ip:port endpoint that can be connected. 

See [control channel wiki page](../../wiki/Control channel) for details. 

## Data endpoints

Data channels are used to interact with VRidge and send/receive actual data. Currently we have two endpoints. See pages below for details.

Controller API allows you to send VR motion controller state without writing a full OpenVR driver. This allows a more stable experience since only one driver is loaded in SteamVR. 

See [Controller API](../../wiki/Controller API) for protocol and details.

Head tracking endpoint allows you to control head tracking in a variety of modes. You can use it to provide positional, rotatioanal or combined data. You can also read mobile sensor data and provide an offset. You can also modify phone tracking data in real time before it's used for rendering in VR. 

See [Head Tracking API](../../wiki/Head Tracking API) for protocol and available modes

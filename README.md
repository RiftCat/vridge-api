# Quick description

VRidge API is a way to interact with VRidge in any language of your choice. We use ZeroMQ sockets as a low latency transport. It can work both locally and remotely through network. 

VRidge API allows you to write full or partial head tracking data, read mobile tracking data and set offsets to correct drift or sync it to your coordinate system. You can also send controller states (buttons, triggers, touchpads), and transforms (motion controller orientation and position in 3D space). 

Everything is loaded as single VRidge OpenVR driver. Some of the above things can be done through writing separate OpenVR drivers but VRidge API has benefits of two-way communications with an ability to mix coordinate systems in a more elastic way. It is also more stable than separate OpenVR drivers potentially providing conflicting data.

# Getting started

## General requirements 
* ZeroMQ (or any lib that communicate with ZMQ sockets)
* JSON serializer

VRidge API is accessible over TCP, abstracted by ZeroMQ sockets. This allows you to use API any language that you choose. See [ZeroMQ language bindings](http://zeromq.org/bindings:_start) page and get library for language of your choice.

## Requirements for this example

This repository contains example implementation of API client in .NET. You can use it in your .NET projects by simply building and referencing the APIClient project.

C# projects require:

* NetMQ
* Newtonsoft.Json
* .NET Standard 2.0 compatible project - one of the following
    * .NET Framework 4.6.1 +
    * .NET Core 2.0 +
    * Mono 5.4 +
    * UWP 10.0.16299 +
    * Xamarin
        * iOS 10.14 +
        * Mac 3.8 
        * Android 8.0 +
* MvvmLight (only for WPF example project - /src/examples/VRE.Vridge.API.DesktopClient)

Both are referenced as NuGet packages in .csproj and will be restored on pre-build.

# API usage

## Simple fire-and-forget access
If you don't want to manage connection state, you can use fire-and-forget access layer which manages the connection state for you. See [VridgeRemote](https://github.com/RiftCat/vridge-api/wiki/VridgeRemote-(fire-and-forget-layer)) for more details).

## Advanced usage

### Control endpoint

This endpoint provides two functions:

- You can find out what API endpoints are available and wether they are currently in use or not.

- You can connect to specific API endpoint by sending a packet with requested endpoint name and your version number. This will open a listener on the server side compatible with your requested version. Response message will contain ip:port endpoint that can be connected. 

See [control channel wiki page](https://github.com/RiftCat/vridge-api/wiki/Control-channel) for details. 

### Data endpoints

Data channels are used to interact with VRidge and send/receive actual data. Currently we have two endpoints. See pages below for details.

Controller API allows you to send VR motion controller state without writing a full OpenVR driver. This allows a more stable experience since only one driver is loaded in SteamVR. 

See [Controller API](https://github.com/RiftCat/vridge-api/wiki/Controller-API) for protocol and details.

Head tracking endpoint allows you to control head tracking in a variety of modes. You can use it to provide positional, rotational or combined data. You can also read mobile sensor data and provide an offset. You can also modify phone tracking data in real time before it's used for rendering in VR. 

See [Head Tracking API](https://github.com/RiftCat/vridge-api/wiki/Head-Tracking-API) for protocol and available modes

Broadcast endpoint carries one-way notifications. Currently only haptic pulses are propagated through broadcast channel. It is implemented as ZMQ PUB-SUB sockets.

See [Listening to broadcasts](https://github.com/RiftCat/vridge-api/wiki/Listening-to-haptic-feedback).

## Changelog

### API v3.1 (VRidge 2.3+)

#### Additions
* Added Java API client.
* Added VridgeRemote class as a fire-and-forget access layer to use API without managing connection state.
* Added a way to remap 3DOF controllers into 6DOF controllers, attached to head.
* Added HeadRelation which configures how the pose is affected by recenter calls. Default "Unrelated" should cover most cases.
* Added discovery UDP broadcast which lets you find active VRidge servers on the local network.


#### Changes

* Removed async offset's pitch and roll axes. Old calls will discard pitch and roll data and only use yaw offset. [Discussion](https://github.com/RiftCat/vridge-api/issues/15). 
* When using reprojection-enabled device, rotational head tracking input will be discarded. External rotational data doesn't play well with devices expecting to use late reprojection based on their own sensor data.
* Deprecated OrientationMatrix as pose format. Added separate rotation quaternion and position vector instead. Removes ambiguity of pose matrix format and byte layout. Old format still works.
* C# API client now multi-targets .NET Standard 2.0 and .NET Framework 4.7.






### API v3 (VRidge 2.0+)
* Added velocity and acceleration to controller data packets.
* Migrated protocol to Protobuf serialization scheme to make it easier to write cross-platform code.
* Migrated client library to .NET Standard 2.0 to make the SDK cross-platform.
* Updated WPF example to use v3.
* Fixed a bug that prevented remote connections without the issue #6 workaround.
* Removed v1 and v2 definitions from the project. VRidge runtime still supports projects using those versions but we recommend using v3 for new projects.

### API v2 (VRidge 1.5)

* Added haptic pulses as PUB-SUB sockets.
* Added recenter head tracking call to mirror recenter hotkey function.
* Added ChangeState to flag HMD as inside/outside of tracking range.

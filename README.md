# v2 note
Wikis haven't been updated yet. This is a quick overview of upcoming changes.

Key notes:
* v1 clients remain compatible, new features are available only if you use "2" in ProtocolVersion header during endpoint request.
* Prerelease build of Riftcat+VRidge is required for these features to work. Use "api" as custom update code in Riftcat's update settings.
* These changes will be merged to Beta mid-May and to Stable in 2nd half of May.

Haptic feedback:

* Example proxy class: https://github.com/RiftCat/vridge-api/blob/v2-pre/src/VRE.Vridge.API.Client/Proxy/Broadcasts/BroadcastProxy.cs
* Struct: https://github.com/RiftCat/vridge-api/blob/v2-pre/src/VRE.Vridge.API.Client/Messages/v2/Broadcast/HapticPulse.cs
* Connecting: https://github.com/RiftCat/vridge-api/blob/v2-pre/src/VRE.Vridge.API.Client/Proxy/APIClient.cs#L115

Broadcast channel endpoint is requested in the same way as Head Tracking and Controller endpoints, with that difference that multiple clients can connect to broadcast channel (ZMQ PUB-SUB).

Head tracking - recenter & tracking state toggle:

* Updated proxy: https://github.com/RiftCat/vridge-api/blob/v2-pre/src/VRE.Vridge.API.Client/Proxy/HeadTracking/HeadTrackingProxy.cs
* Updated struct: https://github.com/RiftCat/vridge-api/blob/v2-pre/src/VRE.Vridge.API.Client/Messages/v2/HeadTracking/Requests/HeadTrackingRequest.cs

WPF desktop tester was updated with controls to test above changes:
- Recenter button.
- Last haptic pulse is displayed in controller UI.
- When HMD marker is dragged in/outside UI canvas the HMD switches its tracking states between "in tracking range"/"not currently tracked".



Above note will be removed once wikis are updated and the branch is merged into main one. This should happen mid-May.

# Quick description

VRidge API is a way to interact with VRidge in any language of your choice. We use ZeroMQ sockets as a low latency transport. It can work both locally and remotely through network. 

VRidge API allows you to write full or partial head tracking data, read mobile tracking data and set offsets to correct drift or sync it to your coordinate system. You can also send controller states (buttons, triggers, touchpads), and transforms (motion controller orientation and position in 3D space). 

Everything is loaded as single VRidge OpenVR driver. Some of the above things can be done through writing separate OpenVR drivers but VRidge API has benefits of two-way communications with an ability to mix coordinate systems in a more elastic way. It is also more stable than separate OpenVR drivers potentially providing conflicting data.

# Getting started

## General requirements 
* ZeroMQ (or any lib that communicate with ZMQ sockets)
* JSON serializer
* API-enabled version of RiftCat (use **beta** channel in RiftCat update settings)

VRidge API is accessible over TCP, abstracted by ZeroMQ sockets. This allows you to use API any any language that you choose. See [ZeroMQ language bindings](http://zeromq.org/bindings:_start) page and get library for language of your choice.

## Requirements for this example

This repository contains example implementation of API client in .NET. You can use it in your .NET projects by simply building and referencing the APIClient project.

C# projects require:

* NetMQ
* Newtonsoft.Json
* MvvmLight (for WPF example project - /src/examples/VRE.Vridge.API.DesktopClient)

Both are referenced as NuGet packages in .csproj and will be restored on pre-build.

# API channels

## Control endpoint

This endpoint provides two functions:

- You can find out what API endpoints are available and wether they are currently in use or not.

- You can connect to specific API endpoint by sending a packet with requested endpoint name and your version number. This will open a listener on the server side compatible with your requested version. Response message will contain ip:port endpoint that can be connected. 

See [control channel wiki page](https://github.com/RiftCat/vridge-api/wiki/Control-channel) for details. 

## Data endpoints

Data channels are used to interact with VRidge and send/receive actual data. Currently we have two endpoints. See pages below for details.

Controller API allows you to send VR motion controller state without writing a full OpenVR driver. This allows a more stable experience since only one driver is loaded in SteamVR. 

See [Controller API](https://github.com/RiftCat/vridge-api/wiki/Controller-API) for protocol and details.

Head tracking endpoint allows you to control head tracking in a variety of modes. You can use it to provide positional, rotatioanal or combined data. You can also read mobile sensor data and provide an offset. You can also modify phone tracking data in real time before it's used for rendering in VR. 

See [Head Tracking API](https://github.com/RiftCat/vridge-api/wiki/Head-Tracking-API) for protocol and available modes

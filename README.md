# R.E.P.O. Network Tweaks
Smoother, more accurate, and more reliable networking  
Download via Thunderstore [here](https://thunderstore.io/c/repo/p/BlueAmulet/REPONetworkTweaks/)

Libraries included in this project are stripped of code and used as reference only.

## What?
This replaces PhotonTransformView with a new version, with better interpolation, no jitter, and better handling of network conditions.  
Hermite interpolation is used instead of jittery Lerp smoothing.  
Rigidbodies are forced to no interpolation kinematic bodies to allow for manual interpolation and smooth updates.  
Update rate is dynamically calculated to handle Photon's skipped updates, Photon's actual update rate, and possible packet loss.  
Data can optionally be projected into the future to help combat latency, but too much may cause rubberbanding.

TimeoutDisconnect checks are also removed to prevent random disconnects, the photon server will still notify of timeout itself.

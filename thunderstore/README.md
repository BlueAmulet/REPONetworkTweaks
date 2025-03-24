# R.E.P.O. Network Tweaks
Replaces Photon's position syncing with a better version.  
Also prevents client sided TimeoutDisconnect.  

This mod is intended for use in multiplayer, and does nothing in singleplayer.  

## Features
Compatible with vanilla clients/hosts  
Hermite interpolation rather than Lerp smoothing.  
Adapts to skipped updates, packet loss, and differing update rates.  
No jittery movement.  
Optional projection into the future to combat latency.  

## Installation (manual)
If you are installing this manually, do the following

1. Extract the archive into a folder. **Do not extract into the game folder.**
2. Move the contents of `plugins` folder into `<GameDirectory>\BepInEx\plugins`.
3. Run the game.

## Configuration
**DisableTimeout:** Disables Photon's client sided network timeout from randomly booting you out of the game.  
**Extrapolate:** Extrapolate data for a little bit when missing information.  
**RateSmoothing:** Photon will skip minor updates, this controls how quick the calculated update rate changes.  
**Future:** How much to project data into the future, too little feels laggy but too much will cause rubberbanding.  

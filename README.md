# SwarmUI-SkimmedCFG

A [SwarmUI](https://github.com/mcmonkeyprojects/SwarmUI/) extension that adds parameters for Extraltodeus's [Skimmed_CFG](https://github.com/Extraltodeus/Skimmed_CFG) custom nodes to the generate tab, a powerful anti-burn allowing much higher CFG scales for latent diffusion models. 

![image](https://github.com/user-attachments/assets/05f8e119-6ca4-4589-9968-8d82092d685c)

## Changelog
<details>
  <summary>19 October 2024</summary>

* Initial Release
</details>

## Installation
1. Update SwarmUI first, if you have an old version this extension may not work
2. Shutdown SwarmUI
3. Open a cmd/terminal window in `SwarmUI\src\Extensions`
4. Run `git clone https://github.com/Quaggles/SwarmUI-SkimmedCFG.git`
5. Run `SwarmUI\update-windows.bat` or `SwarmUI\update-linuxmac.sh` to recompile SwarmUI
6. If the `Skimmed_CFG` custom node is not installed in the backend an install button will be shown in the parameter group, install it and follow the prompts

![image](https://github.com/user-attachments/assets/9f11e371-a51d-4fa0-b9eb-d0bfeced9a60)

7. Now the parameters should appear and you are good to go


## Updating
1. Update SwarmUI first, if you have an old version this extension may not work
2. In SwarmUI go to the Server/Extensions tab
3. Click the update button for 'SkimmedCFGExtension'

## Usage

The extension has 6 modes, one for each custom nodes:
* Base (Skimmed CFG)
* Linear Interpolation
* Linear Interpolation Dual Scales
* Replace
* Timed Flip
* Difference CFG

I'd recommend first experimenting by:
* Using the 'Base' Mode
* Setting the `SkimmingCFG` to 3-4 (Higher for more saturation, lower if it's burning too much)
* Setting the SwarmUI `CFG Scale` to something like 7-12, try increasing this while fiddling with `SkimmingCFG` (If you need to go higher than 20 you can use the `[SkimmedCFG] Override Core CFG` parameter)

This should allow you to gain the benefits of high CFG scale without the typical burn in effects. Read through the [SkimmedCFG readme](https://github.com/Extraltodeus/Skimmed_CFG?tab=readme-ov-file#nodes) for examples and the parameter tooltips to understand what they do (Some parameters only work with certain modes are only visible if you enable `Display Advanced Options` in SwarmUI). 

# OCB Better Auto Exposure Mod - 7 Days to Die (V1.0) Addon

A small harmony mod adjusting the default auto exposure.

## How does Auto Exposure work in 7D2D

Auto Exposure has two components to it. One being how the game determines how
much ambient light all objects should receive. Second is the unity Auto-Exposure
postprocessing shader that should adjust your eye sensitivity.

Unfortunately it seems that the postprocessing shader from unity doesn't really
do what one may expect here, as I had a hard time to get it to do anything other
than pushing the exposure to a defined value. In simple terms, it will find the
optimal exposure for every scene, not just adjusting by a few EVs. It literally
acts like Auto Exposure on your camera, which is not what we want here.

## Ambient Light in 7D2D

Ambient light is the light every object receives (minus ambient occlusion), beside
direct lights shining on to it or not. This helps immersion greatly, as this is how
the real world works. Regular games with static (non-destructible) scenes will use
pre-baked ambient-light-map textures, pre-calculated via computing expensive
algorithms when the scene is exported for the final product. It should be obvious
why this can't be done (easily) for 7D2D, as the whole world and POIs can change.

### Ambient Light approximation in 7D2D

To approximate ambient lighting, vanilla uses the block `light` value to determine
if your player is indoor or outdoor. According to that value, it will configure a
static ambient light mode (`AmbientMode.Trilight` to be specific). The final values
depend on the time of day, if indoor or outdoor and taken from the biome spectra.

Unfortunately the block `light` value is not very exact, and TFP decided to
implement the ambient light mode to be either fully inside or outside. So once
you hit that "indoor" threshold, the ambient light will fade to indoor, which
is IMHO the most annoying part of that whole feature, as it really can mean
that one block position inside a POI may be fully indoor, while the block
next to it will be fully outdoor again.

Note that I made another experimental mod some time ago, that deals exactly
with the situation that block `light` values are less than accurate. That
mod tries to add support for doors and hatches, as currently, every door or
hatch, regardless if it is open or closed, acts like light is able to shine
through it. That is also the reason why it may rain indoors, as a closed door
doesn't really influence the `block` light value currently.

https://github.com/OCB7D2D/OcbEnclosedSpace

## What does this mod actually do

First it will change the calculation for the `AmbientMode.Trilight` config.
Instead of using a threshold, all values are interpolated to linearly scale
with the block `light` value. This should already help to reduce the very
abrupt dimming when you would reach the indoor threshold before.

Secondly I adopted the unity Auto-Exposure compute shader to change the
calculation to just allow some eye sensitivity adjustments. E.g. dark
scenes will become a little brighter, but only to a certain extent.

[![GitHub CI Compile Status][4]][3]

## Download and Install

[Download from GitHub releases][2] and extract into your Mods folder!  
Ensure you don't have double nested folders and ModInfo.xml is at right place!

## Changelog

### Version 0.1.1

- Recompile for V1.1 stable (b14)

### Version 0.1.0

- Recompile for V1.0 stable (b333)
- First compatibility with V1.0 (exp)
- Bumped unity version to 2022.3.29f1

### Version 0.0.2

- Fix server issue

### Version 0.0.1

- Initial version

[1]: https://github.com/OCB7D2D/OcbAutoExposure
[2]: https://github.com/OCB7D2D/OcbAutoExposure/releases
[3]: https://github.com/OCB7D2D/OcbAutoExposure/actions/workflows/ci.yml
[4]: https://github.com/OCB7D2D/OcbAutoExposure/actions/workflows/ci.yml/badge.svg

# NoiseKit - Procedural Noise Generation and Baking

Quick procedural generation and baking of tileable noise textures in the Unity Editor, using compute shaders:

<p align="center">
  <img width="100%" src="https://github.com/comettailz/noisekit/blob/main/.github/images/NoiseKit.gif?raw=true" alt="NoiseComposition">
</p>

Choose from available noise types, edit noise properties, visualize in a realtime viewport, perform channel packing and export as textures.

## Requirements
NoiseKit is supported in Unity 2021 LTS and later. Compute shaders support is required.
 
## Setup
1. In the Unity Editor, go to `Window > Package Manager`
2. On the top left on the Package Manager window, click on `+ > Add package from git URL...` 
3. Add the following URL "https://github.com/comettailz/noisekit.git" and click `Add`

Once installed, open the NoiseKit panel from the main toolbar: `Window > NoiseKit > Open`

## Noise Types
The following noise types are currently supported:
- Value
- Perlin
- Cellular 
- Fractal Value
- Fractal Perlin

## Controls
`Noise Selection`: add or remove noise instances, and set the desired noise type.  
`Noise Editor`: control the available noise properties (per instance).  
`Channels`: add or remove noise channels, and perform channel packing.  
`Viewport`: preview the generated noise texture and per-channel masks.  
`Export`: set the exported texture's precision (8/16 bit per channel), resolution and file path.  

<p align="left">
  <img width="40%" src="https://github.com/comettailz/noisekit/blob/main/.github/images/Controls.png?raw=true" alt="NoiseControls">
</p>

## Pending...
- Additional noise types
- 3D noise support

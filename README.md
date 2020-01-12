# VegetationStudioProExtensions

This repository contains custom extensions for Vegetation Studio Pro. Feel free to use in your own projects.

## Requirements

* Vegetation Studio Pro

  https://assetstore.unity.com/packages/tools/terrain/vegetation-studio-pro-131835

* Vegetation Studio Pro Extensions

  https://bit.ly/VegetationStudioProExtensions

## Installation

* Install Vegetation Studio Pro
* Copy the contents of this repository to your Assets folder

## Mask Extensions

### Center / Grow / Shrink Vegetation and Biome Masks

#### Vegetation Mask Area

Adding the ```VegetationMaskAreaExtension.cs``` script to your Vegetation Mask Area will give you options to center the gameobject handle, grow the mask and shrink the mask.

Inspector:

![vegetation-mask-modifier](https://user-images.githubusercontent.com/10963432/53321440-ca526680-38d8-11e9-99ba-62faeb92a5fd.png)

Example:

![vegetation-mask-example](https://user-images.githubusercontent.com/10963432/53322065-8d876f00-38da-11e9-958a-f452a3149cc2.gif)


#### Biome Mask Area

Adding the ```BiomeMaskAreaExtension.cs``` script to Biome Mask Area will give you options to center the gameobject handle, grow the mask and shrink the mask.

Inspector:

![biome-mask-modifier](https://user-images.githubusercontent.com/10963432/53321441-ca526680-38d8-11e9-8a27-918483777045.png)

Example:

![biome-mask-example](https://user-images.githubusercontent.com/10963432/53323770-60898b00-38df-11e9-9842-284e784c54d2.gif)

---

### Mask Creation Menu

Custom menu in hierarchy for the creation of Biome and Vegetation Mask Area gameobjects. These will also add the ```VegetationMaskAreaExtension.cs``` or ```BiomeMaskAreaExtension.cs``` scripts:

![hierarchy-mask-menu](https://user-images.githubusercontent.com/10963432/53320727-c7ef0d00-38d6-11e9-9a72-59baae57face.png)

---

### Additional Functions

Additional functions for Vegetation Mask Area and Biome Mask Area

![more-functions](https://user-images.githubusercontent.com/10963432/72217536-8dfb1980-352f-11ea-8fa1-caaad2d66e1e.png)

* Circle

  Get the bounds of the mask and create a circular shape using the existing nodes

* Hexagon 
 
  Get the bounds of the mask and create a Hexagon

* Convex Hull

  Get all the nodes of the mask and create a Convex Hull using the node positons

* Subdivide

  Adds new nodes between the existing nodes

* Unsubdivide

  Removes every second node

---

### Automatic creation of a Vegetation Mask Line

With the ```VegetationMaskLineExtension.cs``` extension attached to a gameobject you can have a Vegetation Mask Line automatically generated. All you have to do is provide it a container (parent gameobject). The transforms of the children of this container will be used to create a Vegetation Mask Line.

Inspector, empty:

![vegetation-mask-line-empty](https://user-images.githubusercontent.com/10963432/53391878-fb45a080-3997-11e9-8d9e-be9585445eb5.png)

Inspector, container specified:

![vegetation-mask-line-filled](https://user-images.githubusercontent.com/10963432/53391879-fed92780-3997-11e9-812d-34351fe3ed77.png)

Example:

![vegetation-mask-line-example](https://user-images.githubusercontent.com/10963432/53392096-aeae9500-3998-11e9-844d-83ae85d7c232.gif)

## Batch Functions

Custom Functions for use with Vegetation Studio Pro

Rutime Spawn: Enable or Disable "run-time spawn" of all of the Vegetation Items of the selected Biome.

![batch-functions](https://user-images.githubusercontent.com/10963432/72217509-30ff6380-352f-11ea-8235-417559de31a7.png)

## Biome Mask Spawner

Please backup your project if you use Biome Mask Spawner. It is in active development and bound to get non-backwards-compatible changes depending on requirements and feedback. A documentation will follow later, here's a quick overview video of what it can do:

[![Vegetation Studio Pro Extensions for Unity - Biome Mask Spawner](https://img.youtube.com/vi/n7Kzea4EPmg/0.jpg)](https://www.youtube.com/watch?v=n7Kzea4EPmg)

Features

* Create random Biome Masks in various shapes

  * Voronoi
  * Rectangular
  * Hexagon

* Random shape modifier allows to create randomly convex and concave masks
 
* Multi-Tile-Terrain (naturally because Vegetation Studio Pro supports it)

* Shape bounds variation

  The bounds used for the shapes can be of the following:

  * Combined Terrains
  
    Bounds of all terrains of your Vegetation System

  * Individual Terrains

    All terrains of your Vegetation System are processed individually

  * Biome

    Antoher Biome Mask Area defines the bounds in which the shapes are created


More procedural spawners like Lake creation will follow in a future update.


## Credits

Rafael Kuebler

https://github.com/RafaelKuebler/DelaunayVoronoi

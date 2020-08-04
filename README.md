

# Wandering Islands
Procedural generation of floating islands in Unity, adapted from [this paper](https://www.diva-portal.org/smash/get/diva2:830483/FULLTEXT01.pdf)

## Mesh Generation - finished, untextured 

These are all using a linear relation to vary the height map maximum from edge to center; will experiment with different curves later. 
![](https://github.com/clillianhong/wanderingisland/blob/master/Media/untextured_fullmesh_combined.png)

## Mesh Generation - progress pics 

some tragic bugs (see end for more) 
![](https://github.com/clillianhong/wanderingisland/blob/master/Media/mesh_wip_combined.png)

## Populating 2D vertex disk  

The vertex disk is filled radially, where one can vary the number of rings and radial divisions. 
![](https://github.com/clillianhong/wanderingisland/blob/master/Media/filling_combined.png)

## Offsets for island top and bottom halves 

The initial disk is copied twice, with offsets going off in opposite directions to create the top and bottom of the island. These two halves will be stitched together during the mesh generation phase. 
![](https://github.com/clillianhong/wanderingisland/blob/master/Media/vertex_gizmo_combined.png)

## Outline Generation 

The paper used approximations based on hand drawn outlines, but for simplicity I'm just generating a wobbly outline with perlin noise. 
![Outline visualized with gizmos](https://github.com/clillianhong/wanderingisland/blob/master/Media/outline_floating_island.png?raw=true)





## Tragic Bugs 

1. Procedural Rose Generation???
2. I put my eagle in for fun but he looks like the Avengers Helicarrier 
3. "Giant Pita bread fights a mid century modern chandelier", Lillian Hong, 2020, Digital Media 

![](https://github.com/clillianhong/wanderingisland/blob/master/Media/tragic_bugs_2_combined.png)



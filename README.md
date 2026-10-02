## Overview
Unity project (Built-in Render Pipeline) built to explore the different stages of the graphics pipeline in HLSL, with special emphasis on **geometry shaders**, **tessellation** and **compute shaders**. The result is an interactive sunset island with animated seagulls, fishes, ocean and grass.

## Features
**Geometry shaders: billboard entities**
* Seagulls and fish are generated from a point mesh with no connectivity, and a geometry shader expands every point into an animated billboard, so a very large number of entities can be drawn from a single mesh
* Configurable number of entities, random size ranges (keeping the aspect ratio if needed) and a 3D domain in which they are generated and move
* Fish: tail flapping animation, tilt according to vertical movement and distance fog to simulate water opacity
* Seagulls: wing flapping animation with symmetric wings controlled by a single parameter
* Horizontal flip according to the direction of movement relative to the camera, and animation speed proportional to movement speed
* Per-entity data sent from CPU to GPU through a `GraphicsBuffer`
<p align = "center">
  <img width="300" height="300" alt="Fish" src="https://github.com/user-attachments/assets/fcb61dd4-bd13-4314-a1c0-226f0ebe3b9f" />
  <img width="300" height="300" alt="Seagull" src="https://github.com/user-attachments/assets/ebe628f1-60c9-4c3f-b3d0-7e6ae4ff9c8c" />
</p>

**Tessellation: ocean with displacement mapping**
* Displacement map applied in the domain shader to move the vertices of a plane
* Normals rebuilt from a normal map through a TBN matrix, with adjustable strength to blend between geometric and mapped normals
* Blinn-Phong lighting with Fresnel-attenuated specular
* Animated waves with a changing wind direction, simulated by smoothly varying the offset of the displacement map
* Adaptive tessellation based on camera distance, with maximum and minimum factors and near/far distances
<p align = "center">
  <img width="300" height="300" alt="Ocean" src="https://github.com/user-attachments/assets/385f4cb4-2fd3-450d-91f0-43302e9a99e8" />
  <img width="300" height="300" alt="Adaptive tesellation" src="https://github.com/user-attachments/assets/052eb571-c1e7-4118-9e26-d354833224fd" />
</p>

**Geometry and tessellation: procedural grass**
* Geometry shader that grows a blade of grass from every triangle of the surface, with tessellation to control how dense the grass is (uniform factor)
* Curved blades, with configurable curvature and forward lean
* Blades are oriented using the surface's tangent space, so they grow along the surface normal
* Wind driven by a scrolling distortion map with adjustable frequency and strength
* Second pass that renders the surface itself as the ground beneath the grass, so there are no gaps between blades
<p align = "center">
  <img width="531" height="300" alt="Grass" src="https://github.com/user-attachments/assets/d86fbbaf-183f-4a09-ae54-25286b6c2d41" />
</p>

**Compute shaders: entity movement**
* Wander behavior: each entity picks a random destination and speed, travels to it and rests for a random time before starting again, all computed on the GPU
* Flocking behavior: Boids model (cohesion, alignment and separation), plus an extra destination force that makes the flock follow a shared target that changes at random intervals
* Fully GPU-driven positions and velocities, removing the per-entity coroutines of the basic CPU behavior and scaling to much larger groups

**Wave equation: interactive puddle**
* Water surface simulated (fixed timestep) in real time by solving the 2D wave equation on a grid of cells, with the mesh generated procedurally
* Every footstep of the player over the puddle spawns a Gaussian-shaped ripple that propagates, reflects and fades out
* Configurable resolution, propagation speed, damping and ripple size and strength
* Mesh normals recalculated every frame so the lighting follows the ripples
<p align = "center">
  <img width="531" height="300" alt="Puddle" src="https://github.com/user-attachments/assets/e4d33258-af7c-459f-b026-a512fcecf281" />
</p>

**Scene and gameplay**
* First-person movement (WASD and mouse) and interaction with objects (**E**), with an outline shader (backface rendering) highlighting what can be interacted with
* Elevator that takes the player to an underwater dome to watch the fish up close, with skyboxes and sun position changing during the trip
* Ambient and interactive sound effects, including footsteps that change depending on the surface

## Technologies
* Unity (Built-in Render Pipeline)
* C#
* HLSL

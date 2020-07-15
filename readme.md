# CS284 Project Proposal - Cloud Simulation

## [Project Paper](/CloudSim.pdf)

## Profile Video
[![Volumetric Cloud](http://img.youtube.com/vi/QqOo42GHnGk/0.jpg)](http://www.youtube.com/watch?v=QqOo42GHnGk "Volumetric Cloud")

## Project Video
[![Project Video](http://img.youtube.com/vi/4reia5R-hGQ/0.jpg)](http://www.youtube.com/watch?v=4reia5R-hGQ "Project Video")

## Summary
For our final project, we intend to create a cloud simulation that renders realistic looking clouds and experiment with procedural generation of different cloud types and shapes. We will then try animating the clouds and simulate cloud movements for some sample scenes.

## Group Members
* Fangping Shi <shi_fangping@berkeley.edu>
* Gang Yao <gyao96@berkeley.edu>
* Hsin-pei Lee <hsinpei_lee@berkeley.edu>

## Problem Description
Clouds are every where. In almost all the video games and animated films, you can see clouds. Clouds in the world of computer graphics are rendered in different shapes and styles to suit the artistic vibe of the product. We want to explore the realm of cloud simulation and try out techniques of rendering visually stunning clouds. For realistic cloud, the standard is using ray-marching to render volumetric materials. For other genras, we can use mesh subdivision to simulate moving ocean of clouds, or blending simple geometries to form morphing clouds.

## Goals and Deliverables
### Three Genras of Clouds
![](https://i.imgur.com/01MAzsa.png)
There are three types of clouds distinguishable by their art style and technical approach. Volumetric clouds are used by most triple-A games as an industry standard for photo-realism. Geometric clouds are used by animated movie and some games with cute-looking graphic style. Then there is polygon mesh clouds that uses the same technique as ocean wave simulation, famously implemented in the game *Sky: Children of the Light*.

### Geometric Cloud
We plan to implement the geometric cloud in Unity. The expected visual should some how resembles that from Astroneer.
![](https://i.imgur.com/x8fORyg.jpg)
We plan to first use the ray-marching toolkit from unity just to have an experiment of how blending simple geometries can give us shapes that look like clouds. If the toolkit isn't sufficient, then we will dive deep into coding our own ray marching shader in unity to create our own custom looking geometric cloud. We will use constructive solid geometry (CSG) to blend simple shapes. CSG is a method of creating complex geometric shapes from simple ones via boolean operations. The following diagram from WikiPedia shows what’s possible with the technique, and the toolkit implementation from Unity.
![](/Recordings/blend.png)
![](/Recordings/geo_cloud.png)


After that, we will animate the cloud by scripting the movement of geometries that constitutes the cloud. [See Result.](/Recordings/gemetry_cloud_shade.mp4)

### Polygon Cloud
Cloud sea are made of very dense clouds, meaning it behaves very much like oceans. By layering levels of noise onto a 2D mesh plane we can create and tune the shape of the cloud sea. Then we can offset the texture relative to time to generate cloud movement. The best tool for experimenting with different noise layers is Unity's built-in shader graph. For example, the following graph shows the implementation of unity gradient noise. We can tweak and play around with gradient noise or implement our own shader graph components to add more variations. The ultimate goal is is to create artistically looking cloud sea. After messing around with Unity Shader Graph, this is what we got.
![](/Recordings/mountain_cloud.png)

### Volumetric Cloud
Volumetric cloud is computationally expensive. So we decided to do it as a challenge if everything goes on well. The ray-marching skeleton and noise generator implemented in the previous step will be the foundation of building volumetric cloud. A basic ray-march though a homogenous volume density will give us something like a opaque box.
![](/Recordings/homo_box.png)
We will generate a noise texture map just like in the cloud sea implementation. Then we apply this texture to a box game object according to the ground plane. For each of the camera ray intersecting with the box, we march through the intersection taking samples of the cloud density estimation according to the texture map and calculate the light transport along this view ray. The details of the physically-based rendering algorithm is explained in [our paper](/CloudSim.pdf).
![](/Recordings/phaseCloud.png)

### Resources

##### Hardware
Microsoft Surface Book 2 with Nvidia GTX1050 2G running Unity 2019.2

##### Reference
1. [Ray Marching and Signed Distance Functions, *Jamie Wong*](http://jamie-wong.com/2016/07/15/ray-marching-signed-distance-functions/)
2. [Ray Marching for Dummies!, *The Art of Code*](https://www.youtube.com/watch?v=PGtv-dBi2wE)
3. [Unity Shader Graph - Gradient Noise](https://docs.unity3d.com/Packages/com.unity.shadergraph@6.9/manual/Gradient-Noise-Node.html)
4. [Ocean Simulation, *Karman Interactive*](https://labs.karmaninteractive.com/ocean-simulation-pt-1-introduction-df134a47150)
5. [Unity Shader基于视差映射的云海效果, *Zhihu*](https://zhuanlan.zhihu.com/p/83355147)
7. [Physically-Based Rendering - Chapter 11 Volume Scattering](http://www.pbr-book.org/3ed-2018/Volume_Scattering.html)
8. [Coding Adventure: Clouds, *Sebastian Lague*](https://www.youtube.com/watch?v=4QOcCGI6xOU)
9. [The Real-time Volumetric Cloudscapes of Horizon: Zero Dawn](http://killzone.dl.playstation.net/killzone/horizonzerodawn/presentations/Siggraph15_Schneider_Real-Time_Volumetric_Cloudscapes_of_Horizon_Zero_Dawn.pdf)
10. [Real-Time Volumetric Rendering, *Patapom / Bomb*](http://patapom.com/topics/Revision2013/Revision%202013%20-%20Real-time%20Volumetric%20Rendering%20Course%20Notes.pdf)
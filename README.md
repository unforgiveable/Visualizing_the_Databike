# Visualizing the Databike

My Bachelor Thesis project; Open-source mountain bike visualization

The goal of the project was to visualize a recorded bike ride using the Unity engine.
My thesis describes the architecture and inner workings of the project in detail. See the Documentation for additional materials.


### Abstract from the thesis:
The Databike, a mountain bike equipped with a variety of sensors, can capture a range of properties while in use. This information is sufficient to reconstruct its state at any given point in time. A bike ride can be recorded in its entirety by recording these properties multiple times a second. In this thesis an application for interactively replaying a recorded bike ride is developed using the Unity engine. Additionally, the file format for a bike ride, called a timeline, is defined based on the GPX standard. The bike is visualized using an animated 3\,D model of the Databike, which is purpose-made for this application, as well as various interface elements. The most important techniques used in the creation of the model are outlined as a part of this thesis. In order to allow for smooth and optionally slowed-down playback of the recorded data, the properties are interpolated using a cubic interpolation library. Combined with a video player-style interface, the user is able to skip through parts of the ride and review specific sections in detail. The finished application is available for both Windows and Linux thanks to the cross-platform build support of the Unity engine. Furthermore, the entire project is developed with expansion in mind: It contains provisions for adding more bikes, visualization elements, application settings, and different timeline file formats.

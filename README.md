# aynpath-unity

## Overview
This repository contains the **Unity-based Augmented Reality (AR) module** for the Final Year Project (FYP). The Unity module is **integrated with a Flutter application** and is responsiblesolely for rendering AR navigation elements. It is not a standalone application.

## Role of Unity in AynPath
Unity is used as the **AR rendering engine** in AynPath. Its role is limited to:
- Displaying AR navigation content
- Rendering waypoints and navigation paths
- Managing spatial placement of AR objects

All application flow, user interface, and camera handling are managed by Flutter.

## Functionality
The Unity module provides the following functionalities:
- Initialization of AR scenes using AR Foundation
- Placement of waypoints in real-world space
- Visualization of navigation paths using LineRenderer
- Real-time updating of AR content based on navigation state

## Technologies Used
- Unity Engine
- AR Foundation
- ARCore XR Plugin (Android)
- C#
- LineRenderer component

## Project Structure
aynpath-unity/
- Assets/ # Unity scripts, scenes, prefabs, materials
- Packages/ # Unity package dependencies
- ProjectSettings/ # Unity project configuration
- README.md # Documentation

## Flutter Integration
This Unity project is **embedded and launched from a Flutter application**.
Unity does not control the application lifecycle.

Integration characteristics:
- Unity is invoked only when AR navigation is required
- Flutter handles permissions and navigation flow
- Unity focuses exclusively on AR visualization

## Build and Usage
The Unity project is configured for **Android ARCore support** and is built as
part of the Flutter application workflow. It is not distributed as a standalone
APK.

Testing must be performed on a **physical Android device** that supports ARCore.

## Notes
- Emulator testing is not recommended due to limited AR support
- Camera permissions are handled by Flutter

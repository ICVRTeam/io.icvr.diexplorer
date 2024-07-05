# Unity DI Explorer
## Plugin Overview

The plugin offers several functionalities designed to enhance the development process in Unity by providing detailed insights into Zenject container bindings. To utilize the plugin, open the window located at (Custom/DI Explorer) and enter Play Mode. Upon activation, the window will begin gathering information about the current scene. Users can switch between multiple scenes and analyze the collected data to gain a comprehensive understanding of the scene's dependency injection structure.

## Installation
To incorporate the package into your Unity project, follow the [instructions](https://wiki.icvr.io/xwiki/bin/view/General/Unity%20Info/Manuals/Unity%20Packages/Working%20with%20internal%20NPM%20repository/) provided.<br>

# ⚠️ Important
This plugin is compatible with Unity versions starting from 2021.1.<br>
When you first launch the plugin, open the plugin window in editor mode (`Custom/DI Explorer`) to initialize the plugin.<br>

### Features and Capabilities

- **Binding Visualization:** The plugin allows users to view the current bindings present in Zenject containers. This feature helps in understanding how different components are wired together and aids in debugging dependency injection issues.

- **Object Tracking:** The plugin tracks the number of created objects that use these bindings. This functionality is crucial for performance analysis and optimization, as it provides insights into object instantiation patterns within the scene.

- **Real-Time Signal Monitoring:** Users can observe signal calls in real-time. The plugin displays the list of subscribers to each signal, providing a clear view of event-driven interactions within the application.

### Usage Instructions

1. **Open the Plugin Window:** Navigate to (`Custom/DI Explorer`) from the Unity menu to open the plugin window.
2. **Enter Play Mode:** Activate Play Mode in Unity. The plugin window will start collecting information about the current scene.
3. **Scene Switching and Data Analysis:** Switch between different scenes to collect and analyze data. The plugin offers a detailed overview of the bindings and the interactions within each scene.

By using this plugin, developers can gain a deeper understanding of the dependency injection patterns and signal mechanisms in their Unity projects, facilitating better debugging and optimization.


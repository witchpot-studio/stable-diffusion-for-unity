# stable-diffusion-for-unity

Image assets generation using [Stable Diffusion web UI](https://github.com/AUTOMATIC1111/stable-diffusion-webui) within Unity

![](Images/depth2imgsample.png)    

## Features
You can generate image assets on Unity.
- Depth to Image
- Text to Image
- Image to Image

## Recomended Environment
- OS: Windows
- VRAM: 8GB~
- Unity version: Unity2021
    - RenderPipeline: URP

## Installation and Running
Please see [Documents](https://docs.witchpot.com/) for more informations.

1. Download latest unitypackage from [Release page](https://github.com/witchpot-studio/stable-diffusion-for-unity/releases/tag/v1.2.0).    
2. Import the unitypackage to your unity project.    
3. Install stable diffusion webui    
![](Images/preferences_sd4u.png)
    We have options which webui you use for this project
    - Internal StableDiffusion    
    Default. Witchpot's custom webui.     
    Automatically install stable diffusion when you select Internal.
    - External StableDiffusion    
    You can use your own local installed webui.    
    Requirement: Forked webui from AUTOMATIC1111    
    Enable the webui API mode with add below option for the bat file    
        ```
        set COMMANDLINE_ARGS=--api
        ```    
        Please select webui-user.bat path in External bat path.    
4. Start Server from Menu bar > Witchpot > StartServer     
![](Images/startserver.png)
5. (If you use Internal webui) Wait until installation finished on CommandPrompt: local URL shown when the installation finished    
![](Images/setup.png)
6. Confirm 2DStageDemo scene    
    Assets/Plugins/Witchpot/Packages/StableDiffusion/Example/Demo/Scenes/2DStageDemo
7. Generate Depth2Img     
![](Images/depth2img.png)


## Contributing
In Preparation.

## License
Distributed under MIT License.
Also this system installs and uses code from our repository (https://github.com/witchpot-studio/stable-diffusion-webui) which fork from [Stable Diffusion web UI](https://github.com/AUTOMATIC1111/stable-diffusion-webui).
If you modify these repository, please apply the GPL v3 license.



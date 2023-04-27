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
- VRAM: 12GB~
- Unity version: Unity2020~
    - RenderPipeline: URP

## Installation and Running
Please see [Documents](https://docs.witchpot.com/) for more informations.

1. Download latest unitypackage from Release page.    
2. Import the unitypackage to your unity project.
3. Start Server from Menu bar > Witchpot > StartServer     
![](Images/startserver.png)
4. Wait until installation finished on CommandPrompt: local URL shown when the installation finished    
![](Images/setup.png)
5. Confirm 2DStageDemo scene    
    Assets/Plugins/Witchpot/Packages/StableDiffusion/Example/Demo/Scenes/2DStageDemo
6. Generate Depth2Img     
![](Images/depth2img.png)


## Contributing
In Preparation.

## Acknowledgments
This system installs and uses code from our repository (https://github.com/witchpot-studio/stable-diffusion-webui) which fork from [Stable Diffusion web UI](https://github.com/AUTOMATIC1111/stable-diffusion-webui).
If you modify these repository, please apply the GPL v3 license.
stable-diffusion-for-unity connect to stable diffusion webui via API, so stable-diffusion-for-unity is available under the MIT license. 

Also we are using
- lllyasviel/ControlNet : https://github.com/lllyasviel/ControlNet
- runwayml/stable-diffusion-v1-5 : https://huggingface.co/runwayml/stable-diffusion-v1-5




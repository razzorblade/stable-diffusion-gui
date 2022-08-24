# stable-diffusion-gui
Windows UI for Stable Diffusion

![showcase](https://raw.githubusercontent.com/razzorblade/stable-diffusion-gui/main/img/sdgui.gif)

# Installation

## Prerequisites
- StableDiffusion and model weights locally (follow steps on [official github](https://github.com/CompVis/stable-diffusion))
- [Anaconda installation](https://www.anaconda.com/) or miniconda/conda - needed to activate conda environment using Anaconda prompt.
Please, **make sure you can run "python txt2img.py --help"** from the Anaconda prompt and that you have **ldm environment** available (following the official guide in previous point).

## Keep in mind
- When using optimized script, put it into /scripts/ folder of archive downloaded from official Stable Diffusion repository.
- It is possible that optimized script does not support PLMS, disable it if Anaconda prompt closes immediately.
- img2img is now supported, but is an experimental feature.

## Steps
1. Download the pre-release package from [github releases](https://github.com/razzorblade/stable-diffusion-gui/releases/tag/alpha-release)
2. Open StableDiffusionGUI.exe
3. (install NET Framework for desktop applications if prompted)
4. Open File->Preferences and assign Anaconda+Txt2img.py file (Anaconda installation should point to "Anaconda" directory which contains bin,DLLs,condabin etc; txt2img file should be assigned from stable-diffusion-main/scripts)
5. Run some prompts, tweak the values

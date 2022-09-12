# stable-diffusion-gui
Windows UI for Stable Diffusion

![showcase](https://raw.githubusercontent.com/razzorblade/stable-diffusion-gui/main/img/sdgui.gif)

# Archivation

As there are much more advanced stable-diffusion UIs working also without a localhost server, I am archiving this project. In latest changes I have introduced regex reading of SD outputs and progress bar in UI showing actual iteration. The next thing would be to preload the model so it won't take that long to run the generation. If you are interested in how the python was called and how anaconda env was switched, refer to [ExternalProcessRunner.cs](https://github.com/razzorblade/stable-diffusion-gui/blob/main/StableDiffusionGUI/ExternalProcessRunner.cs)

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
1. Download the pre-release package from [github releases](https://github.com/razzorblade/stable-diffusion-gui/releases/tag/alpha-release-v0.2.0)
2. Open StableDiffusionGUI.exe
3. (install NET Framework for desktop applications if prompted)
4. Open File->Preferences and assign Anaconda+Txt2img.py file (Anaconda installation should point to "Anaconda" directory which contains bin,DLLs,condabin etc; txt2img file should be assigned from stable-diffusion-main/scripts)
5. Run some prompts, tweak the values

# Development
To add new funcionalities or edit existing, only thing you need is Visual Studio with WPF (C#). Open the solution and everything should work out of the box. Nuget packages used:
- [MahApps Metro](https://www.nuget.org/packages/MahApps.Metro)
- [Ookii Dialogs](https://www.nuget.org/packages/Ookii.Dialogs.Wpf)

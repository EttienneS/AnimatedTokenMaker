# AnimatedTokenMaker

AnimatedTokenMaker is a WPF application to create animated tokens for virtual tabletops like FoundryVTT and Roll20.  Use video or gif files as input and output a VTT compatible webm with a token style.

AnimatedTokenMaker uses [FFmpeg](https://ffmpeg.org/) to split the source and to create the output file.

Example Tokens:

![Token](https://raw.githubusercontent.com/EttienneS/AnimatedTokenMaker/master/Examples/Blue%20Dragon.gif)
![Token](https://raw.githubusercontent.com/EttienneS/AnimatedTokenMaker/master/Examples/Cleric%20of%20Fire.gif)
![Token](https://raw.githubusercontent.com/EttienneS/AnimatedTokenMaker/master/Examples/Cool%20Magnus.gif)
![Token](https://raw.githubusercontent.com/EttienneS/AnimatedTokenMaker/master/Examples/Lich.gif)
![Token](https://raw.githubusercontent.com/EttienneS/AnimatedTokenMaker/master/Examples/Talfen%20Token.gif)

Note: These examples get stretched a bit in the github preview, they look much nicer in an actual tabletop.

## Requirements

- .net 4.6.1 or higher on the client machine
- FFmpeg.exe in the folder where the AnimatedTokenMaker.exe is

## Installation

Download the [latest release](https://github.com/EttienneS/AnimatedTokenMaker/releases)
Extract the zip file
Download a copy of [FFmpeg.exe](https://ffmpeg.org/) and put it in the same folder
Run AnimatedTokenMaker.exe

## Example usage

See this video for an example of usage https://youtu.be/HUzbAshwydk

## Adding borders

All borders are stored in the Assets/Borders directory.  A border consists of two parts, an overlay image that will be the top most layer and a mask image that tells the program what part of the image to cut out.

When making a border keep the center transparent as the entire border image will be drawn over everything after the layers are combined.

The mask image can be any color as long as the alpha/transparency is 100%, anything that is not 100% will be trimmed out.

Every boder image should have a '_mask' image that matches it or it will not load.

The border and has to be the same size but the mask shape does not have to be anything like the border if you want to get creative.

The root of this project has a BorderPSD folder that contains two photoshop project files for the circle and roundedsquare borders that would serve as a good starting point to make new borders.

If you would like to add your borders to the project please feel free to make them and create a pull request (or mail me) and we can get them included for everyone to use.

## Configuration

Framerate and max clip time can be changed in the AnimatedTokenMaker.exe.config file

## Contributing

Feel free to create a pull request, issue or send a direct mail to [me](https://github.com/EttienneS).


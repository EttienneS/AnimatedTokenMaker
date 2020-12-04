# AnimatedTokenMaker

AnimatedTokenMaker is a WPF application to create animated tokens for virtual tabletops like FoundryVTT and Roll20.  Use video or gif files as input and output a VTT compatible webm with a token style.

AnimatedTokenMaker uses [FFmpeg](https://ffmpeg.org/) to split the source and to create the output file.

Example Tokens:

![Token](https://raw.githubusercontent.com/EttienneS/AnimatedTokenMaker/master/Examples/Blue%20Dragon.gif)
![Token](https://raw.githubusercontent.com/EttienneS/AnimatedTokenMaker/master/Examples/Cleric%20of%20Fire.gif)
![Token](https://raw.githubusercontent.com/EttienneS/AnimatedTokenMaker/master/Examples/Cool%20Magnus.gif)
![Token](https://raw.githubusercontent.com/EttienneS/AnimatedTokenMaker/master/Examples/Lich.gif)
![Token](https://raw.githubusercontent.com/EttienneS/AnimatedTokenMaker/master/Examples/Talfen%20Token.gif)

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

## Configuration

Framerate and max clip time can be changed in the AnimatedTokenMaker.exe.config file

## Upcoming features

- Add more video control, choose start/stop points for the video source
- Add one or more layers of static image on top of the video and border layers, to create a static token with an animated background

## Contributing

Feel free to create a pull request, issue or send a direct mail to [me](https://github.com/EttienneS).


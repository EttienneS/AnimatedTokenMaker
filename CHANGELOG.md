# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [0.2.1]

### Changed

- Remove reload layer button, now reloads on change automatically
- Add layer textboxes to enable fine tuning of layers
- Add layer panning by clicking and dragging the image in the layer preview (still WIP)
- Improved centering on rounded square border

### Added

- Added fire border

## [0.2.0]

### Changed

- Rework borders to use a seperate mask image
- Improved border layering to reduce draw calls
- Move borders to Assets folder

### Added

- Added sci border
- Added gradients to asset folder
- Added button to easily add gradients
- Updated readme with section on how to make borders

## [0.1.2]

### Added

- Added support for .webp files as input
- Added webpmux/dwebp service to handle webp files

### Changed

- Added ServiceManager to support multiple encoder/decoder services
- Changed URL for ffmpeg download

## [0.1.1]

### Fixed

- Issue where clips that are longer than 4 seconds become static

## [0.1.0]

### Changed

- Changed versioning model to allow for minor version bit to be rolled when fixing small issues

### Fixed

- Fixed issue where clips that are less than the default export time (5s) would break


## [0.0.2] - 2020-12-04

### Added 

- Added GNU GPLv3 LICENSE
- Added option to add static layers
- Added layer blending
- Added new preview UI
- Added per layer transparency setting
- Added video layer controls
- Added layer preview dragging

### Changed

- Combine all FFmpeg calls into single wrapper service
- Change preview, load and export calls to run in seperate threads

## [0.0.1] - 2020-11-29

### Added

- Initial implementation
- Border controller to manage the final size and to mask out the token border
- SourceImage to manage the loading of input files (using FFmpeg), also manages offset and scaling
- Exporter to output the final animated token (using FFmpeg)
- TokenMaker that combines the token and border and invokes the Exporter
- Simple WPF UI that can be used to preview the final token

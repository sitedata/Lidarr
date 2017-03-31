# Lidarr

Lidarr is a PVR for Usenet and BitTorrent users. It can monitor multiple RSS feeds for new tracks from your favorite artists and will grab, sort and rename them. It can also be configured to automatically upgrade the quality of files already downloaded when a better quality format becomes available.

## Major Features Include:

* Support for major platforms: Windows, Linux, macOS, Raspberry Pi, etc.
* Automatically detects new tracks.
* Can scan your existing library and download any missing tracks.
* Can watch for better quality of the tracks you already have and do an automatic upgrade.
* Automatic failed download handling will try another release if one fails
* Manual search so you can pick any release or to see why a release was not downloaded automatically
* Fully configurable episode renaming
* Full integration with SABnzbd and NZBGet
* Full integration with Kodi, Plex (notification, library update, metadata)
* Full support for specials and multi-episode releases
* And a beautiful UI

## Feature Requests

[![Feature Requests](http://feathub.com/mattman86/Lidarr?format=svg)](http://feathub.com/mattman86/Lidarr)

## Configuring Development Environment:

### Requirements

* Visual Studio 2015 (https://www.visualstudio.com/vs/)
* [Git](https://git-scm.com/downloads)
* [NodeJS](https://nodejs.org/en/download/)

### Setup

* Make sure all the required software mentioned above are installed.
* Clone the repository into your development machine. [*info*](https://help.github.com/articles/working-with-repositories)
* Grab the submodules `git submodule init && git submodule update`
* Install the required Node Packages `npm install`
* Start gulp to monitor your dev environment for any changes that need post processing using `npm start` command.

*Please note gulp must be running at all times while you are working with Lidarr client source files.*

### Development

* Open `NzbDrone.sln` in Visual Studio
* Make sure `NzbDrone.Console` is set as the startup project

### License

* [GNU GPL v3](http://www.gnu.org/licenses/gpl.html)
* Copyright 2010-2017
# CS2-HideLowerBody
 This plugin allows players to hide their lower body view models.

# Features
 - [x] Hides player lower body view model with !legs/!lowerbody
 - [x] Instantly update.
 - [x] MySQL saving.
 - [ ] Translations. (Waiting for CounterStrikeSharp support)

# Installation

 ### Requirements

  - [Metamod:Source](https://www.sourcemm.net/downloads.php/?branch=master) (Dev Build)
  - [CounterStrikeSharp](https://github.com/roflmuffin/CounterStrikeSharp) ([Version 30](https://github.com/roflmuffin/CounterStrikeSharp/releases/tag/v30) or higher)

  Download the latest release of CS2-HideLowerBody from the [GitHub Release Page](https://github.com/dran1x/CS2-HideLowerBody/releases/latest).

  Extract the contents of the archive into your `counterstrikesharp/plugins` folder.

 ### Build Instructions

  If you want to build CS2-HideLowerBody from the source, follow these instructions:

  ```bash
  git clone https://github.com/dran1x/CS2-HideLowerBody && cd CS2-HideLowerBody

  dotnet publish -f net7.0 -c Release 
  ```

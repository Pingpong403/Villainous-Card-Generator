# ToonVil Card Generator
Clone this repository into the folder of your choice to start using it.

## How to Use Git
Ensure you have Git installed on your system. [Here](https://git-scm.com/install/) is the installation page for Git.

- Open up Terminal and navigate to the folder where you will store this generator (I recommend Desktop or Documents).
  - For faster navigation, simply right-click on the desired folder and choose "Open in Terminal"
- With Git installed, simply type `git clone https://github.com/Pingpong403/Villainous-Card-Generator` and press ENTER.
- Once it's done cloning, this generator is ready to use!
  - To update the generator when a patch is released, use `git pull`
  <br> *NOTE: you will overwrite your work if you pull without first copying your files into a temporary folder outside the project folder. Always make a backup before patching!*

## How to Use this Generator
### If you would like to use this generator for making DisVil customs:
1) Update the files in `-TextFiles\` with your card data.
2) Place layout and card images in `-Layout\` and `-Images\`, respectively.
3) Update the files in `-Settings\`, if desired.
4) Run `Villainous-Card-Generator.exe` to assemble the cards. They will be found in `-Exports\`.
<br><br>
### Otherwise, if you are looking to outfit this generator for another system:
1) Delete all of the images in `assets\` (except for `black_bg.png`) and replace them with the system's custom assets.
2) Do the same for `fonts\`, leaving `roboto.ttf`.
3) Delete all of the images in `-Layout\` and `-Layout\Default\`. Replace them with the system's card elements.
4) Run `Villainous-Card-Generator.exe` to generate sample cards to test element positioning, size, etc.
5) Based on the outputs in `-Exports\`, adjust the values in each `config\` file to match the system's style.

## Disclaimer
I did not create, nor was I involved in the development of, any official Villainous titles. I am not affiliated with Ravensburger North America (RA) in any way. I am not receiving financial compensation from RA or any other third party.

# Installation guide
This software was tested with Unity Editor version 2020.3.26f1 on both Windows 10 and Ubuntu 18.04.

## 1. Download this repository
```
git clone https://github.com/HuchieWuchie/affordanceSynthetic.git
```

## 2. Download the models
In order to use this generator, you must first download the annotated models from the following link: https://drive.google.com/drive/folders/1k9gZXCKIrNwlfj3wfRI7sB72yN8cxc85?usp=sharing

After the download is completed, extract the zip file, and place the *Models* folder within the project's *Assets/Resources* folder.

## 3. Open the project using Unity Hub
It might take some time to import all the models. Some warnings regarding normals will pop up in the Unity console window the first time you import the models, see figure. These should be disregarded and will disappear next time you open the project.

After the project opens, perform the following steps:
  - Load *SampleScene*
    - The file can be found in the *Assets/Scenes* folder
  - Connect the *scenemanger* gameobject with the *scenemanger* script

## 4. Build and run the project
IMPORTANT - do not just press Play button. This will not work. Build and Run the project using Ctrl + B shortcut or by selecting Build and Run from the File drop-down menu

## 5. Optinal - test the bounding 

# Authors
Albert Christensen, Department of Electronic Systems, Aalborg University<br/>
Daniel Lehotsky, Department of Electronic Systems, Aalborg University<br/>
Project supervisor - Dimitris Chrysostomou, Robotics & Automation Group, Department of Materials and Production, Aalborg University

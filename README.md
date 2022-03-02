# Installation guide
This software was tested with Unity Editor version 2020.3.26f1 on both Windows 10 and Ubuntu 18.04.

## 1. Create a new Unity 3D project

## 2. Download this repository to your Unity project
```
cd /PATH/TO/YOUR/UNITY/PROJECT
rm -r Assets
rm -r ProjectSettings
git clone https://github.com/HuchieWuchie/affordanceSynthetic.git
mv affordanceSynthetic/ProjectSettings ProjectSettings
mv affordanceSynthetic/ Assets
```

## 3. Download the models
In order to use this generator, you must first download the annotated models from the following link:
https://drive.google.com/drive/folders/1k9gZXCKIrNwlfj3wfRI7sB72yN8cxc85?usp=sharing

After the download is completed, extract the zip file, and place the "Models" folder within your Assets folder.

## 4. Build and run the project

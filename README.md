<h1 align="center" style="border-bottom: none;">Unity package template📦 </h1>
<h3 align="center">Fully automated version management and package publishing</h3>
<p align="center">
  <a href="https://github.com/semantic-release/semantic-release/actions?query=workflow%3ATest+branch%3Amaster">
    <img alt="Build states" src="https://github.com/semantic-release/semantic-release/workflows/Test/badge.svg">
  </a>
  <a href="https://github.com/semantic-release/semantic-release/actions?query=workflow%3ATest+branch%3Amaster">
    <img alt="semantic-release: angular" src="https://img.shields.io/badge/semantic--release-angular-e10079?logo=semantic-release">
  </a>
  <a href="LICENSE">
    <img alt="License" src="https://img.shields.io/badge/License-MIT-blue.svg">
  </a>
</p>
<p align="center">
  <a href="package.json">
    <img alt="Version" src="https://img.shields.io/github/package-json/v/OpenSourceUnityPackage/PackageTemplate">
  </a>
  <a href="#LastActivity">
    <img alt="LastActivity" src="https://img.shields.io/github/last-commit/OpenSourceUnityPackage/PackageTemplate">
  </a>
</p>

## What is it ?
Package template is a template repository to create quickly package for unity.

Unity package allow you to create independents features and include it like puzzle in your unity project to include its features.

This architecture is based on unity package layout presented [here](https://docs.unity3d.com/Manual/cus-layout.html)

I encourage you to create sample that can be imported independently thanks to unity package manager for more modularity.
You can for example create demo, independent feature in your package theme...
Sample folder contain character '~' that mean that this folder will be ignored by unity.
All sample must be referenced in package.json.

Please don't change the license, and don't forget to update your changelog file and package version in package.json.
For clear information about your package, make a demonstration, add description in GitHub and information in README.

## How to create new package ?
Follow these step to create package based on this template:

1: Click on "Use this template" button to create a new repository based on this template.
![Capture d’écran 2022-03-05 210916](https://user-images.githubusercontent.com/55276408/156898721-99195bf3-02c1-41f5-9bc8-483a9b65c55a.png)

2: In this step, we need add information about package that will be used to generate your package thanks to github action.
- Select the owner (by default you, if you want to create a package for this repository, please select OpenSourceUnityPackage). This repository name will be used to generate the package name.
- Then enter your project name. This name is very important because it will be used to generate the package name inside unity. For example TestPackageUnity will become open-source-unity-package.test-package-unity according to unity convention.
- Step 2, 3 and 4 are optional. You can add description, select public repository and include all branch (to include git flow).
- Now click on "select repository from template" to done this process
![Capture d’écran 2022-03-05 211005](https://user-images.githubusercontent.com/55276408/156898722-cc3bf2aa-b6bd-44a1-9f74-a63d9543d1f1.png)

Well done ! Your package is now available. You can now develop it. Don't forgot to add complete description of it inside your README, unity and github to be community friendly ! May the opensource spirit guide your precious step ;D

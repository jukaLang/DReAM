# Juka - Programming Language 

[![Build Status](https://travis-ci.com/jukaLang/Juka.svg?branch=master)](https://travis-ci.com/jukaLang/Juka)

Juka is currently being built as an isolated worker for Microsoft's Azure Cloud Server. 
The goal of the project is to leverage the power of cloud servers to quickly compile large programs.

Juka can also be used as a desktop application and can compile programs into executables.
It can also compile into a standard .NET .dll which can be used in other projects.


## Running Juka
Juka can be ran on a Microsoft Azure's cloud or using an emulator on a destop pc. Check out https://github.com/jukaLang/juka

## Contributing
- Create a new branch, work on it, and create a pull request once you are done working on the feature.

### Examples
- Provides you examples to get you started on using Juka

### src/DreamAzureFunction
- Azure Function runtime code. Set this as a "startup project".

### src/DreamCompiler
- .NET .dll library that can be used in any C# projects including Xamarin for building iOS/Android Apps, 
.NET Core for building cross platform apps for Mac/OS, Windows Apps, and Windows desktop applications.

### src/DreamUnitTest
- Unit tests

### Visual Studio/Development Requirements
##### Make sure you have .NET installed in Visual Studio 2019

The following packages that are required:

- Microsoft.CodeAnalysis
- MSTest.TestAdapter (latest version downloaded via NuGet)[Unit Test Only]
- MSTest.TestFramework (latest version downloaded via NuGet)[Unit Test Only]


### Running Development version
- Open up Juka.sln
- Right click on solution and click "Restore NuGet Packages"
- (alternatively you can do this via console in src/ folder)
- Run DreamUnitTests using Test->Run->All Tests to make sure all tests are passed.
- Click "Start AzureDreamFunction" button which will run an Azure emulator locally.
- Use Postman to send functions to the Azure server in "body" as raw request.


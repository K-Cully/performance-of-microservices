[![Build Status](https://dev.azure.com/keithcully/performance_of_microservices/_apis/build/status/K-Cully.performance-of-microservices?branchName=master)](https://dev.azure.com/keithcully/performance_of_microservices/_build/latest?definitionId=5&branchName=master)

# performance-of-microservices
This repo contains the Cluster Emulator rapid-prototyping suite for configuring, generating and deploying microservice-based applications.

At the core of Cluster Emulator is a library which facilitates registration and execution of real, emulated, and simulated business logic. This emulation library is utilised by other libraries and tools to enable generation and deployment of a fully operational cluster, based on a simple configuration file.

The suite currently provides utility scripts for integration with Service Fabric but is open to extension with any cluster orchestrator.

This project was developed in part fulfilment of the degree of MSc Advanced Software Engineering, University College Dublin.

## Documentation
[Application and service configuration](./ClusterEmulator/ClusterEmulator.Emulation/ApplicationAndServices.md)

[Viewing and generating diagrams](./plantuml_diagrams/SystemDiagrams.md)

## Building Cluster Emulator
The core libraries are cross platform and can be build using the dotnet CLI.
1. Install the .NET Core 2.2 SDK.
2. Clone the repo.
3. Run "dotnet build" then "dotnet test".

## Service Fabric Debugging
To debug the Service Fabric projects, you must have the Service Fabric SDK to be installed.
See [this documentaiton](https://github.com/uglide/azure-content/blob/master/articles/service-fabric/service-fabric-get-started-with-a-local-cluster.md) on setting up your development environment for Service Fabric.

## Generating a Service Fabric Application Package
To generate a Service Fabric Application, define your configuration as per the "Application and service configuration" documentation above, then:
1. Install the .NET Core 2.2 SDK. 
2. Install and launch PowerShell Core. 
3. Install the Service Fabric PowerShell modules:
> Install-Module -Name Microsoft.ServiceFabric.Powershell.Http -AllowPrerelease -Scope CurrentUser
4. Open a PowerShell Core session in the ClusterGeneration folder of the repository. 
5. Run “.\AppGenerator.ps1 -Name <ApplicationName> -ConfigFile <AbsolutePathToConfigFile>”.
6. Run “.\AppPacker.ps1 -Name <ApplicationName>”.
7. This will generate a folder at <repositoryRoot>\generated\<ApplicationName>\pkg. This folder is a Service Fabric application package which can be deployed to any Service Fabric cluster.

# AzureIotSignalRDemoJun262019

This repo contains the sample code for the Azure IoT Hub, Azure SignalR, Azure Functions and Forge Viewer Plant Dashboard experiment.

# Setting up

After cloning the repo, you will need to setup your own IoT Hub and SignalR services in your Azure tenant. This is pretty simple and there are free tiers available for your use. Check out the slides for more information on this.

## Azure Functions

Once you publish your Azure functions to Azure, you will need to go into the application settings and add in your connection string settings for IoT hub and the SignalR service. If you want to play with the functions locally, you can do that as well, just add the settings to the local.settings file. You will have to create your own as this file is not included in source control because of the secrets.

## Forge App

If you want to play with the Forge Viewer, you will have to register yourself at forge.autodesk.com. For first time users, I think you still get free credits to consume their services. You will need to upload a model and translate it. You will also need to setup a method for authenticating the viewer with the Forge cloud services. In this demo, that is accomplished with another Azure function which you can find in the repo. Again, you will need to add your forge client id and secret to the azure function settings to authenticate.
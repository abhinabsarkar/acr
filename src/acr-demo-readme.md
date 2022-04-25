# ACR HelloWorld
This sample is a Asp.net Core application used to demonstrate Azure Container Registry Geo-Replication feature. The application shows the location of the ACR from where it pulls the image.

This sample is built from https://github.com/Azure-Samples/acr-helloworld.

> The original source code requires to be build into a docker image for which it needs dotnet core. This version is built using Dotnet 6.0 & made available as a docker image available to download from docker hub. No need to build the application locally for the demo.

```bash
# Run the application locally
dotnet run
# The app should be locally available on https://localhost:7121

# Replace the environment variable ACR registry name in the Dockerfile with your registry.
# Do it locally now or ensure environment variable is updated when run as container
ENV DOCKER_REGISTRY <registry-name>.azurecr.io

# Build the docker image locally
docker build . -f Dockerfile -t acr-hello-world
# Run the docker image locally
docker run -d -p 8080:80 acr-hello-world
# Test if the container is running locally
docker ps -a
# The running container should show up on the below url
http://localhost:8080/
# log into the running container 
docker exec -it <container-name> /bin/bash
# Remove the container
docker rm <continer-name> -f

# Push the image to docker hub
docker login
# Tag the local image & map it to the docker repo
docker tag local-image:tagname new-repo:tagname
# e.g. docker tag acr-hello-world:latest abhinabsarkar/acr-hello-world:v1.0.0
# push the tagged image to the docker hub
docker push new-repo:tagname
# e.g. docker push abhinabsarkar/acr-hello-world:v1.0.0
# To tag v1.0.0 as latest
docker tag abhinabsarkar/acr-hello-world:v1.0.0 abhinabsarkar/acr-hello-world:latest
# push the tagged image to the docker hub
docker push abhinabsarkar/acr-hello-world:latest
```
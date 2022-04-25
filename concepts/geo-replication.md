# ACR - Geo-Replication
Pros & Cons of multiple Registries across different locations.

![Alt txt](/images/multiple-registries.png)

Geo-replication enables an Azure container registry to function as a single registry, serving multiple regions with multi-master regional registries.
A geo-replicated registry provides the following benefits:
* Single registry, image, and tag names can be used across multiple regions
* Improve performance and reliability of regional deployments with network-close registry access
* Reduce data transfer costs by pulling image layers from a local, replicated registry in the same or nearby region as your container host
* Single management of a registry across multiple regions
* Registry resilience if a regional outage occurs

Considerations for using a geo-replicated registry
* When you push or pull images from a geo-replicated registry, Azure Traffic Manager in the background sends the request to the registry located in the region that is closest to you in terms of network latency.
* After you push an image or tag update to the closest region, it takes some time for Azure Container Registry to replicate the manifests and layers to the remaining regions you opted into. Larger images take longer to replicate than smaller ones. Images and tags are synchronized across the replication regions with an eventual consistency model.
* To manage workflows that depend on push updates to a geo-replicated registry, we recommend that you configure webhooks to respond to the push events. You can set up regional webhooks within a geo-replicated registry to track push events as they complete across the geo-replicated regions.

![Alt txt](/images/geo-replicated-acr.png)

Considerations for high availability
* For high availability and resiliency, we recommend creating a registry in a region that supports enabling [zone redundancy](https://docs.microsoft.com/en-us/azure/container-registry/zone-redundancy). Enabling zone redundancy in each replica region is also recommended.

## Geo-Replication in action
### 1. Create an ACR
[Source code for ACR-Demo application](/src/acr-demo-readme.md)

```bash
# Create ACR
az acr create -n abhinab1 -g rg-abhi-acr --sku premium --admin-enabled true
# Pull the image from docker hub
docker pull abhinabsarkar/acr-hello-world

# Login to ACR
acrName=abhinab1.azurecr.io
# "az acr login" uses the token created when you executed az login to seamlessly authenticate your session with your registry
# az acr login uses the Docker client to set an Azure Active Directory token in the docker.config file. Once you've logged in this way, your credentials are cached, and subsequent docker commands in your session do not require a username or password.
az acr login --name $acrName 

# Tag the local image & map it to the ACR repo
docker tag local-image:tagname new-repo:tagname
# e.g. docker tag abhinabsarkar/acr-hello-world abhinab1.azurecr.io/acr-hello-world
# Push the docker image to the registry abhinab1.acr.io
docker push abhinab1.azurecr.io/acr-hello-world
```

### 2. Deploy application on CanadaCentral from ACR
The image is now available in ACR. From the ACR, deploy it to a Web App. See figure below:

![Alt Txt](/images/deploy-to-web-app.png)

Deploy the web app to canada central web app

![Alt Txt](/images/wa-acr-cc.png)

```bash
# Update the value of environment variable DOCKER_REGISTRY defined in Dockerfile 
az webapp config appsettings set --resource-group rg-abhi-acr --name wa-acr-cc --settings DOCKER_REGISTRY="abhinab1.azurecr.io"
```

Browse to the url https://wa-acr-cc.azurewebsites.net/ The app should show up stating "Hello World from: CanadaCental." If it doesn't, refresh the app since it takes few seconds for the environment variable to pick up.

![Alt txt](/images/acr-demo-app.png)

### 3. Geo-Replicate the application to East US
From the Azure portal, go to the ACR abhinab1 --> in the Resource menu, go to Replications under Services --> Click Add. Create Replication for East US location.

In few seconds, it will show that the East US replicated location is up & the image will be available. From the 

Deploy the same application to East US from ACR by following the instructions mentioned above in step 2. 

```bash
# Update the value of environment variable DOCKER_REGISTRY defined in Dockerfile 
az webapp config appsettings set --resource-group rg-abhi-acr --name wa-acr-eastus --settings DOCKER_REGISTRY="abhinab1.azurecr.io"
```

The app should look like as shown below: 

![Alt txt](/images/acr-demo-app-eastus.png)

### 4. Understanding Geo-Replication
If you do nslookup ACR "abhinab1.azurecr.io" from a VM in East US, you will find the Traffic Manager endpoint is different from that of in Canada Central. The nslookup has to be done from the VMs in respective locations.

From VM in East US
```cmd
nslookup abhinab1.azurecr.io
Server:  UnKnown
Address:  168.63.129.16

Non-authoritative answer:
Name:    r0330eus-3.eastus.cloudapp.azure.com
Address:  20.42.66.0
Aliases:  abhinab1.azurecr.io
          804e7ffe746c42aba27f19023a280bdc.trafficmanager.net
          eus-3.fe.azcr.io
          eus-3-acr-reg.trafficmanager.net
```
From VM in Canada Central
```bash
nslookup abhinab1.azurecr.io
Server:  mynetwork
Address:  192.168.2.1

Non-authoritative answer:
Name:    r0330cac.canadacentral.cloudapp.azure.com
Address:  52.246.154.144
Aliases:  abhinab1.azurecr.io
          804e7ffe746c42aba27f19023a280bdc.trafficmanager.net
          cac.fe.azcr.io
          cac-acr-reg.trafficmanager.net
```

### 5. Time taken for Geo-Replication between regions
I tested geo-replication & found that it took 34 seconds for a 975 MB image to replicate between Canada Central & SouthEast Asia region. A similar experiment was done & results were reported in a [personal blog](https://arsenvlad.medium.com/experiment-to-better-understand-azure-container-registry-geo-replication-behavior-775efac7a564) by a principal engineer & it is consistent. ACR supports webhook & it can be used to notify/trigger a pipeline to build when a new image version is available.

To test the time taken, publish another version of the image to Canada Central. Run the below script from the VM in East US location. The script lists the image versions from the ACR every 2 seconds.
```bash
# The admin user name & password can be found in ACR "abhinab1" --> in the Resource menu, go to Access Keys under Settings
while true
do
    echo "$(date '+TIME:%H:%M:%S')" $(curl -u <admin-username>:<password> https://abhinab1.azurecr.io/v2/acr-hello-world/tags/list) | tee -a logfile
    sleep 2
done
```
Once the image is pushed to Canada Central, it will take few seconds in East US for the new image version to show up. 

## References
* [Geo-Replication in ACR](https://docs.microsoft.com/en-us/azure/container-registry/container-registry-geo-replication)
* [Geo-Replication Demo - Youtube](https://www.youtube.com/watch?v=NcB1Ji-DciI)
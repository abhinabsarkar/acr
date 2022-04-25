while true
do
    echo "$(date '+TIME:%H:%M:%S')" $(curl -u abhinab1:1N8ZUA13a2S8gHx+QvTkX/x8AWI/kxyi https://abhinab1.azurecr.io/v2/acr-hello-world/tags/list) | tee -a logfile
    sleep 2
done
sudo docker rmi meetapp-api-backend -f
sudo docker rmi meetapp-frontend -f
sudo docker rmi meetapp-notification-service -f
sudo docker container rm meetapp-redis-1
sudo docker container rm meetapp-rabbit-1

sudo docker compose up

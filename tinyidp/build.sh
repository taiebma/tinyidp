docker buildx build -t tinyidp:v1.0.9 --platform linux/amd64 --load .
docker tag tinyidp:v1.0.9 nas2.taieb.fr:5050/tinyidp:v1.0.9
docker push nas2.taieb.fr:5050/tinyidp:v1.0.9
docker compose up

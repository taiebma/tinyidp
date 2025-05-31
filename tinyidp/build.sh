docker buildx build -t tinyidp:v1.0.8 --platform linux/amd64 --load .
docker tag tinyidp:v1.0.8 nas2.taieb.fr:5050/tinyidp:v1.0.8
docker push nas2.taieb.fr:5050/tinyidp:v1.0.8
docker compose up

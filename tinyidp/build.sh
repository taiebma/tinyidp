docker buildx build -t nas2.taieb.fr:5050/tinyidp:v1.0.12 --platform linux/amd64,linux/arm64 --push .
docker tag tinyidp:v1.0.9 nas2.taieb.fr:5050/tinyidp:v1.0.9
docker push nas2.taieb.fr:5050/tinyidp:v1.0.9
docker compose up

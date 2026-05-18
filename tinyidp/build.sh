docker buildx build -t nas2.taieb.fr:5050/tinyidp:v1.1.0 --platform linux/amd64,linux/arm64 --push .
docker tag tinyidp:v1.1.0 nas2.taieb.fr:5050/tinyidp:v1.1.0
docker push nas2.taieb.fr:5050/tinyidp:v1.1.0
docker compose up

uid := `id -u`
gid := `id -g`
dev_compose_file := "docker-compose.dev.yml"

[group('docker')]
build-dev:
    UID="{{ uid }}" GID="{{ gid }}" docker compose -f {{ dev_compose_file }} --env-file .env --env-file .env.dev build

[group('docker')]
run-dev:
    UID="{{ uid }}" GID="{{ gid }}" docker compose -f {{ dev_compose_file }} --env-file .env --env-file .env.dev up -d

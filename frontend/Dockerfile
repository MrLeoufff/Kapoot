# Build
FROM node:22-alpine AS build
WORKDIR /app

COPY package*.json ./
RUN npm ci
COPY . .
RUN npm run build

# Run with nginx (NGINX_CONF=nginx.conf par défaut, nginx.symfony.conf pour la stack Symfony)
FROM nginx:alpine AS final
ARG NGINX_CONF=nginx.conf
COPY --from=build /app/dist /usr/share/nginx/html
COPY nginx.conf nginx.symfony.conf /tmp/
RUN cp /tmp/${NGINX_CONF} /etc/nginx/conf.d/default.conf
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]

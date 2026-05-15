# Stage 1: Build
FROM node:18-alpine AS build
WORKDIR /app
COPY package*.json ./
RUN npm install
COPY . .
RUN npm run build -- --configuration production

# Stage 2: Serve
FROM nginx:alpine
COPY --from=build /app/dist/flow-board-web/browser /usr/share/nginx/html
COPY nginx.conf.template /etc/nginx/templates/default.conf.template
EXPOSE 80

# This extracts Render's exact internal DNS server so Nginx never gets "Connection Refused"
CMD ["/bin/sh", "-c", "export RESOLVER_IP=$(awk '/nameserver/ {print $2}' /etc/resolv.conf | head -n 1); /docker-entrypoint.sh nginx -g 'daemon off;'"]

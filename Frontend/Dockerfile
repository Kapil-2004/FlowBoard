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
# If your project builds to a different directory, update the path above.
# Usually it is /app/dist/[project-name]/browser for Angular 17+
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]

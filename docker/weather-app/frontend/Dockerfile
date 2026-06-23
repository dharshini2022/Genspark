# Stage 1: Build the Angular application
FROM node:20-alpine AS build
WORKDIR /app

# Copy package configuration files and install dependencies
COPY package*.json ./
RUN npm ci

# Copy the rest of the source code and run the build script
COPY . .
RUN npm run build -- --configuration=production

# Stage 2: Serve the application using Nginx
FROM nginx:alpine
COPY nginx.conf /etc/nginx/conf.d/default.conf

# Copy build artifacts from Stage 1 to Nginx public directory
COPY --from=build /app/dist/frontend/browser /usr/share/nginx/html

EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]

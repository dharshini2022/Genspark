# Stage 1: Build the Angular app
FROM node:20 AS build
WORKDIR /app
COPY package*.json ./
RUN npm install
COPY . .
RUN npm run build

# Stage 2: Serve with nginx
FROM nginx:alpine
# to copy the buit angular files to nginx folder
COPY --from=build /app/dist/my-angular-app/browser /usr/share/nginx/html
# configure nginx - to forward the client requests to api (backend)
COPY nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80

#NOTE:
#Dockerfile , nginx.conf and package.json must be placed in same level of directory.

FROM node:22-alpine AS build
WORKDIR /usr/local/app

COPY . /usr/local/app/

# Install global Angular CLI and dependencies
RUN npm install -g @angular/cli && npm install

# Build the app for production
RUN ng build --configuration production

# ----- Stage 2 : Nginx to serve ------
FROM nginx:alpine AS prod

# Remove default nginx static assets
RUN rm -rf /usr/share/nginx/html/*

# Copy Angular dist output to Nginx html folder
COPY --from=build /usr/local/app/dist/client/browser /usr/share/nginx/html

COPY nginx.conf /etc/nginx/conf.d/default.conf

# Expose port 80

EXPOSE 80

ENTRYPOINT ["nginx", "-g", "daemon off;"]

FROM node:22-alpine AS frontend-build
WORKDIR /usr/local/app

COPY . /usr/local/app/

# Install global Angular CLI and dependencies
RUN npm install -g @angular/cli && npm install

EXPOSE 4200

ENTRYPOINT ["npm", "run", "start", "--", "--host", "0.0.0.0"]

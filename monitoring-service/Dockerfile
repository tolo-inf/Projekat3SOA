FROM node
WORKDIR /usr/src/app
COPY ./monitoring-service/package*.json ./
RUN npm install
COPY ./monitoring-service .
EXPOSE 3000
CMD ["node", "index.js"]
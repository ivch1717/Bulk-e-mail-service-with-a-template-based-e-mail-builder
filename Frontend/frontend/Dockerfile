FROM node:20-alpine
WORKDIR /app
COPY package*.json ./
RUN npm install
COPY . .
ENV NODE_OPTIONS=--dns-result-order=ipv4first
EXPOSE 4200
CMD ["npm", "start"]

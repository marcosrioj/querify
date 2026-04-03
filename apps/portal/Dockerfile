FROM node:22-alpine

WORKDIR /app

COPY apps/portal/package.json apps/portal/package-lock.json ./
RUN npm ci --legacy-peer-deps

COPY apps/portal/ ./

EXPOSE 5500

CMD ["npm", "run", "dev", "--", "--strictPort"]

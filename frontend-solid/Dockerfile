FROM node:22
WORKDIR /app
COPY ./frontend-solid/ ./
COPY .env .env
RUN npm i
RUN npm run build
EXPOSE 3000
CMD ["npx", "vite", "preview", "--host", "0.0.0.0", "--port", "3000"]

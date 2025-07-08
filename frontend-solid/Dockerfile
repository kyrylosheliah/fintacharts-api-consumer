FROM node:22
WORKDIR /app
COPY . ./
RUN npm i
EXPOSE 3000
CMD ["npx", "vite", "preview", "--host", "0.0.0.0", "--port", "3000"]
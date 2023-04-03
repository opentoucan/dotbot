FROM mongo:4
COPY mongo-setup.js .
ENTRYPOINT [ "bash", "-c", "mongo mongodb://mongo1:27017 mongo-setup.js"]
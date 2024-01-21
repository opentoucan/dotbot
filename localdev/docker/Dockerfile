FROM docker.io/library/mongo:5.0.23-focal
COPY mongo-setup.js .
ENTRYPOINT [ "bash", "-c", "mongosh mongodb://mongo1:27017 mongo-setup.js"]
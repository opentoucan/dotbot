FROM docker.io/library/mongo:4.4.6
COPY mongo-setup.js .
ENTRYPOINT [ "bash", "-c", "mongosh mongodb://mongo1:27017 mongo-setup.js"]
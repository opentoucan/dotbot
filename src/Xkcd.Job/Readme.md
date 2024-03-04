# Xkcd.Job 

A simple console app to check for the latest XKCD.
This service checks for an existing XKCD in the database, if it doesn't exist or it's out of date then it'll update the entity and send an event onto the message broker.

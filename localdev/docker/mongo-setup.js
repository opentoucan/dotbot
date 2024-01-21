rsconf = {
    _id : "dbrs",
    members: [
        {
            "_id": 0,
            "host": "mongo1:27017",
            "priority": 3
        }
    ]
}

rs.initiate(rsconf);
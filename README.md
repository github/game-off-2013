## The Challenge

We have the entire month of November to create a **web-based** game *loosely* based on the theme "change".

## Developer setup

### Pre-requisites

Install node.js from http://nodejs.org/

```
npm install -g grunt-cli
npm install -g karma
```

### Setup

```
git clone git@github.com:Softwire/game-off-2013.git gameoff
cd gameoff
npm install
```

### Development

Run static analysis (JSLint)
```
grunt
```

Run continuous tests
```
karma start
```

### Launching the game

If you're using WebStorm/IntelliJ, just right-click on index.html and select "Open in browser".

Otherwise, you can just run a simple HTTP server, e.g.
```
npm install -g http-server
http-server src -p 8001
```
Then visit http://localhost:8001
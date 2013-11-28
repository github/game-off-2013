## The Challenge

We have the entire month of November to create a **web-based** game *loosely* based on the theme "change".

## Developer setup

### Pre-requisites

Install node.js from http://nodejs.org/

```
npm install -g grunt-cli
npm install -g karma
npm install -g karma-coverage
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

Run continuous tests (you may need to set the CHROME_BIN environment variable to the location of your Google Chrome executable)
```
karma start
```

Coverage reports should appear under /coverage.

You can also run continuous tests within WebStorm by creating a new Karma run configuration and specifying the project's Karma config file.

### Launching the game

If you're using WebStorm/IntelliJ, just right-click on index.html and select "Open in browser".

Otherwise, you can just run a simple HTTP server, e.g.
```
npm install -g http-server
http-server src -p 8001
```
Then visit http://localhost:8001

const http = require("http");

const host = "localhost";
const port = 8000;

var AnimatedGif2E131 = require("animatedgif2e131");

var fs = require("fs");

var options = {
    port: 5568,
    host: "10.52.6.54",
};

var buf = fs.readFileSync("fire16x16.gif");

var mappingFunction = AnimatedGif2E131.mapping.snake;

var theGif = new AnimatedGif2E131(buf, options, mappingFunction);

var fps = 25;

theGif.startAnimation(fps);
// sleep 1 second
setTimeout(() => {
    theGif.stopAnimation();
    theGif.output.close(() => {
        console.log("done");
    })
}, 1000);

const requestListener = function (req, res) {
    res.writeHead(200);
    res.end("My first server!");
};

const server = http.createServer(requestListener);
server.listen(port, host, () => {
    console.log(`Server is running on http://${host}:${port}`);
});

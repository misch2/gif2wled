const http = require("http");
const AnimatedGif2E131 = require("animatedgif2e131");
const fs = require("fs");

const listenhost = "localhost";
const listenport = 8000;

var currentGif;
var currentTimeout;

function playGifForSpecifiedTime(duration, filename, fps = 25) {
    var options = {
        port: 5568,
        host: "10.52.6.54",
    };

    console.log("Loading gif from " + filename);
    var buf = fs.readFileSync(filename);
    var mappingFunction = AnimatedGif2E131.mapping.snake;

    if (currentTimeout) {
        console.log("Clearing previous timeout");
        clearTimeout(currentTimeout);
    }

    if (currentGif) {
        console.log("Stopping previous gif animation.");
        currentGif.stopAnimation();
        currentGif.output.close(() => {}); // disconnect from the E131 server explicitly
    }
    currentGif = new AnimatedGif2E131(buf, options, mappingFunction);
    currentGif.startAnimation(fps);

    currentTimeout = setTimeout(() => {
        currentGif.stopAnimation();
        currentGif.output.close(() => {}); // disconnect from the E131 server explicitly
    }, duration);
    console.log("Playing gif for " + duration + "ms");
}

const requestListener = function (req, res) {
    res.writeHead(200);
    res.end("My first server!");
};

const server = http.createServer(requestListener);
server.listen(listenport, listenhost, () => {
    console.log(`Server is running on http://${listenhost}:${listenport}`);
});

playGifForSpecifiedTime(5000, "fire16x16.gif");
setTimeout(() => {
    playGifForSpecifiedTime(5000, "test3.gif");
}, 3000);

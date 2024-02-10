const http = require("http");
const AnimatedGif2E131 = require("animatedgif2e131");
const fs = require("fs");

const listenhost = process.env.LISTEN_HOST || "localhost";
const listenport = process.env.LISTEN_PORT || 8000;
const wled_host = process.env.WLED_HOST;

var currentGif;
var currentTimeout;

function playGifForSpecifiedTime(duration, filename, fps = 25) {
    var options = {
        port: 5568,
        host: wled_host,
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
        try {
            currentGif.output.close(() => {}); // disconnect from the E131 server explicitly
        } catch (e) {
            console.log("Error disconnecting from E131 server: " + e);
        }
    }
    currentGif = new AnimatedGif2E131(buf, options, mappingFunction);
    currentGif.startAnimation(fps);

    currentTimeout = setTimeout(() => {
        currentGif.stopAnimation();
        try {
            currentGif.output.close(() => {}); // disconnect from the E131 server explicitly
        } catch (e) {
            console.log("Error disconnecting from E131 server: " + e);
        }
    }, duration);
    console.log("Playing gif for " + duration + "ms");
}

const requestListener = function (req, res) {
    // play gif if "/play" is called with parameters "f" (for "file"), "d" (for "duration") and "s" (for "frames per second")
    const url = new URL(req.url, `http://${req.headers.host}`);
    const basename = url.searchParams.get("f");
    const duration = url.searchParams.get("d");
    const fps = url.searchParams.get("s");

    // bail out if file or duration is not specified
    if (!basename || !duration || !fps) {
        console.log("No gif specified");
        res.writeHead(200, { "Content-Type": "application/json" });
        res.end(
            JSON.stringify({
                status: "Fail",
                message:
                    "No gif + duration + fps specified. Use /play?f=<filename>&d=<duration>&s=<fps>",
            })
        );
        return;
    }

    // error out if basename contains forbidden characters
    if (basename.match(/[^a-zA-Z0-9\.\-_]/)) {
        console.log("Invalid gif specified");
        res.writeHead(200, { "Content-Type": "application/json" });
        res.end(
            JSON.stringify({
                status: "Fail",
                message: "Invalid gif specified",
            })
        );
        return;
    }

    // error out if file does not exist in the "gifs" directory
    const file = "gifs/" + basename + ".gif";
    if (!fs.existsSync(file)) {
        console.log("File " + file + " does not exist");
        res.writeHead(200, { "Content-Type": "application/json" });
        res.end(
            JSON.stringify({
                status: "Fail",
                message: "File " + file + " does not exist",
            })
        );
        return;
    }

    console.log(
        "Playing gif " + file + " for " + duration + "ms with " + fps + "fps"
    );
    try {
        playGifForSpecifiedTime(duration, file, fps);
    } catch (e) {
        console.log("Error playing gif: " + e);
        res.writeHead(500, { "Content-Type": "application/json" });
        res.end(
            JSON.stringify({
                status: "Fail",
                message: "Error playing gif",
            })
        );
        return;
    }
    // confirm with a JSON "OK" response
    res.writeHead(200, { "Content-Type": "application/json" });
    res.end(JSON.stringify({ status: "OK" }));
};

const server = http.createServer(requestListener);
server.listen(listenport, listenhost, () => {
    console.log(`Server is running on http://${listenhost}:${listenport}`);
});

// playGifForSpecifiedTime(5000, "fire16x16.gif");
// setTimeout(() => {
//     playGifForSpecifiedTime(5000, "test3.gif");
// }, 3000);

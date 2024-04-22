const http = require("http");
const AnimatedGif2E131 = require("animatedgif2e131");
const fs = require("fs");

const listenhost = process.env.LISTEN_HOST || "localhost";
const listenport = process.env.LISTEN_PORT || 8000;
const e131_port = 5568; // WLED E1.31 port

var instance_id = 0;
var currentHostData = {};

// WLED uses 510 channels per universe (170 LEDs), i.e. no LED is shared between universes
function mapping_wled(sourceX, sourceY, width, height) {
    var channelNumber = sourceX * 3 + sourceY * width * 3 + 1;
    return {
        universe: Math.floor(channelNumber / 510) + 1,
        channel: channelNumber % 510,
    };
}

function playGifForSpecifiedTime(duration, filename, fps = 25, e131_host) {
    var options = {
        port: e131_port,
        host: e131_host,
    };

    var id = instance_id++;
    var data = currentHostData[e131_host];
    if (!data) {
        data = {
            gif: null,
            timeout: null,
            id: id,
        };
        currentHostData[e131_host] = data;
    }

    console.log("[#" + id + "] " + "Loading gif from " + filename);
    var buf = fs.readFileSync(filename);

    if (data["timeout"]) {
        console.log("[#" + id + "] " + "Clearing previous timeout for host " + e131_host + " (#" + data["id"] + ")");
        clearTimeout(data["timeout"]);
    }

    if (data["gif"]) {
        console.log("[#" + id + "] " + "Stopping previous gif animation" + " (#" + data["id"] + ")");
        data["gif"].stopAnimation();
        try {
            data["gif"].output.close(() => { }); // disconnect from the E131 server explicitly
        } catch (e) {
            console.log("[#" + id + "] " + "Error disconnecting from E131 server " + e131_host + ":  +                e            );
        }
    }
    data["id"] = id;
    data["gif"] = new AnimatedGif2E131(buf, options, mapping_wled);
    data["gif"].send(); // fix to send the first frame immediately instead of waiting for the first timeout (which may be too long for the first frame to be displayed if fps is set to a low value like 0.5)
    data["gif"].startAnimation(fps);

    data["timeout"] = setTimeout(() => {
        data["gif"].stopAnimation();
        try {
            data["gif"].output.close(() => { }); // disconnect from the E131 server explicitly
            data["gif"] = null;
        } catch (e) {
            console.log("[#" + id + "] " + "Error disconnecting from E131 server: " + e);
        }
        console.log("[#" + id + "] " + "stopped playing gif");
        data["timeout"] = null;
    }, duration);
    console.log("[#" + id + "] " + "Playing gif for " + duration + "ms");
}

const requestListener = function (req, res) {
    // play gif if "/play" is called with parameters "f" (for "file"), "d" (for "duration") and "s" (for "frames per second")
    const url = new URL(req.url, `http://${req.headers.host}`);
    const basename = url.searchParams.get("gif");
    const duration_seconds = url.searchParams.get("len");
    const fps = url.searchParams.get("fps");
    const e131_host = url.searchParams.get("host");

    // bail out if file or duration is not specified
    if (!basename || !duration_seconds || !fps || !e131_host) {
        console.log("No gif specified");
        res.writeHead(400, { "Content-Type": "application/json" });
        res.end(
            JSON.stringify({
                status: "Fail",
                message:
                    "No gif + len + fps + host specified. Use /play?gif=<filename>&len=<duration>&fps=<fps>&host=<host>",
            })
        );
        return;
    }

    // error out if basename contains forbidden characters
    if (basename.match(/[^a-zA-Z0-9\.\-_]/)) {
        console.log("Invalid gif specified");
        res.writeHead(400, { "Content-Type": "application/json" });
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
        res.writeHead(400, { "Content-Type": "application/json" });
        res.end(
            JSON.stringify({
                status: "Fail",
                message: "File " + file + " does not exist",
            })
        );
        return;
    }

    console.log("Playing gif " + file + " on " + e131_host + " for " + duration_seconds + "s at " + fps + "fps");
    try {
        playGifForSpecifiedTime(duration_seconds * 1000, file, fps, e131_host);
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

const server = http.createServer(); // requestListener);
server.on("request", async (req, res) => {
    requestListener(req, res);
});
server.listen(listenport, listenhost, () => {
    console.log(`Server is running on http://${listenhost}:${listenport}`);
});

// playGifForSpecifiedTime(5000, "gifs/test_pattern.gif", 0.5);
// setTimeout(() => {
//     playGifForSpecifiedTime(5000, "test3.gif");
// }, 3000);

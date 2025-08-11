from flask import Flask, request, jsonify
import threading
from PIL import Image
import os
import io
import re
import socket
import time

app = Flask(__name__)

LISTEN_HOST = os.getenv("LISTEN_HOST", "localhost")
LISTEN_PORT = os.getenv("LISTEN_PORT", 8000)
WLED_REALTIME_PORT = 21324  # see https://kno.wled.ge/interfaces/udp-realtime/

instance_id = 0
currentHostData = {}


def send_colors(clientSock, clientAddr, colors, fps):
    v = []
    v.append(2)  # DRGB protocol

    frame_duration = int(1 / fps)
    v.append(
        max(int(frame_duration * 2), 1)
    )  # seconds to wait after no packet is received

    # for color in colors:
    #     v.extend(color[0], color[1], color[2])

    v.extend(colors)  # Assuming colors is a flat list of RGB values

    Message = bytearray(v)

    clientSock.sendto(Message, clientAddr)


# Placeholder for handling GIF to E131 conversion.
def send_frame_to_wled(frame_data, fps, clientSock, clientAddr):
    # Implement the logic to send the frame to the E131 host
    # print(f"Sending frame data to {clientAddr[0]} on port {clientAddr[1]}")
    send_colors(clientSock, clientAddr, frame_data, fps)
    time.sleep(1 / fps)


def play_gif_for_specified_time(duration, filename, fps, wled_host):
    global instance_id

    id = instance_id
    instance_id += 1

    print(f"[#{id}] Loading GIF from {filename}")

    with Image.open(filename) as img:
        frames = []
        for frame in range(0, img.n_frames):
            img.seek(frame)
            rgb_stream = img.convert("RGB").tobytes()

            # rgb_triplets = [list(rgb_stream[i:i+3]) for i in range(0, len(rgb_stream), 3)]
            # frames.append(rgb_triplets)

            frames.append(rgb_stream)

    print(f"[#{id}] Playing gif for {duration / 1000}s")

    clientSock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)

    # Convert the "wled_host" to IP and keep a cache for it
    if wled_host not in currentHostData:
        ip = socket.gethostbyname(wled_host)
        currentHostData[wled_host] = {"ip": ip, "port": WLED_REALTIME_PORT}

    clientAddr = (currentHostData[wled_host]["ip"], currentHostData[wled_host]["port"])

    while duration > 0:
        # Send each frame
        for frame_data in frames:
            send_frame_to_wled(frame_data, fps, clientSock, clientAddr)
            duration -= 1000 / fps

    clientSock.close()
    print(f"[#{id}] Playback ended for {filename}")


@app.route("/play", methods=["GET"])
def play():
    basename = request.args.get("gif")
    duration_seconds = request.args.get("len")
    fps = request.args.get("fps")
    wled_host = request.args.get("host")

    if not basename or not duration_seconds or not fps or not wled_host:
        print("No gif specified")
        return (
            jsonify(
                {
                    "status": "Fail",
                    "message": "No gif + len + fps + host specified. Use /play?gif=<filename>&len=<duration>&fps=<fps>&host=<host>",
                }
            ),
            400,
        )

    if re.match(r"[^a-zA-Z0-9\.\-_]", basename):
        print("Invalid gif specified")
        return jsonify({"status": "Fail", "message": "Invalid gif specified"}), 400

    file = f"gifs/{basename}.gif"
    if not os.path.exists(file):
        print(f"File {file} does not exist")
        return (
            jsonify({"status": "Fail", "message": f"File {file} does not exist"}),
            400,
        )

    try:
        print(
            f"Playing gif {file} on {wled_host} for {duration_seconds}s at {fps}fps (background thread)"
        )
        t = threading.Thread(
            target=play_gif_for_specified_time,
            args=(float(duration_seconds) * 1000, file, float(fps), wled_host),
            daemon=True,
        )
        t.start()
    except Exception as e:
        print(f"Error starting background thread: {e}")
        return (
            jsonify({"status": "Fail", "message": "Error starting background thread"}),
            500,
        )

    return jsonify({"status": "OK", "message": "Playback started in background"}), 200


if __name__ == "__main__":
    app.run(host=LISTEN_HOST, port=LISTEN_PORT, threaded=True)

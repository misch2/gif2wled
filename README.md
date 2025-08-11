# Play GIF on WLED using UDPRealtime protocol

A simple way to display animated GIFs on a WLED matrix (or a WLED strip).

# Installation
Enable "Receive UDP realtime" in WLED Sync Interfaces.

Then install the server requirements:

```
pip -r requirements.txt
LISTEN_PORT=8000 python server.py
```

Then run a test:
```
curl 'http://localhost:8000/play?len=2&gif=test_pattern&fps=1&host=1.2.3.4'
```
where 1.2.3.4 is your WLED IP address or hostname.

A test pattern should be displayed for 2 seconds. This can be used to verify correctnes of the mapping function for the given display orientation.

# Adding GIFs

Just put them into the "gifs/" subfolder.

# HomeAssistant

configuration.yaml:
```
...
rest_command: !include rest_command.yaml
```

rest_command.yaml:
```
wled_matrix_play_animation:
  method: get
  url: http://xxxxx:8000/play?gif={{ file }}&len={{ duration }}&fps={{ fps }}&host={{ host }}
```
where "xxxxx:8000" is address of the server.

## Sample automation action

Plays a "doorbell_animated" GIF for 20 seconds with 0.5 second per frame:
```
service: rest_command.wled_matrix_play_animation
data:
  file: doorbell_animated
  duration: 20
  fps: 2
```


# Example systemd unit file

Service listening on port 8000 with WLED host on address 1.2.3.4:

```
# /etc/systemd/system/michals-gif2wled.service
[Unit]
Description=GIF to WLED web server
After=network-online.target

[Service]
WorkingDirectory={{ /home/foobar/gif2wled_e131 }}
Environment="LISTEN_HOST=0.0.0.0"
Environment="LISTEN_PORT=8000"
ExecStart=/usr/bin/node server.js
Type=simple
Restart=always
KillMode=control-group
TimeoutStartSec=30
TimeoutStopSec=15
```

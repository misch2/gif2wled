# GIF to WLED via E1.31 (DMX-512)

A simple way to display animated GIFs on WLED matrix.

# Installation
```
npm i
WLED_HOST=1.2.3.4 node server.js 
```
then
```
curl 'http://localhost:8000/play?len=2&gif=test_pattern&fps=1'
```
A test pattern should be displayed for 2 seconds

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
  url: http://xxxxx:8000/play?gif={{ file }}&len={{ duration }}&fps={{ fps }}
```

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

Service listening on port 8000:

```
# /etc/systemd/system/michals-gif2wled.service
[Unit]
Description=GIF to WLED web server
After=network-online.target

[Service]
WorkingDirectory={{ /home/foobar/gif2wled_e131 }}
Environment="WLED_HOST=1.2.3.4"
Environment="LISTEN_HOST=0.0.0.0"
Environment="LISTEN_PORT=8000"
ExecStart=/usr/bin/node server.js
Type=simple
Restart=always
KillMode=control-group
TimeoutStartSec=30
TimeoutStopSec=15
```

# GIF to WLED via E1.31 (DMX-512)

A simple way to display animated GIFs on WLED matrix.

Advantages of this method over defining those animations directly via WLED:
 1. Easy to use: no need to split the GIF into individual frames and then convert them to WLED-compatible bitmap definitions
 2. No need to save/restore WLED state when displaying an animation only temporarily. WLED with E1.31 does this automatically
 3. Play complex animations without cluttering the WLED presets or playlists with too many items

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
  url: http://xxxxx:8000/play?gif={{ file }}&len={{ duration }}&fps={{ fps }}
```
where "xxxxx:8000" is address of the nodejs server.

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

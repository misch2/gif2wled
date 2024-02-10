#!/bin/env python3
import sacn
import time

ip_address = "10.52.6.54"
# time.sleep(5)
from PIL import Image

filename = "fire.gif"
width = 16
height = 16

matrix_size = (width, height)
# frame_pause = 0.5
frame_pause = 0.05


def generate_image_data(image):
    image_data = []
    for y in range(height):
        for x in range(width):
            if y % 2 == 0:
                for val in image.getpixel((15 - x, y)):
                    image_data.append(val)
            else:
                for val in image.getpixel((x, y)):
                    image_data.append(val)
    return image_data


def generate_image_data_palette(image):
    image_data = []
    for y in range(height):
        for x in range(width):
            if y % 2 == 0:
                start_val = image.getpixel((15 - x, y)) * 3
                print("y=" + str(y))
                print("start_val=" + str(start_val))
                for i in range(start_val, start_val + 3):
                    image_data.append(image.getpalette()[i])
            else:
                start_val = image.getpixel((x, y)) * 3
                for i in range(start_val, start_val + 3):
                    image_data.append(image.getpalette()[i])
    return image_data


def send_two_universes(data, sender):
    sender[1].dmx_data = data[:510]
    sender[2].dmx_data = data[510:]


# load a gif
print("loading gif")
img = Image.open(filename)
img.seek(1)
out_imgs = []
out_datas = []
try:
    while 1:
        this_img = img.resize(matrix_size)
        out_datas.append(generate_image_data_palette(this_img))
        img.seek(img.tell() + 1)
except EOFError:
    pass

# connect to matrix
sender = sacn.sACNsender(fps=40)
sender.start()  # start the sending thread
sender.activate_output(1)
sender[1].destination = ip_address
sender.activate_output(2)
sender[2].destination = ip_address
sender.manual_flush = True
print("start")
for i in range(5):
    print(i)
    for data in out_datas:
        send_two_universes(data, sender)
        sender.flush()
        time.sleep(frame_pause)
sender.stop()

# Copyright (C) 2019-2022 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

# Distributed under MIT license.
# See file LICENSE for detail or copy at https://opensource.org/licenses/MIT


import sys
from io import *
from time import time

class VirtualStream(TextIOBase):
    def __init__(self):
        super().__init__()
        self.buffer = ''
    def write(self, text):
        self.buffer += text
    def read(self):
        ret = self.buffer
        self.buffer = ''
        return ret

sys.stdout = VirtualStream()

# recv_port = // get recv port via writing this line before writing this script
send_port = recv_port+1
address = '127.0.0.1'

import socket
from ssl import SOL_SOCKET

s = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
s.setsockopt(SOL_SOCKET, socket.SO_RCVTIMEO, 1)
s.bind((address, recv_port))

import json

message = {
    'success': True,
    'message': 'server started'
}
buf = json.dumps(message).encode('utf8')
s.sendto(buf, (address, send_port))

while True:
    try:
        buf, port = s.recvfrom(65536)
    except:
        continue
    cmd = buf.decode('utf8')
    try:
        start = time()
        exec(cmd)
        print('consumed:', time()-start)
    except Exception as e:
        message = {
            'success': False,
            'message': str(e)
        }
    else:
        message = {
            'success': True,
            'message': sys.stdout.read()
        }
    finally:
        buf = json.dumps(message).encode('utf8')
        s.sendto(buf, (address, send_port))

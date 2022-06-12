import sys
from io import *

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

if len(sys.argv) != 2:
    sys.exit(-1)

recv_port = int(sys.argv[1])
send_port = recv_port+1
address = '127.0.0.1'

import socket
from ssl import SOL_SOCKET

s = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
s.setsockopt(SOL_SOCKET, socket.SO_RCVTIMEO, 1)
s.bind((address, recv_port))

import json

while True:
    try:
        buf, port = s.recvfrom(1024)
    except:
        continue
    cmd = buf.decode('utf8')
    try:
        exec(cmd)
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
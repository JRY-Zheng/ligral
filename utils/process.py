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
        self.buffer += str(text)
    def read(self):
        ret = self.buffer
        self.buffer = ''
        return ret

sys.stdout = VirtualStream()
sys.stderr = VirtualStream()

import builtins
builtins.old_exec = builtins.exec

def try_exec(source, globals=None, locals=None):
    try:
        builtins.old_exec(source, globals, locals)
    except Exception as e:
        sys.stderr.write('error occurred when executing '+source+': '+str(e))

# builtins.exec = try_exec

import socket
import json

class Status:
    def __init__(self) -> None:
        self.port = 8781
        self.address = '127.0.0.1'
        self.s = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
        message = {
            'success': True,
            'message': 'server started'
        }
        buf = json.dumps(message).encode('utf8')
        self.s.sendto(buf, (self.address, self.port))

    def __call__(self):
        out = sys.stdout.read()
        err = sys.stderr.read()
        message = {
            'success': err == '',
            'message': out + err
        }
        buf = json.dumps(message).encode('utf8')
        self.s.sendto(buf, (self.address, self.port))

__status__ = Status()

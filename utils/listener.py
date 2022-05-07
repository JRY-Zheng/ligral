# Copyright (C) 2019-2021 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

# Distributed under MIT license.
# See file LICENSE for detail or copy at https://opensource.org/licenses/MIT


import socket
import json
from ssl import SOL_SOCKET
from time import sleep
import sys

UDP_IP_ADDRESS = '127.0.0.1'
if len(sys.argv)==2:
    UDP_PORT_NO = int(sys.argv[1])
else:
    UDP_PORT_NO = 8784

serverSock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
serverSock.setsockopt(SOL_SOCKET, socket.SO_RCVTIMEO, 1)
serverSock.bind((UDP_IP_ADDRESS, UDP_PORT_NO))
for i in range(10000):
    try:
        buf = serverSock.recvfrom(1024)
    except:
        continue
    else:
        print(buf)
    # sleep(0.1)
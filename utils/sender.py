import socket
import json
from time import sleep

UDP_IP_ADDRESS = '127.0.0.1'
UDP_PORT_NO = 8783

serverSock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
# serverSock.bind((UDP_IP_ADDRESS, UDP_PORT_NO))
jsonbytes = json.dumps({"label":0xffb0}).encode('ascii')
for i in range(100):
    serverSock.sendto(jsonbytes, (UDP_IP_ADDRESS, UDP_PORT_NO))
    sleep(0.1)
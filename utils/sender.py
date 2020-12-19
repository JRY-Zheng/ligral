import socket

UDP_IP_ADDRESS = '127.0.0.1'
UDP_PORT_NO = 8783

serverSock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
serverSock.bind((UDP_IP_ADDRESS, UDP_PORT_NO))
serverSock.sendto(b'{label:1}', (UDP_IP_ADDRESS, UDP_PORT_NO))
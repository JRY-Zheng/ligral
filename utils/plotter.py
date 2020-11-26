import sys
import socket
import json
import pyqtgraph


if __name__ == "__main__":
    try:
        UDP_IP_ADDRESS = str(sys.argv[1])
        UDP_PORT_NO = int(sys.argv[2])
    except:
        print("parameter error. usage: python plotter 127.0.0.1 8783")
        sys.exit(1)

    serverSock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
    serverSock.bind((UDP_IP_ADDRESS, UDP_PORT_NO))

    while True:
        databytes, addr = serverSock.recvfrom(1024)
        data = json.loads(databytes)
        print(data)
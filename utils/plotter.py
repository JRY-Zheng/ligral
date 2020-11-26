import sys
import socket
import json
import threading
import matplotlib.pyplot as plt
import numpy as np

class Figure:
    def __init__(self, fig, ax):
        self.fig = fig
        self.ax = ax
        self.curves = {}
        self.showed = False

    def __getitem__(self, item):
        if isinstance(self.ax, np.ndarray):
            return self.ax[item]
        elif item==(0,0) or item==0:
            return self.ax

    def show(self):
        self.showed = True

class PlotterHandler:
    def __init__(self, agg='mpl'):
        if agg=='mpl':
            self.plotter = plt
        else:
            raise NotImplementedError()
        self.figs = {}
        self.handle = self.handler_wrapper(self.handle)
        self.pools = []

    def invoke(self, label, data):
        self.pools.append((label, data))

    def handler_wrapper(self, handler):
        def wrapper(label, data):
            try:
                handler(label, data)
            except KeyError as e:
                print(f'invalid packet: {e}')
        return wrapper

    def handle(self, label, data):
        if label==0xffe0:
            self.figs[data['fig']].show()
        elif label==0xffd0:
            figure = self.figs[data['fig']]
            cv = figure.curves[data['curve']]
        elif label==0xffc0:
            figure = self.figs[data['fig']]
            ax = figure[data['row'], data['col']]
            cv = ax.plot([], [])
            figure.curves[data['curve']] = cv
        elif label==0xffb0:
            fig, ax = self.plotter.subplots(data['rows'], data['cols'])
            fig.suptitle(data['title'])
            self.figs[data['fig']] = Figure(fig, ax)
        elif label==0xffa0:
            figure = self.figs[data['fig']]
            ax = figure[data['row'], data['col']]
            ax.set_xlabel(data['xlabel'])
            ax.set_ylabel(data['ylabel'])
            ax.grid()
        else:
            raise NotImplementedError()
    
def task(serverSock, handler):
    while True:
        packetbytes, addr = serverSock.recvfrom(1024)
        packet = json.loads(packetbytes)
        handler.invoke(packet['label'], packet['data'])

if __name__ == "__main__":
    try:
        UDP_IP_ADDRESS = str(sys.argv[1])
        UDP_PORT_NO = int(sys.argv[2])
    except:
        UDP_IP_ADDRESS = '127.0.0.1'
        UDP_PORT_NO = 8783

    serverSock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
    serverSock.bind((UDP_IP_ADDRESS, UDP_PORT_NO))

    handler = PlotterHandler()

    threading._start_new_thread(task, (serverSock, handler))

    while not handler.figs or np.array([not fig.showed for i, fig in handler.figs.items()]).any():
        if handler.pools:
            handler.handle(*handler.pools.pop())

    plt.show()
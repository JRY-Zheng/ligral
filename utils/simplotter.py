# Copyright (C) 2019-2021 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

# Distributed under MIT license.
# See file LICENSE for detail or copy at https://opensource.org/licenses/MIT


import sys
import socket
import json
import threading
import matplotlib.pyplot as plt
from matplotlib.animation import FuncAnimation
import numpy as np

class Figure:
    def __init__(self, fig, ax):
        self.fig = fig
        self.ax = ax
        self.curves = {}
        self.showed = False

    def __getitem__(self, item):
        if isinstance(self.ax, np.ndarray):
            if isinstance(item, tuple) and len(item)==2 and (item[0]==0 or item[1]==0):
                return self.ax[item[0]+item[1]]
            else:
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
        self.pools = []
        self.handle = self.handler_wrapper(self.handle)

    def invoke(self, label, data):
        self.pools.append((label, data))

    def handler_wrapper(self, handler):
        def wrapper(label, data):
            try:
                handler(label, data)
            except KeyError as e:
                print(f'ERROR: invalid packet: {e}')
        return wrapper

    def handle(self, label, data):
        if label==0xffe0:
            figure = self.figs[data['fig']]
            figure.show()
        elif label==0xffd0:
            figure = self.figs[data['fig']]
            cv, row, col = figure.curves[data['curve']]
            ax = figure[row, col]
            xdata, ydata = cv.get_xdata(), cv.get_ydata()
            xdata = np.append(xdata, data['xvalue'])
            ydata = np.append(ydata,data['yvalue'])
            cv.set_data(xdata, ydata)
            ax.set_xlim(*self.get_lim(xdata))
            ax.set_ylim(*self.get_lim(ydata))
        elif label==0xffc0:
            figure = self.figs[data['fig']]
            ax = figure[data['row'], data['col']]
            cv, = ax.plot([], [])
            figure.curves[data['curve']] = cv, data['row'], data['col']
        elif label==0xffb0:
            fig, ax = self.plotter.subplots(data['rows'], data['cols'], num=data['title'])
            fig.suptitle(data['title'])
            self.figs[data['fig']] = Figure(fig, ax)
        elif label==0xffa0:
            figure = self.figs[data['fig']]
            ax = figure[data['row'], data['col']]
            ax.set_xlabel(data['xlabel'])
            ax.set_ylabel(data['ylabel'])
            ax.grid()
        else:
            pass

    def get_lim(self, data, margin=0.06):
        factor = 1 + margin
        left, right = min(data), max(data)
        return factor*left-margin*right, factor*right-margin*left
    
def task(serverSock, handler):
    print(f'INFO: server started at {UDP_IP_ADDRESS}:{UDP_PORT_NO}')
    while True:
        packetbytes, addr = serverSock.recvfrom(1024)
        packet = json.loads(packetbytes)
        # print(f'INFO: received packet {packet}')
        label, data = packet['label'], packet['data']
        if label < 0xff00:
            pass
        handler.invoke(label, data)

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
            handler.handle(*handler.pools.pop(0))

    plt.ion()

    while True:
        for i in range(100):
            if not handler.pools:
                break
            handler.handle(*handler.pools.pop(0))
        plt.pause(0.01)
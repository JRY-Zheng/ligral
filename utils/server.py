import numpy as np
import json
import socket

class VariableServer:    
    def __init__(self) -> None:
        self.address = '127.0.0.1'
        self.port = 8782
        self.serverSock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
    def check_list(self, var):
        if len(var) == 0:
            raise Exception('Row number cannot be zero')
        if all(isinstance(row, list) for row in var):
            for row in var:
                for item in row:
                    if not isinstance(item, float) and not isinstance(item, int):
                        raise Exception('Item should be number')
        elif all(isinstance(row, int) or isinstance(row, float) for row in var):
            return [var, ]
        colno = [len(row) for row in var]
        colmatch = [col==colno[0] for col in colno]
        if not all(colmatch):
            raise Exception('Column numbers are not matched')
        if colno[0] == 0:
            raise Exception('Column number cannot be zero')
        return var
    def check_ndarray(self, var):
        if not np.issubdtype(var.dtype, np.integer) and \
            not np.issubdtype(var.dtype, np.floating):
            raise Exception('Item should be number')
        if len(var) == 0:
            raise Exception('Empty matrix is not supported')
        if len(var.shape) > 2:
            raise Exception('High dimension is not supported')
        elif len(var.shape) == 2:
            return [[*row] for row in var]
        elif len(var.shape) == 1:
            return [[*var], ]
        else:
            raise Exception('Invalid shape')
    def get(self, name):
        pool = {**globals()}
        if name not in pool:
            self.send({
                'success': False,
                'message': 'not found'
            })
            return
        val = pool[name]
        try:
            if isinstance(val, list):
                val = self.check_list(val)
            elif isinstance(val, np.matrix):
                val = self.check_ndarray(val.A)
            elif isinstance(val, np.ndarray):
                val = self.check_ndarray(val)
            else:
                self.send({
                    'success': False,
                    'message': f'type {type(val)} is not supported'
                })
                return
        except Exception as e:
            self.send({
                'success': False,
                'message': f'error encountered: {e}'
            })
        self.send({
            'success': True,
            'value': val
        })
    def send(self, message):
        buffer = json.dumps(message).encode('utf8')
        self.serverSock.sendto(buffer, (self.address, self.port))
        # raise Exception('im here')

__server__ = VariableServer()

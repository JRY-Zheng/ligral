import numpy as np

class Plant:
    def __init__(self):
        self.n = 1
        self.m = 1
        self.p = 1
        self.A = np.zeros((1,1))
        self.D = np.zeros((1,1))
        self.C = np.zeros((1,1))
        self.D = np.zeros((1,1))

    def f(self, x, u, t):
        return x

    def h(self, x, u, t):
        return x

class Pendulum(Plant):
    def __init__(self):
        self.n = 2
        self.m = 1
        self.p = 1
        self.M = 0.1
        self.g = 9.8
        self.l = 0.5
        self.I = 0.001
        self.A = np.matrix([[0, 1], [-self.M*self.g*self.l/self.I, 0]])
        self.B = np.matrix([[0], [1/self.I]])
        self.C = np.matrix([[1, 0]])
        self.D = np.zeros((1,1))

    def f(self, x, u, t):
        x1, x2 = x[:,0]
        x1dot = x2
        x2dot = -self.M*self.g*self.l/self.I*np.sin(x1)+u[0,0]/self.I
        return np.vstack((x1dot, x2dot))

    def h(self, x, u, t):
        return x[0]
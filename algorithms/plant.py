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

from sqp import SQP

class AlgebraicLoop(Plant):
    def __init__(self):
        self.n = 2
        self.m = 1
        self.p = 1
        self.q = 1
        self.z0 = np.matrix(0.0)
        self.A = np.matrix([[0, 1], [-2, -2]])
        self.B = np.matrix([[0], [2]])
        self.C = np.matrix([[1, 0]])
        self.D = np.matrix(0)

    def fz(self, x, u, t, z):
        x1, x2 = x[:,0]
        x1dot = x2
        x2dot = -x1-x2+z/2+u[0,0]
        return np.vstack((x1dot, x2dot))
    
    def gz(self, x, u, t, z):
        x1, x2 = x[:,0]
        return -x1-x2+z/2+u[0,0]

    def f(self, x, u, t):
        def cost(z):
            dz = z-self.z0
            return dz.T*dz
        def equal(z):
            return self.gz(x, u, t, z)-z
        def bound(z):
            return np.matrix(np.zeros((0,1)))
        sqp = SQP()
        sqp.optimize(cost, self.z0, equal, bound)
        print('x', x, 'u', u)
        z = sqp.results()
        print('z', z)
        return self.fz(x, u, t, z)

    def h(self, x, u, t):
        return x[0]
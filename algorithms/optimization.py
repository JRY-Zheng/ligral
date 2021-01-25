import numpy as np

class Optimizer:
    def __init__(self):
        self.wmax = 0.3
        self.wmin = 0.1
        self.c1max = 2
        self.c1min = 1.5
        self.c2max = 2
        self.c2min = 1.5
        self.gmax = 100
        self.m = 20

    def optimize(self, cost, x0, xmax, xmin):
        self.x0s = x0 + (np.random.rand((self.m))*2-1)*(xmax-xmin)/10
        self.xs = np.copy(self.x0s)
        self.vs = np.zeros_like(self.xs)
        self.Pij = np.copy(self.x0s)
        self.Cij = [cost(x) for x in self.xs.T]
        self.opt = np.argmin(self.Cij)
        self.Pj = self.Pij[:,self.opt:self.opt+1]
        self.Cj = self.Cij[self.opt]
        for g in range(1, self.gmax):
            c1 = self.c1max-g/self.gmax*(self.c1max-self.c1min)
            c2 = self.c2min+g/self.gmax*(self.c2max-self.c2min)
            w = self.wmax-g/self.gmax*(self.wmax-self.wmin)
            for p in range(self.m):
                self.vs[:,p] = w*self.vs[:,p]+c1*np.random.rand()*(self.Pij[:,p]-self.xs[:,p])+c2*np.random.rand()*(self.Pj[:,0]-self.xs[:,p])
                self.xs[:,p] = self.xs[:,p]+self.vs[:,p]
                C = cost(self.xs[:,p])
                if self.Cij[p]>C:
                    self.Cij[p] = C
                    self.Pij[:,p] = self.xs[:,p]
            self.opt = np.argmin(self.Cij)
            self.Pj = self.Pij[:,self.opt:self.opt+1]
            self.Cj = self.Cij[self.opt]
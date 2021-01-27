import numpy as np

class Optimizer:
    def optimize(self, cost, x0, xmax, xmin):
        pass

    def results(self):
        return None

class PSO(Optimizer):
    def __init__(self):
        self.wmax = 0.3
        self.wmin = 0.1
        self.c1max = 2
        self.c1min = 1.5
        self.c2max = 2
        self.c2min = 1.5
        self.gmax = 1000
        self.m = 20
        self.max_repeat_count = 20

    def results(self):
        return self.Pj

    def optimize(self, cost, x0, xmax, xmin):
        count = 0
        x0 = np.matrix(x0)
        self.x0s = x0 + (xmax-xmin).tolist()*(np.random.rand(len(x0),self.m)*2-1)/10
        self.xs = self.x0s.copy()
        self.vs = np.matrix(np.zeros_like(self.xs))
        self.Pij = self.x0s.copy()
        self.Cij = [cost(x.T) for x in self.xs.T]
        self.opt = np.argmin(self.Cij)
        self.Pj = self.Pij[:,self.opt]
        self.Cj = self.Cij[self.opt]
        for g in range(1, self.gmax):
            c1 = self.c1max-g/self.gmax*(self.c1max-self.c1min)
            c2 = self.c2min+g/self.gmax*(self.c2max-self.c2min)
            w = self.wmax-g/self.gmax*(self.wmax-self.wmin)
            for p in range(self.m):
                self.vs[:,p] = w*self.vs[:,p]+c1*np.diag(np.random.rand(len(x0)))*(self.Pij[:,p]-self.xs[:,p])+c2*np.random.rand()*(self.Pj[:,0]-self.xs[:,p])
                self.xs[:,p] = self.xs[:,p]+self.vs[:,p]
                C = cost(self.xs[:,p])
                if self.Cij[p]>C:
                    self.Cij[p] = C
                    self.Pij[:,p] = self.xs[:,p]
            self.opt = np.argmin(self.Cij)
            self.Pj = self.Pij[:,self.opt]
            if self.Cj == self.Cij[self.opt]:
                count += 1
                if count > self.max_repeat_count:
                    break
            else:
                count = 0
            self.Cj = self.Cij[self.opt]
        print('generations =', g)
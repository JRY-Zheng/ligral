from optimization import *
from plant import *

class TrimCondition:
    def __init__(self, plant:Plant):
        self.n = plant.n
        self.p = plant.p
        self.m = plant.m
        self.plant = plant
        self.x0 = np.matrix(np.zeros((self.n,1)))
        self.x_der = np.zeros_like(self.x0)
        self.u0 = np.matrix(np.zeros((self.p,1)))
        self.y0 = np.matrix(np.zeros((self.m,1)))
        self.x_wgt = np.zeros_like(self.x0).T
        self.x_der_wgt = np.ones_like(self.x0).T
        self.u_wgt = np.zeros_like(self.u0).T
        self.y_wgt = np.zeros_like(self.y0).T
        self.x_max = np.ones_like(self.x0)*1e5
        self.x_der_max = np.ones_like(self.x0)*1e5
        self.u_max = np.ones_like(self.u0)*1e5
        self.y_max = np.ones_like(self.y0)*1e5
        self.x_min = np.ones_like(self.x0)*-1e5
        self.x_der_min = np.ones_like(self.x0)*-1e5
        self.u_min = np.ones_like(self.u0)*-1e5
        self.y_min = np.ones_like(self.y0)*-1e5

class Trimmer:
    def __init__(self, optimizer:Optimizer):
        self.optimizer = optimizer

    def trim(self, plant:Plant, condition:TrimCondition, t=0):
        def cost(p):
            assert p.shape == (condition.n+condition.p,1)
            x = p[:condition.n]
            dx = x - condition.x0
            u = p[condition.n:] - condition.u0
            du = u - condition.u0
            dx_der = plant.f(x, u, t) - condition.x_der
            dy = plant.h(x, u, t) - condition.y0
            return (condition.x_wgt*np.diag(dx.T.tolist()[0])*dx+
                    condition.x_der_wgt*np.diag(dx_der.T.tolist()[0])*dx_der+
                    condition.u_wgt*np.diag(du.T.tolist()[0])*du+
                    condition.y_wgt*np.diag(dy.T.tolist()[0])*dy)
        p0 = np.vstack((condition.x0, condition.u0))
        p_max = np.vstack((condition.x_max, condition.u_max))
        p_min = np.vstack((condition.x_min, condition.u_min))
        self.optimizer.optimize(cost, p0, p_max, p_min)
        x = self.optimizer.Pj[:plant.n]
        u = self.optimizer.Pj[plant.n:]
        return x, u
import numpy as np
from plant import *

class Linearizer:
    def __init__(self):
        self.eps = 1e-3

    def linearize(self, plant:Plant, x_star, u_star, t_star):
        # self.plant = plant
        # self.x_star = x_star
        # self.u_star = u_star
        # self.t_star = t_star
        assert x_star.shape == (plant.n, 1)
        assert u_star.shape == (plant.p, 1)
        assert type(t_star) is float or type(t_star) is int
        dx = 0.1*np.abs(x_star)+1
        du = 0.1*np.abs(u_star)+1
        A = np.zeros((plant.n, plant.n))
        B = np.zeros((plant.n, plant.p))
        C = np.zeros((plant.m, plant.n))
        D = np.zeros((plant.m, plant.p))
        for i in range(plant.n):
            A[:,i:i+1] = self.partial(lambda d: plant.f(x_star+self.mask(plant.n, i, d), u_star, t_star), dx[i])
        for i in range(plant.m):
            B[:,i:i+1] = self.partial(lambda d: plant.f(x_star, u_star+self.mask(plant.p, i, d), t_star), du[i])
        for i in range(plant.n):
            C[:,i:i+1] = self.partial(lambda d: plant.h(x_star+self.mask(plant.n, i, d), u_star, t_star), dx[i])
        for i in range(plant.m):
            D[:,i:i+1] = self.partial(lambda d: plant.h(x_star, u_star+self.mask(plant.p, i, d), t_star), du[i])
        return A, B, C, D

    def mask(self, length, index, value):
        vec = np.zeros((length, 1))
        vec[index] = value
        return vec

    def partial(self, func, d0):
        slopes = []
        d = d0
        for i in range(100):
            vp = func(d)
            vn = func(-d)
            slope = (vp-vn)/d/2
            if slopes and (np.abs(slope-slopes[-1])<self.eps).all():
                return slope
            slopes.append(slope)
            d = d/2
        raise Exception('cannot converge')
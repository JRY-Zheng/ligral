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
        '''Obtain the partial derivative of the function func(d).
        The initial bias is d0, then iterate half of the previous
        bias until the slope gets stable at an asymptotic value.
        '''
        # Initial slope
        vp = func(d0)
        vn = func(-d0)
        slopes = (vp-vn)/d0/2
        d = d0/2
        # Iterate at most 100 time to get slope-series
        for i in range(100):
            vp = func(d)
            vn = func(-d)
            slope = (vp-vn)/d/2
            # If the difference between this slope and the previous
            # slope is small enough, then stop iteration. But at least 
            # calculate four slops
            if (np.abs(slope.T[0]-slopes.T[-1])<self.eps).all():
                if len(slopes.T)>3:
                    break
            slopes = np.hstack((slopes, slope))
            d = d/2
        # For each value in the slope vector, calculate the asymptotic
        # value, and stack them back to a vector
        return np.vstack([self.asymptotic(slope.T) for slope in np.matrix(slopes)])

    def asymptotic(self, series):
        '''Calculate the asymptotic value of a scalar series.
        The series must be exponentially stable and monotonic
        increasing or decreasing. Only the range of the tail
        of the series where this condition is satisfied will
        be taken into account for asymptotic value calculation.
        '''
        assert len(series) > 3
        diff = series[-2] - series[-1]
        # If the last two value are the same, they are the asymptotic value.
        if diff == 0:
            return series[-1]
        # Otherwise, the difference between them defines whether the series
        # should be monotonic increasing or decreasing.
        mode = 1 if diff > 0 else -1
        tail = diff*mode
        for i in range(-2, -len(series), -1):
            diff = series[i-1] - series[i]
            # Only the range where the condition is satisfied will be added
            # to th tail series. Beside, the differences should be smaller 
            # and smaller. Once these conditions are no longer satisfied,
            # the tail series is cut off.
            if diff*mode > tail[0]:
                tail = np.vstack((diff*mode, tail))
            else:
                break
        assert len(tail)>3
        # Since the series is assumed to be exponentially stable, it can be
        # fitted by xi = B*A^i, where i=1...n. Let y = log(xn), x = i, we got
        # y = log(A)*x + log(B), so that least squares regression method can
        # be used here to estimate A and B.
        n = len(tail)
        y = np.log(tail)
        x = np.arange(n)
        ybar = sum(y)/n
        xbar = sum(x)/n
        khat = (sum(x*y)-n*xbar*ybar)/(sum(x*x)-n*xbar**2)
        if khat>0:
            raise Exception('series is not stable')
        bhat = ybar-khat*xbar
        B = np.exp(bhat)
        A = np.exp(khat)
        # Now we confirmed that the difference of slopes satisfies that
        # diff[i] = B*A^i, and we have the slope at n `series[-1]`. We 
        # have to calculate the sum of differences from n to infinity.
        # They are diff[n], diff[n+1], diff[n+2], ..., diff[inf], and the 
        # common ratio is A, so the sum of them is diff[n]/(1-A)
        print(ybar, xbar, sum(x*y), sum(x*x))
        print(khat, bhat, A, B, series[-1])
        return series[-1]-mode*B*A**n/(1-A)
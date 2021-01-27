from optimization import *

class SQP(Optimizer):
    def __init__(self):
        self.eps = 1e-8
        self.tolerant = 1e-6

    def gradient(self, f, x):
        n, cols = x.shape
        assert cols == 1
        fx = f(x)
        m, cols = fx.shape
        assert cols == 1
        A = np.matrix(np.ones((m, n)))
        for i in range(n):
            xp = x.copy()
            xp[i] = x[i]+self.eps
            A[:,i] = (f(xp)-fx)/self.eps
        return A

    def hessian(self, f, x):
        return self.gradient(lambda x: self.gradient(f, x).T, x)
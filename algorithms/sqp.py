from optimization import *

class SQP(Optimizer):
    def __init__(self):
        self.eps = 1e-5
        self.tolerant = 1e-3

    def gradient(self, f, x):
        n, cols = x.shape
        assert cols == 1
        fx = f(x)
        m, cols = fx.shape
        assert cols == 1
        A = np.matrix(np.ones((n, m)))
        for i in range(n):
            xp = x.copy()
            xp[i] = x[i]+self.eps
            A[i] = (f(xp)-fx).T/self.eps
        return A

    def hessian(self, f, x):
        return self.gradient(lambda x: self.gradient(f, x), x)

    def optimize(self, cost, x0, equal, bound):
        n_s, cols = x0.shape
        assert cols == 1
        x = x0
        c = cost(x)
        for i in range(1000):
            H = self.hessian(cost, x0)
            C = self.gradient(cost, x0)
            Ae = self.gradient(equal, x0).T
            Be = equal(x0)
            n_lam, cols = Be.shape
            assert cols == 1
            Ab = self.gradient(bound, x0).T
            Bb = bound(x0)
            sel = np.array(Bb).T[0] >= self.eps
            A = np.vstack((Ae, Ab[sel]))
            B = np.vstack((Be, Bb[sel]))
            n_mu, cols = Bb[sel].shape
            assert cols == 1
            K = (np.vstack((np.hstack((H, A.T)),np.hstack((A,np.zeros((n_lam+n_mu, n_lam+n_mu)))))))
            p = np.linalg.inv(K) * np.vstack((-C,-B))
            s = p[:n_s]
            lam = p[n_s:]
            x += s
            c_new = cost(x)
            if (np.abs(lam) < self.tolerant).all():
                print('optimal value found')
                break 
            elif (np.abs(c_new - c)<self.tolerant).all():
                print('cannot found optimal value')
                break
            else:
                c = c_new
        self.opt = x
        self.opt_cost = c_new

    def results(self):
        return self.opt


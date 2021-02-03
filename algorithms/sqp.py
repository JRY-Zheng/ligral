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
            H = self.hessian(cost, x)
            C = self.gradient(cost, x)
            Ae = self.gradient(equal, x).T
            Be = equal(x)
            Ae, Be = self.filter(Ae, Be)
            n_lam, cols = Be.shape
            assert cols == 1
            Ab = self.gradient(bound, x).T
            Bb = bound(x)
            sel = np.array(Bb).T[0] >= self.eps
            # A = np.vstack((Ae, Ab[sel]))
            # B = np.vstack((Be, Bb[sel]))
            A, B = self.filter(Ab[sel], Bb[sel], Ae, Be)
            # n_mu, cols = Bb[sel].shape
            n, cols = B.shape
            assert cols == 1
            K = (np.vstack((np.hstack((H, A.T)),np.hstack((A,np.zeros((n, n)))))))
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

    def filter(self, A, B, A0=None, B0=None):
        Ap = A0 if A0 is not None else A[0]
        Bp = B0 if A0 is not None else B[0]
        for i in range(1, len(A)):
            At = np.vstack((Ap, A[i]))
            Bt = np.vstack((Bp, B[i]))
            if np.linalg.matrix_rank(At) == At.shape[0]:
                Ap = At
                Bp = Bt
            elif np.linalg.matrix_rank(np.hstack((At, Bt))) == At.shape[0]:
                pass
            elif A0==None and B0==None:
                pass
            else:
                raise Exception('conflict')
        return Ap, Bp
            

    def results(self):
        return self.opt


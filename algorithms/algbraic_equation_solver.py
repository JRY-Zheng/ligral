from sqp import *

class AESolver:
    def solve(self, f, x0):
        def cost(x):
            dx = x - x0
            print('dx', dx, 'x', x, 'x0', x0)
            return dx.T*dx
        equal = f
        def bound(x):
            return np.matrix(np.zeros((0,1)))
        optimizer = SQP()
        optimizer.optimize(cost, x0.copy(), equal, bound)
        return optimizer.results()

if __name__ == '__main__':
    f = lambda x: np.cos(x) - x
    solver = AESolver()
    x0 = np.matrix([0., 0., 0.]).T
    x = solver.solve(f, x0)
    print(x, f(x))

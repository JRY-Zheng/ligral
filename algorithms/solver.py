import numpy as np
import matplotlib.pyplot as plt
from plant import Pendulum, Plant
from sqp import SQP

class Solver:
    def __init__(self) -> None:
        self.step = 0.1
        self.stop = 10

    def solve(self, plant:Plant, x0):
        self.x = [x0]
        self.y = [plant.h(x0, np.matrix(0), 0)]
        u = np.matrix(0)
        for t in np.arange(self.step, self.stop, self.step):
            x = self.step*plant.f(self.x[-1], u, t)
            self.x.append(x)
            self.y.append(plant.h(x, u, t))

    def plot(self):
        t = np.arange(0, self.stop, self.step)
        x = np.hstack(self.x).T
        y = np.hstack(self.y).T
        plt.plot(t, x)
        plt.title('x')
        plt.figure()
        plt.plot(t, y)
        plt.title('y')
        plt.show()


class ImplicitSolver(Solver):
    def __init__(self) -> None:
        self.sqp = SQP()
        self.step = 0.5
        self.stop = 10

    def solve(self, plant: Plant, x0):
        self.x = [x0]
        self.y = [plant.h(x0, np.matrix(0), 0)]
        def cost(x):
            dx = self.x[-1] - x
            return dx.T*dx
        def equal(x):
            xdot = plant.f(x, np.matrix(0), t)
            return x - x[-1] - self.step*xdot
        def bound(x):
            return np.matrix(np.zeros((0,1)))
        for t in np.arange(self.step, self.stop, self.step):
            self.sqp.optimize(cost, self.x[-1].copy(), equal, bound)
            x = self.sqp.results()
            self.x.append(x)
            self.y.append(plant.h(x, np.matrix(0), t))
        return self.y

def forward_euler(f, x0, h, n):
    xs = [x0]
    x = x0
    for i in range(n):
        x = f(x)*h+x
        xs.append(x)
    return xs

def backward_euler(f, x0, h, n):
    xs = [x0]
    x = x0
    eps = 1e-5
    g = lambda x: x-h*f(x)
    for i in range(n):
        y = x
        for j in range(100):
            gy = g(y)
            k = (g(y+eps)-gy)/eps
            y = (x-gy)/k+y
            if abs(x-gy)<eps:
                break
        x = y
        xs.append(x)
    return xs

# h = 0.1
# tau = 0.01
# t = np.arange(0, 10, h)
# x = np.array(backward_euler(lambda x:-1/tau*x, 1, h, 99))
# # print(len(t), len(x))
# plt.plot(t, x)
# plt.show()

pendulum = Pendulum()
# solver = Solver()
solver = ImplicitSolver()
solver.solve(pendulum, np.matrix([0.1,0.]).T)
solver.plot()
from linearization import *
from optimization import *
from trimmer import *
from sqp import *

class Tester:
    def __init__(self):
        self.eps = 1e-3

    def test_linearization(self, linearizer:Linearizer, plant:Plant):
        print('test linearizer')
        A, B, C, D = linearizer.linearize(plant, np.matrix([[0], [0]]), np.matrix([[0]]), 0)
        print('the theoretical A = \n', plant.A, '\nand we got\n', A, '\n')
        print('the theoretical B = \n', plant.B, '\nand we got\n', B, '\n')
        print('the theoretical C = \n', plant.C, '\nand we got\n', C, '\n')
        print('the theoretical D = \n', plant.D, '\nand we got\n', D, '\n')
        assert (np.abs(A-plant.A)<self.eps).all()
        assert (np.abs(B-plant.B)<self.eps).all()
        assert (np.abs(C-plant.C)<self.eps).all()
        assert (np.abs(D-plant.D)<self.eps).all()
        print('test passed!\n\n')

    def test_pso(self, optimizer:Optimizer):
        print('test optimizer')
        cost = lambda x: sum(np.sin(x)+np.cos(2*x)*1.5+np.sin(1.3*x+0.3)*0.9+2.5)
        test_x = np.matrix(np.arange(4.53, 4.56, 0.0001))
        costs = [cost(x.T) for x in test_x.T]
        index = np.argmin(costs)
        x_opt = test_x[:,index]
        optimizer.optimize(cost, np.matrix(3), np.matrix(-2), np.matrix(3))
        print('the theoretical optimal value is', x_opt, '\nand we got', optimizer.results())
        assert (np.abs(optimizer.results()-x_opt)<self.eps).all()
        print('test passed!\n\n')

    def test_optimization(self, optimizer:Optimizer):
        print('test optimizer')
        cost = lambda x: x.T*x
        x0 = np.matrix([1,2.]).T
        x_opt = np.matrix([0,0]).T
        optimizer.optimize(cost, x0, lambda x:x[0], lambda x: np.vstack((x[0]-3, 1-x[1])))
        print('the theoretical optimal value is', x_opt, '\nand we got', optimizer.results())
        assert (np.abs(optimizer.results()-x_opt)<self.eps).all()
        print('test passed!\n\n')

    def test_trimmer(self, plant:Plant, condition:TrimCondition, trimmer:Trimmer):
        print('test trimmer')
        x, u = trimmer.trim(plant, condition)
        print('x =\n', x, '\nu =\n', u)
        x_der = plant.f(x, u, 0)
        y = plant.h(x, u, 0)
        print('xdot =\n', x_der, '\ny =\n', y)
        assert (np.abs(np.multiply(np.sign(condition.x_wgt), x - condition.x0)) < self.eps).all()
        assert (np.abs(np.multiply(np.sign(condition.x_der_wgt), x_der - condition.x_der)) < self.eps).all()
        assert (np.abs(np.multiply(np.sign(condition.u_wgt), u - condition.u0)) < self.eps).all()
        assert (np.abs(np.multiply(np.sign(condition.y_wgt), y - condition.y0)) < self.eps).all()
        print('test passed!\n\n')

    def test_gradient(self, gradient):
        A = np.random.rand(3,2)
        print('f(x) = Ax, where A =', A)
        Ap = gradient(lambda x: A*x, np.matrix([1.,2.]).T)
        print("we got gradient A' =", Ap)
        assert (np.abs(A-Ap.T) < self.eps).all()
        print('test passed')

    def test_hessian(self, hessian):
        A = np.random.rand(3,3)
        print("f(x) = x'Ax, where A =", A)
        H = hessian(lambda x: x.T*A*x, np.matrix([1.,2.,3.]).T)
        print('we got hessian H =', H)
        assert (np.abs(A+A.T-H) < self.eps).all()
        print('test passed')

    def test(self):
        linearizer = Linearizer()
        pendulum = Pendulum()
        self.test_linearization(linearizer, pendulum)

        optimizer = SQP()
        self.test_optimization(optimizer)

        condition = TrimCondition(pendulum)
        condition.x0[0,0] = 1
        condition.x_wgt[0,0] = 1
        condition.x_wgt[0,1] = 1
        condition.x_der_wgt[0,1] = 1
        condition.u_wgt[0,0] = 0
        condition.x_max[0,0] = 2
        condition.x_min[0,0] = -2
        condition.x_max[1,0] = 2
        condition.x_min[1,0] = -2
        condition.u_max[0,0] = 2
        condition.u_min[0,0] = -2
        optimizer.max_repeat_count = 100
        trimmer = Trimmer(optimizer)
        # self.test_trimmer(pendulum, condition, trimmer)

        sqp = SQP()
        self.test_gradient(sqp.gradient)
        self.test_hessian(sqp.hessian)


tester = Tester()
tester.test()
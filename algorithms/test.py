from linearization import *
from optimization import *

class Tester:
    def __init__(self):
        self.eps = 1e-3

    def test_linearization(self, linearizer:Linearizer, plant:Plant) -> bool:
        A, B, C, D = linearizer.linearize(plant, np.array([[0], [0]]), np.array([[0]]), 0)
        print('the theoretical A = \n', plant.A, '\nand we got\n', A, '\n')
        print('the theoretical B = \n', plant.B, '\nand we got\n', B, '\n')
        print('the theoretical C = \n', plant.C, '\nand we got\n', C, '\n')
        print('the theoretical D = \n', plant.D, '\nand we got\n', D, '\n')
        assert (np.abs(A-plant.A)<self.eps).all()
        assert (np.abs(B-plant.B)<self.eps).all()
        assert (np.abs(C-plant.C)<self.eps).all()
        assert (np.abs(D-plant.D)<self.eps).all()
        print('test passed!')

    def test_optimization(self, optimizer:Optimizer) -> bool:
        opt = Optimizer()
        x_opt = np.array([[-1]])
        opt.optimize(lambda x: sum((x+1)**2), np.array([[1]]), np.array([[-2]]), np.array([[3]]))
        print('the theoretical optimal value is', opt.Pj, '\nand we got', x_opt)
        assert (np.abs(opt.Pj-x_opt)<self.eps).all()
        print('test passed!')

tester = Tester()
linearizer = Linearizer()
pendulum = Pendulum()
tester.test_linearization(linearizer, pendulum)
optimizer = Optimizer()
tester.test_optimization(optimizer)
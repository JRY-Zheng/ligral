from linearization import *

class Tester:
    def __init__(self):
        self.eps = 1e-3

    def test(self, linearizer:Linearizer, plant:Plant) -> bool:
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

tester = Tester()
linearizer = Linearizer()
pendulum = Pendulum()
tester.test(linearizer, pendulum)
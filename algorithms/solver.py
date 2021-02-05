import numpy as np
import matplotlib.pyplot as plt

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

h = 0.1
tau = 0.01
t = np.arange(0, 10, h)
x = np.array(backward_euler(lambda x:-1/tau*x, 1, h, 99))
# print(len(t), len(x))
plt.plot(t, x)
plt.show()
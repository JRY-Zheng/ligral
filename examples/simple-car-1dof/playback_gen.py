# Copyright 2019-2020 Junruoyu Zheng. All rights reserved.

# Distributed under MIT license.
# See file LICENSE for detail or copy at https://opensource.org/licenses/MIT


import numpy as np
import matplotlib.pyplot as plt

time = np.arange(0,10,0.01)
throttle = np.zeros_like(time)
brake = np.zeros_like(time)
throttle[:300] = np.tanh(time[:300])
brake[300:325] = time[300:325] - time[300]
brake[325:350] = time[350] - time[325:350]
throttle[350:900] = 0.4*np.tanh(time[350:900]-time[350])
brake[900:1000] = np.ones_like(time[900:1000])

# plt.plot(time, throttle)
# plt.plot(time, brake)
# plt.show()

with open('examples/simple-car-1dof/throttle.csv', 'w') as f:
    f.write('Time, Data\n')
    for t, thr in zip(time, throttle):
        f.write(f'{t},{thr}\n')

with open('examples/simple-car-1dof/brake.csv', 'w') as f:
    f.write('Time, Data\n')
    for t, br in zip(time, brake):
        f.write(f'{t},{br}\n')
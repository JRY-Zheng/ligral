# Copyright (C) 2019-2020 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

# Distributed under MIT license.
# See file LICENSE for detail or copy at https://opensource.org/licenses/MIT


# Auto-generated by Ligral (c) 2020
import matplotlib.pyplot as plt
import numpy as np
import pandas as pd


fig0, ax0 = plt.subplots(1, 1, num='position')
fig0.suptitle('position')


ax0.set_xlabel('time/s')
ax0.set_ylabel('position')
ax0.grid()


fig1, ax1 = plt.subplots(1, 1, num='position/velocity')
fig1.suptitle('position/velocity')


ax1.set_xlabel('position')
ax1.set_ylabel('velocity')
ax1.grid()


df0 = pd.read_csv(r'C:\Users\zheng\Documents\programming\ligral\examples\mass-spring-damper\out\Data.csv', skipinitialspace=True)


x = df0['Time'].values
y = df0['position'].values


ax0.plot(x, y)


x = df0['position'].values
y = df0['velocity'].values


ax1.plot(x, y)


plt.show()

exit()

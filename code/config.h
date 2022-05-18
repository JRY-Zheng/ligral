#ifndef CONFIG_H
#define CONFIG_H

#include <Eigen/Dense>
using Eigen::Matrix;

#include "solvers.h"

#define h 0.1
#define n 2
#define m 1
#define p 0
#define integral euler_integral
#define Vector<v> Matrix<double, v, 1>
#define context Context<m, n, p>
context ctx;

#endif
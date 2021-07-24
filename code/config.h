#ifndef CONFIG_H
#define CONFIG_H

#include <Eigen/Dense>
using Eigen::Matrix;

#include "solvers.h"

#define h 0.1
#define n 2
#define integral euler_integral
#define Vector Matrix<double, n, 1>

Vector f(Vector x);

#endif
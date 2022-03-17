#ifndef CONFIG_H
#define CONFIG_H

#include <Eigen/Dense>
using Eigen::Matrix;

#include "solvers.h"
#include "models.h"

#define h 0.1
#define n 2
#define m 1
#define p 0
#define integral euler_integral
#define Vector Matrix<double, n, 1>
#define context Context<m, n, p>

Vector f(Vector x);

#endif
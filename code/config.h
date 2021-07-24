#ifndef CONFIG_H
#define CONFIG_H

#include <Eigen/Dense>
using Eigen::MatrixXd;
using Eigen::VectorXd;

#include "solvers.h"

#define h 0.1
#define integral euler_integral

VectorXd f(VectorXd x);

#endif
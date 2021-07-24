#ifndef SOLVERS_H
#define SOLVERS_H

#include <Eigen/Dense>
using Eigen::MatrixXd;
using Eigen::VectorXd;

VectorXd euler_integral(VectorXd f(VectorXd), VectorXd x, double h);

#endif
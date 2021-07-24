#ifndef SOLVERS_H
#define SOLVERS_H

#include <Eigen/Dense>
using Eigen::Matrix;

template<int n>
Matrix<double, n, 1> euler_integral(
    Matrix<double, n, 1> f(Matrix<double, n, 1>), 
    Matrix<double, n, 1> x, double h) {
    return x+f(x)*h;
}

#endif
#ifndef CONTEXT_H
#define CONTEXT_H

#include <eigen3/Eigen>
using Eigen::Matrix;

template<int n, int m, int p>
struct Context {
    double time;
    Matrix<double, n, 1> x;
    Matrix<double, m, 1> y;
    Matrix<double, p, 1> u;
};


#endif
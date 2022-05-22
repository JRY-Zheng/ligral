#ifndef CONTEXT_H
#define CONTEXT_H

#include <Eigen/Dense>
using Eigen::Matrix;
#define Vector(v) Matrix<double, v, 1>

// context must be a template because context is defined before config
template<int m, int n, int p>
struct context_struct {
    double t;
    Vector(n) x;
    Vector(m) y;
    Vector(p) u;
    Vector(n) xdot;
};


#endif
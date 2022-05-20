#ifndef CONTEXT_H
#define CONTEXT_H

#include <Eigen/Dense>
using Eigen::Matrix;

template<int n, int m, int p>
struct Context {
    double time;
    Vector<n> x;
    Vector<m> y;
    Vector<p> u;
    Vector<n> xdot;
};


#endif
#ifndef MODELS_H
#define MODELS_H

#include <Eigen/Dense>
using Eigen::Matrix;

#include <iostream>
#include<config.h>

template<int rows_cnt, int cols_cnt>
struct constant_struct {
    Matrix<double, rows_cnt, cols_cnt> value;
    void calculate(Matrix<double, rows_cnt, cols_cnt>* output) {
        *output = value;
    }
};

template<int rows_cnt, int cols_cnt, int n>
struct integrator_struct {
    Matrix<double, rows_cnt, cols_cnt> initial;
    int index;
    void calculate(Matrix<double, rows_cnt, cols_cnt> xdot,
        Matrix<double, rows_cnt, cols_cnt>* x) {
        for (int r=0; r<rows_cnt; ++r) {
            for (int c=0; c<cols_cnt; ++c) {
                (*x)(r, c) = (*(ctx.x))(index+r*cols_cnt+c);
            }
        }
    }
    void input_update(Matrix<double, rows_cnt, cols_cnt> xdot) {
        for (int r=0; r<rows_cnt; ++r) {
            for (int c=0; c<cols_cnt; ++c) {
                (*(ctx.xdot))(index+r*cols_cnt+c) = xdot(r, c);
            }
        }
    }
};

#endif
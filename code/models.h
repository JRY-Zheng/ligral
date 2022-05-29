#ifndef MODELS_H
#define MODELS_H

#include <Eigen/Dense>
using Eigen::Matrix;

#include <iostream>
#include "config.h"

#define BROADCAST_RCRC 0
#define BROADCAST_RC11 1
#define BROADCAST_11RC 2
#define BROADCAST_RCR1 3
#define BROADCAST_R1RC 4
#define BROADCAST_RC1C 5
#define BROADCAST_1CRC 6

template<int R, int C>
struct Node {
    void calculate(Matrix<double, R, C> input,
        Matrix<double, R, C>* output) {
        *output = input;
    }
};

// template<int R, int C>
// struct Gain {};

// template<int R, int C>
// struct SineWave {};

// template<int R, int C>
// struct Step {};

// template<int R, int C>
// struct Playback {};

template<int R, int C, int n>
struct Integrator {
    Matrix<double, R, C> initial;
    int index;
    context* ctx;
    void calculate(Matrix<double, R, C> xdot,
        Matrix<double, R, C>* x) {
        for (int r=0; r<R; ++r) {
            for (int c=0; c<C; ++c) {
                (*x)(r, c) = ctx->x(index+r*C+c);
            }
        }
    }
    void input_update(Matrix<double, R, C> xdot) {
        for (int r=0; r<R; ++r) {
            for (int c=0; c<C; ++c) {
                ctx->xdot(index+r*C+c) = xdot(r, c);
            }
        }
    }
    void config() {
        for (int r=0; r<R; ++r) {
            for (int c=0; c<C; ++c) {
                ctx->x(index+r*C+c) = initial(r, c);
            }
        }
    }
};

// template<int R, int C>
// struct BoundedIntegrator {};

// template<int R, int C>
// struct Scope {};

// template<int R, int C>
// struct PhaseDiagram {};

// template<int R, int C>
// struct Print {};

template<int R, int C>
struct Constant {
    Matrix<double, R, C> value;
    void calculate(Matrix<double, R, C>* output) {
        *output = value;
    }
};

template<int R, int C, int type>
struct Add {
    void calculate(Matrix<double, R, C> left, 
        Matrix<double, R, C> right, 
        Matrix<double, R, C>* output) {
        *output = left + right;
    }
};

template<int R, int C>
struct Add<R, C, BROADCAST_RC11> {
    void calculate(Matrix<double, R, C> left, 
        Matrix<double, 1, 1> right, 
        Matrix<double, R, C>* output) {
        auto right_val = Matrix<double, R, C>::Ones()*right(0, 0);
        *output = left + right_val;
    }
};

template<int R, int C>
struct Add<R, C, BROADCAST_11RC> {
    void calculate(Matrix<double, 1, 1> left, 
        Matrix<double, R, C> right, 
        Matrix<double, R, C>* output) {
        auto left_val = Matrix<double, R, C>::Ones()*left(0, 0);
        *output = left_val + right;
    }
};

template<int R, int C>
struct Add<R, C, BROADCAST_RCR1> {
    void calculate(Matrix<double, R, C> left, 
        Matrix<double, R, 1> right, 
        Matrix<double, R, C>* output) {
        auto ones = Matrix<double, R, C>::Ones();
        for (int r=0; r<R; r++) {
            output->row(r) = left.row(r) + ones.row(r)*right(r, 0);
        }
    }
};

template<int R, int C>
struct Add<R, C, BROADCAST_R1RC> {
    void calculate(Matrix<double, R, 1> left, 
        Matrix<double, R, C> right, 
        Matrix<double, R, C>* output) {
        auto ones = Matrix<double, R, C>::Ones();
        for (int r=0; r<R; r++) {
            output->row(r) = ones.row(r)*left(r, 0) + right.row(r);
        }
    }
};

template<int R, int C>
struct Add<R, C, BROADCAST_RC1C> {
    void calculate(Matrix<double, R, C> left, 
        Matrix<double, 1, C> right, 
        Matrix<double, R, C>* output) {
        auto ones = Matrix<double, R, C>::Ones();
        for (int c=0; c<C; c++) {
            output->col(c) = left.col(c) + ones.col(c)*right(0, c);
        }
    }
};

template<int R, int C>
struct Add<R, C, BROADCAST_1CRC> {
    void calculate(Matrix<double, 1, C> left, 
        Matrix<double, R, C> right, 
        Matrix<double, R, C>* output) {
        auto ones = Matrix<double, R, C>::Ones();
        for (int c=0; c<C; c++) {
            output->col(c) = ones.col(c)*left(0, c) + right.col(c);
        }
    }
};


// template<int R, int C>
// struct Sub {};

// template<int R, int C>
// struct Mul {};

// template<int R, int C>
// struct Div {};

// template<int R, int C>
// struct RDiv {};

// template<int R, int C>
// struct DotMul {};

// template<int R, int C>
// struct DotDiv {};

// template<int R, int C>
// struct DotPow {};

// template<int R, int C>
// struct Abs {};

// template<int R, int C>
// struct Input {};
#define Input Node

// template<int R, int C>
// struct Output {};
#define Output Node

// template<int R, int C>
// struct Memory {};

// template<int R, int C>
// struct Clock {};

// template<int R, int C>
// struct Deadzone {};

// template<int R, int C>
// struct Saturation {};

template<int R, int C>
struct Sin {
    void calculate(Matrix<double, R, C> input,
        Matrix<double, R, C>* output) {
        *output = input.array().sin();
    }
};

template<int R, int C>
struct Cos {
    void calculate(Matrix<double, R, C> input,
        Matrix<double, R, C>* output) {
        *output = input.array().cos();
    }
};

template<int R, int C>
struct Tan {
    void calculate(Matrix<double, R, C> input,
        Matrix<double, R, C>* output) {
        *output = input.array().tan();
    }
};

// template<int R, int C>
// struct Sinh {};

// template<int R, int C>
// struct Cosh {};

// template<int R, int C>
// struct Tanh {};

// template<int R, int C>
// struct Asin {};

// template<int R, int C>
// struct Acos {};

// template<int R, int C>
// struct Atan {};

// template<int R, int C>
// struct Atan2 {};

// template<int R, int C>
// struct Asinh {};

// template<int R, int C>
// struct Acosh {};

// template<int R, int C>
// struct Atanh {};

// template<int R, int C>
// struct Exp {};

// template<int R, int C>
// struct Pow {};

// template<int R, int C>
// struct Pow2 {};

// template<int R, int C>
// struct Sqrt {};

// template<int R, int C>
// struct Sign {};

// template<int R, int C>
// struct Log {};

// template<int R, int C>
// struct Log2 {};

// template<int R, int C>
// struct LogicSwitch {};

// template<int R, int C>
// struct ThresholdSwitch {};

// template<int R, int C>
// struct Max {};

// template<int R, int C>
// struct Min {};

// template<int R, int C>
// struct Rand {};

// template<int R, int C>
// struct Terminal {};

// template<int R, int C>
// struct VStack {};

// template<int R, int C>
// struct HStack {};

// template<int R, int C>
// struct Split {};

// template<int R, int C>
// struct VSplit {};

// template<int R, int C>
// struct HSplit {};

// template<int R, int C>
// struct InputMarker {};

// template<int R, int C>
// struct OutputSink {};

// template<int R, int C>
// struct Sweep {};

// template<int R, int C>
// struct Inverse {};

// template<int R, int C>
// struct Equal {};

// template<int R, int C>
// struct EqualToZero {};

// template<int R, int C>
// struct Variable {};

// template<int R, int C>
// struct Interpolation {};

// template<int R, int C>
// struct UDPListener {};

// template<int R, int C>
// struct UDPSender {};

// template<int R, int C>
// struct Cross {};

// template<int R, int C>
// struct Transpose {};

// template<int R, int C>
// struct Sec {};

// template<int R, int C>
// struct Csc {};

// template<int R, int C>
// struct Tan {};

// template<int R, int C>
// struct Interpolation2D {};

// template<int R, int C>
// struct InterpolationHD {};


#endif
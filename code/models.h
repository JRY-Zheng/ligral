#ifndef MODELS_H
#define MODELS_H

#include <Eigen/Dense>
using Eigen::Matrix;

#include <iostream>
#include "config.h"

template<int R, int C>
struct Node {
    void calculate(Matrix<double, R, C> input,
        Matrix<double, R, C>* output) {
        *output = input;
    }
};

template<int R, int K, int C>
struct Gain {
    Matrix<double, K, C> value;
    void calculate(Matrix<double, R, K> input,
        Matrix<double, R, C>* output) {
        *output = input*value;
    } 
    void calculate(Matrix<double, K, R> input,
        Matrix<double, R, C>* output) {
        *output = value.transpose()*input;
    } 
};

template<int S>
struct Gain<0, 0, S> {
    Matrix<double, S, S> value;
    void calculate(Matrix<double, S, S> input,
        Matrix<double, S, S>* output) {
        *output = input*value;
    } 
};

template<int S>
struct Gain<S, 0, 0> {
    Matrix<double, S, S> value;
    void calculate(Matrix<double, S, S> input,
        Matrix<double, S, S>* output) {
        *output = value*input;
    } 
};

template<int R, int C>
struct Gain<R, C, 0> {
    Matrix<double, R, C> value;
    void calculate(Matrix<double, 1, 1> input,
        Matrix<double, R, C>* output) {
        *output = value*input(0,0);
    } 
};

template<int R, int C>
struct Gain<0, R, C> {
    Matrix<double, 1, 1> value;
    void calculate(Matrix<double, R, R> input,
        Matrix<double, R, C>* output) {
        *output = value(0,0)*input;
    } 
};

struct SineWave {
    double ampl;
    double omega;
    double phi;
    context* ctx;
    void calculate(Matrix<double, 1, 1>* output) {
        *output = (Matrix<double, 1, 1>::Ones().array()*omega*(ctx->t)+phi).sin()*ampl;
    }
};

struct Step {
    double start;
    double level;
    context* ctx;
    void calculate(Matrix<double, 1, 1>* output) {
        *output = Matrix<double, 1, 1>::Ones()*level*((ctx->t)>=start);
    }
};

template<int R, int C, int L>
struct Playback {
    context* ctx;
    Matrix<double, L, 1> time;
    Matrix<double, L, R*C> table;
    void calculate(Matrix<double, R, C>* output) {
        int i=0;
        if (time(i,0) >= ctx->t) {
            *output = table.row(i);
            return;
        }
        while (time(++i,0) <= ctx->t) {
            if (time(i,0) == ctx->t) {
                *output = table.row(i);
                return;
            }
            if (i+1 >= L) {
                *output = table.row(i);
                return;
            }
        }
        double ratio = ((ctx->t)-time(i-1,0))/(time(i,0)-time(i-1,0));
        *output = table.row(i-1)*(1-ratio)+table.row(i)*ratio;
    }
};

template<int R, int C>
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

template<int R, int C>
struct BoundedIntegrator {
    Matrix<double, R, C> initial;
    int index;
    context* ctx;
    double upper;
    double lower;
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
                if ((ctx->x(r, c) < upper && ctx->x(r, c) > lower) ||
                (ctx->x(r, c) >= upper && xdot(r, c) < 0) ||
                (ctx->x(r, c) <= lower && xdot(r, c) > 0))
                ctx->xdot(index+r*C+c) = xdot(r, c);
                else ctx->xdot(index+r*C+c) = 0;
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

template<int R, int C>
struct Scope {
    int index;
    context* ctx;
    void calculate(Matrix<double, R, C> input) {
        for (int r=0; r<R; ++r) {
            for (int c=0; c<C; ++c) {
                ctx->y(index+r*C+c) = input(r, c);
            }
        }
    }
};

template<int xR, int xC, int yR, int yC>
struct PhaseDiagram {
    int index;
    context* ctx;
    void calculate(Matrix<double, xR, xC> x, Matrix<double, yR, yC> y) {
        for (int r=0; r<xR; ++r) {
            for (int c=0; c<xC; ++c) {
                ctx->y(index+r*xC+c) = x(r, c);
            }
        }
        for (int r=0; r<yR; ++r) {
            for (int c=0; c<yC; ++c) {
                ctx->y(index+r*yC+c+xR*xC) = y(r, c);
            }
        }
    }
};

#define Print Scope

template<int R, int C>
struct Constant {
    Matrix<double, R, C> value;
    void calculate(Matrix<double, R, C>* output) {
        *output = value;
    }
};

template<int R, int C>
struct Add {
    void calculate(Matrix<double, R, C> left, 
        Matrix<double, R, C> right, 
        Matrix<double, R, C>* output) {
        *output = left + right;
    }
    void calculate(Matrix<double, R, C> left, 
        Matrix<double, 1, 1> right, 
        Matrix<double, R, C>* output) {
        auto right_val = Matrix<double, R, C>::Ones()*right(0, 0);
        *output = left + right_val;
    }
    void calculate(Matrix<double, 1, 1> left, 
        Matrix<double, R, C> right, 
        Matrix<double, R, C>* output) {
        auto left_val = Matrix<double, R, C>::Ones()*left(0, 0);
        *output = left_val + right;
    }
    void calculate(Matrix<double, R, C> left, 
        Matrix<double, R, 1> right, 
        Matrix<double, R, C>* output) {
        auto ones = Matrix<double, R, C>::Ones();
        for (int r=0; r<R; r++) {
            output->row(r) = left.row(r) + ones.row(r)*right(r, 0);
        }
    }
    void calculate(Matrix<double, R, 1> left, 
        Matrix<double, R, C> right, 
        Matrix<double, R, C>* output) {
        auto ones = Matrix<double, R, C>::Ones();
        for (int r=0; r<R; r++) {
            output->row(r) = ones.row(r)*left(r, 0) + right.row(r);
        }
    }
    void calculate(Matrix<double, R, C> left, 
        Matrix<double, 1, C> right, 
        Matrix<double, R, C>* output) {
        auto ones = Matrix<double, R, C>::Ones();
        for (int c=0; c<C; c++) {
            output->col(c) = left.col(c) + ones.col(c)*right(0, c);
        }
    }
    void calculate(Matrix<double, 1, C> left, 
        Matrix<double, R, C> right, 
        Matrix<double, R, C>* output) {
        auto ones = Matrix<double, R, C>::Ones();
        for (int c=0; c<C; c++) {
            output->col(c) = ones.col(c)*left(0, c) + right.col(c);
        }
    }
};

template<int R>
struct Add<R, 1> {
    void calculate(Matrix<double, R, 1> left, 
        Matrix<double, R, 1> right, 
        Matrix<double, R, 1>* output) {
        *output = left + right;
    }
    void calculate(Matrix<double, R, 1> left, 
        Matrix<double, 1, 1> right, 
        Matrix<double, R, 1>* output) {
        auto right_val = Matrix<double, R, 1>::Ones()*right(0, 0);
        *output = left + right_val;
    }
    void calculate(Matrix<double, 1, 1> left, 
        Matrix<double, R, 1> right, 
        Matrix<double, R, 1>* output) {
        auto left_val = Matrix<double, R, 1>::Ones()*left(0, 0);
        *output = left_val + right;
    }
};

template<int C>
struct Add<1, C> {
    void calculate(Matrix<double, 1, C> left, 
        Matrix<double, 1, C> right, 
        Matrix<double, 1, C>* output) {
        *output = left + right;
    }
    void calculate(Matrix<double, 1, C> left, 
        Matrix<double, 1, 1> right, 
        Matrix<double, 1, C>* output) {
        auto right_val = Matrix<double, 1, C>::Ones()*right(0, 0);
        *output = left + right_val;
    }
    void calculate(Matrix<double, 1, 1> left, 
        Matrix<double, 1, C> right, 
        Matrix<double, 1, C>* output) {
        auto left_val = Matrix<double, 1, C>::Ones()*left(0, 0);
        *output = left_val + right;
    }
};

template<>
struct Add<1, 1> {
    void calculate(Matrix<double, 1, 1> left, 
        Matrix<double, 1, 1> right, 
        Matrix<double, 1, 1>* output) {
        *output = left + right;
    }
};



template<int R, int C>
struct Sub {
    void calculate(Matrix<double, R, C> left, 
        Matrix<double, R, C> right, 
        Matrix<double, R, C>* output) {
        *output = left - right;
    }
    void calculate(Matrix<double, R, C> left, 
        Matrix<double, 1, 1> right, 
        Matrix<double, R, C>* output) {
        auto right_val = Matrix<double, R, C>::Ones()*right(0, 0);
        *output = left - right_val;
    }
    void calculate(Matrix<double, 1, 1> left, 
        Matrix<double, R, C> right, 
        Matrix<double, R, C>* output) {
        auto left_val = Matrix<double, R, C>::Ones()*left(0, 0);
        *output = left_val - right;
    }
    void calculate(Matrix<double, R, C> left, 
        Matrix<double, R, 1> right, 
        Matrix<double, R, C>* output) {
        auto ones = Matrix<double, R, C>::Ones();
        for (int r=0; r<R; r++) {
            output->row(r) = left.row(r) - ones.row(r)*right(r, 0);
        }
    }
    void calculate(Matrix<double, R, 1> left, 
        Matrix<double, R, C> right, 
        Matrix<double, R, C>* output) {
        auto ones = Matrix<double, R, C>::Ones();
        for (int r=0; r<R; r++) {
            output->row(r) = ones.row(r)*left(r, 0) - right.row(r);
        }
    }
    void calculate(Matrix<double, R, C> left, 
        Matrix<double, 1, C> right, 
        Matrix<double, R, C>* output) {
        auto ones = Matrix<double, R, C>::Ones();
        for (int c=0; c<C; c++) {
            output->col(c) = left.col(c) - ones.col(c)*right(0, c);
        }
    }
    void calculate(Matrix<double, 1, C> left, 
        Matrix<double, R, C> right, 
        Matrix<double, R, C>* output) {
        auto ones = Matrix<double, R, C>::Ones();
        for (int c=0; c<C; c++) {
            output->col(c) = ones.col(c)*left(0, c) - right.col(c);
        }
    }
};

template<int R>
struct Sub<R, 1> {
    void calculate(Matrix<double, R, 1> left, 
        Matrix<double, R, 1> right, 
        Matrix<double, R, 1>* output) {
        *output = left - right;
    }
    void calculate(Matrix<double, R, 1> left, 
        Matrix<double, 1, 1> right, 
        Matrix<double, R, 1>* output) {
        auto right_val = Matrix<double, R, 1>::Ones()*right(0, 0);
        *output = left - right_val;
    }
    void calculate(Matrix<double, 1, 1> left, 
        Matrix<double, R, 1> right, 
        Matrix<double, R, 1>* output) {
        auto left_val = Matrix<double, R, 1>::Ones()*left(0, 0);
        *output = left_val - right;
    }
};

template<int C>
struct Sub<1, C> {
    void calculate(Matrix<double, 1, C> left, 
        Matrix<double, 1, C> right, 
        Matrix<double, 1, C>* output) {
        *output = left - right;
    }
    void calculate(Matrix<double, 1, C> left, 
        Matrix<double, 1, 1> right, 
        Matrix<double, 1, C>* output) {
        auto right_val = Matrix<double, 1, C>::Ones()*right(0, 0);
        *output = left - right_val;
    }
    void calculate(Matrix<double, 1, 1> left, 
        Matrix<double, 1, C> right, 
        Matrix<double, 1, C>* output) {
        auto left_val = Matrix<double, 1, C>::Ones()*left(0, 0);
        *output = left_val - right;
    }
};

template<>
struct Sub<1, 1> {
    void calculate(Matrix<double, 1, 1> left, 
        Matrix<double, 1, 1> right, 
        Matrix<double, 1, 1>* output) {
        *output = left - right;
    }
};

template<int R, int K, int C>
struct Mul {
    void calculate(Matrix<double, R, K> left, 
        Matrix<double, K, C> right, 
        Matrix<double, R, C>* output) {
        *output = left*right;
    }
};
template<int R, int C>
struct Mul<R, 0, C> {
    void calculate(Matrix<double, R, C> left, 
        Matrix<double, 1, 1> right, 
        Matrix<double, R, C>* output) {
        *output = left*right(0,0);
    }
    void calculate(Matrix<double, 1, 1> left, 
        Matrix<double, R, C> right, 
        Matrix<double, R, C>* output) {
        *output = left(0,0)*right;
    }
};

template<int R, int C>
struct Div {
    void calculate(Matrix<double, R, C> left, 
        Matrix<double, 1, 1> right, 
        Matrix<double, R, C>* output) {
        *output = left/right(0,0);
    }
    void calculate(Matrix<double, 1, 1> left, 
        Matrix<double, R, C> right, 
        Matrix<double, R, C>* output) {
        *output = left(0,0)*right.array().inverse();
    }
    void calculate(Matrix<double, R, C> left, 
        Matrix<double, C, C> right, 
        Matrix<double, R, C>* output) {
        *output = left*right.inverse();
    }
};

template<int R>
struct Div<R, 1> {
    void calculate(Matrix<double, R, 1> left, 
        Matrix<double, 1, 1> right, 
        Matrix<double, R, 1>* output) {
        *output = left/right(0,0);
    }
    void calculate(Matrix<double, 1, 1> left, 
        Matrix<double, R, 1> right, 
        Matrix<double, R, 1>* output) {
        *output = left(0,0)*right.array().inverse();
    }
};

template<>
struct Div<1, 1> {
    void calculate(Matrix<double, 1, 1> left, 
        Matrix<double, 1, 1> right, 
        Matrix<double, 1, 1>* output) {
        *output = left/right(0,0);
    }
};

template<int R, int C>
struct RDiv {
    void calculate(Matrix<double, R, C> left, 
        Matrix<double, 1, 1> right, 
        Matrix<double, R, C>* output) {
        *output = left.array().inverse()*right(0,0);
    }
    void calculate(Matrix<double, 1, 1> left, 
        Matrix<double, R, C> right, 
        Matrix<double, R, C>* output) {
        *output = right/left(0,0);
    }
    void calculate(Matrix<double, R, R> left, 
        Matrix<double, R, C> right, 
        Matrix<double, R, C>* output) {
        *output = left.inverse()*right;
    }
};

template<int C>
struct RDiv<1, C> {
    void calculate(Matrix<double, 1, C> left, 
        Matrix<double, 1, 1> right, 
        Matrix<double, 1, C>* output) {
        *output = left.array().inverse()*right(0,0);
    }
    void calculate(Matrix<double, 1, 1> left, 
        Matrix<double, 1, C> right, 
        Matrix<double, 1, C>* output) {
        *output = right/left(0,0);
    }
};

template<>
struct RDiv<1, 1> {
    void calculate(Matrix<double, 1, 1> left, 
        Matrix<double, 1, 1> right, 
        Matrix<double, 1, 1>* output) {
        *output = right/left(0,0);
    }
};

template<int R, int C>
struct DotMul {
    void calculate(Matrix<double, R, C> left, 
        Matrix<double, R, C> right, 
        Matrix<double, R, C>* output) {
        *output = left.array()*right.array();
    }
    void calculate(Matrix<double, R, C> left, 
        Matrix<double, 1, 1> right, 
        Matrix<double, R, C>* output) {
        *output = left*right(0,0);
    }
    void calculate(Matrix<double, 1, 1> left, 
        Matrix<double, R, C> right, 
        Matrix<double, R, C>* output) {
        *output = left(0,0)*right;
    }
    void calculate(Matrix<double, R, C> left, 
        Matrix<double, R, 1> right, 
        Matrix<double, R, C>* output) {
        for (int r=0; r<R; r++) {
            output->row(r) = left.row(r)*right(r, 0);
        }
    }
    void calculate(Matrix<double, R, C> left, 
        Matrix<double, 1, C> right, 
        Matrix<double, R, C>* output) {
        for (int c=0; c<C; c++) {
            output->col(c) = left.col(c)*right(0, c);
        }
    }
    void calculate(Matrix<double, R, 1> left, 
        Matrix<double, R, R> right, 
        Matrix<double, R, C>* output) {
        for (int r=0; r<R; r++) {
            output->row(r) = left(r, 0)*right.row(r);
        }
    }
    void calculate(Matrix<double, 1, C> left, 
        Matrix<double, R, C> right, 
        Matrix<double, R, C>* output) {
        for (int c=0; c<C; c++) {
            output->col(c) = left(0, c)*right.col(c);
        }
    }
};

template<int R>
struct DotMul<R, 1> {
    void calculate(Matrix<double, R, 1> left, 
        Matrix<double, R, 1> right, 
        Matrix<double, R, 1>* output) {
        *output = left.array()*right.array();
    }
    void calculate(Matrix<double, R, 1> left, 
        Matrix<double, 1, 1> right, 
        Matrix<double, R, 1>* output) {
        *output = left*right(0,0);
    }
    void calculate(Matrix<double, 1, 1> left, 
        Matrix<double, R, 1> right, 
        Matrix<double, R, 1>* output) {
        *output = left(0,0)*right;
    }
};

template<int C>
struct DotMul<1, C> {
    void calculate(Matrix<double, 1, C> left, 
        Matrix<double, 1, C> right, 
        Matrix<double, 1, C>* output) {
        *output = left.array()*right.array();
    }
    void calculate(Matrix<double, 1, C> left, 
        Matrix<double, 1, 1> right, 
        Matrix<double, 1, C>* output) {
        *output = left*right(0,0);
    }
    void calculate(Matrix<double, 1, 1> left, 
        Matrix<double, 1, C> right, 
        Matrix<double, 1, C>* output) {
        *output = left(0,0)*right;
    }
};

template<>
struct  DotMul<1, 1> {
    void calculate(Matrix<double, 1, 1> left, 
        Matrix<double, 1, 1> right, 
        Matrix<double, 1, 1>* output) {
        *output = left*right;
    }
};


template<int R, int C>
struct DotDiv {
    void calculate(Matrix<double, R, C> left, 
        Matrix<double, R, C> right, 
        Matrix<double, R, C>* output) {
        *output = left.array()/right.array();
    }
    void calculate(Matrix<double, R, C> left, 
        Matrix<double, 1, 1> right, 
        Matrix<double, R, C>* output) {
        *output = left/right(0,0);
    }
    void calculate(Matrix<double, 1, 1> left, 
        Matrix<double, R, C> right, 
        Matrix<double, R, C>* output) {
        *output = left(0,0)*right.array().inverse();
    }
    void calculate(Matrix<double, R, C> left, 
        Matrix<double, R, 1> right, 
        Matrix<double, R, C>* output) {
        for (int r=0; r<R; r++) {
            output->row(r) = left.row(r)/right(r, 0);
        }
    }
    void calculate(Matrix<double, R, C> left, 
        Matrix<double, 1, C> right, 
        Matrix<double, R, C>* output) {
        for (int c=0; c<C; c++) {
            output->col(c) = left.col(c)/right(0, c);
        }
    }
    void calculate(Matrix<double, R, 1> left, 
        Matrix<double, R, R> right, 
        Matrix<double, R, C>* output) {
        for (int r=0; r<R; r++) {
            output->row(r) = left(r, 0)*right.row(r).array().inverse();
        }
    }
    void calculate(Matrix<double, 1, C> left, 
        Matrix<double, R, C> right, 
        Matrix<double, R, C>* output) {
        for (int c=0; c<C; c++) {
            output->col(c) = left(0, c)*right.col(c).array().inverse();
        }
    }
};

template<int R>
struct DotDiv<R, 1> {
    void calculate(Matrix<double, R, 1> left, 
        Matrix<double, R, 1> right, 
        Matrix<double, R, 1>* output) {
        *output = left.array()/right.array();
    }
    void calculate(Matrix<double, R, 1> left, 
        Matrix<double, 1, 1> right, 
        Matrix<double, R, 1>* output) {
        *output = left/right(0,0);
    }
    void calculate(Matrix<double, 1, 1> left, 
        Matrix<double, R, 1> right, 
        Matrix<double, R, 1>* output) {
        *output = left(0,0)*right.array().inverse();
    }
};

template<int C>
struct DotDiv<1, C> {
    void calculate(Matrix<double, 1, C> left, 
        Matrix<double, 1, C> right, 
        Matrix<double, 1, C>* output) {
        *output = left.array()/right.array();
    }
    void calculate(Matrix<double, 1, C> left, 
        Matrix<double, 1, 1> right, 
        Matrix<double, 1, C>* output) {
        *output = left/right(0,0);
    }
    void calculate(Matrix<double, 1, 1> left, 
        Matrix<double, 1, C> right, 
        Matrix<double, 1, C>* output) {
        *output = left(0,0)*right.array().inverse();
    }
};

template<>
struct DotDiv<1, 1> {
    void calculate(Matrix<double, 1, 1> left, 
        Matrix<double, 1, 1> right, 
        Matrix<double, 1, 1>* output) {
        *output = left/right(0,0);
    }
};

// template<int R, int C>
// struct DotPow {};

template<int R, int C>
struct Abs {
    void calculate(Matrix<double, R, C> input,
        Matrix<double, R, C>* output) {
        *output = input.array().abs();
    }
};

#define Input Node

#define Output Node

// template<int R, int C>
// struct Memory {};

struct Clock {
    context* ctx;
    void calculate(Matrix<double, 1, 1>* output) {
        *output = Matrix<double, 1, 1>::Ones()*(ctx->t);
    }
};

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

template<int R, int C>
struct Sinh {
    void calculate(Matrix<double, R, C> input,
        Matrix<double, R, C>* output) {
        *output = input.array().sinh();
    }
};

template<int R, int C>
struct Cosh {
    void calculate(Matrix<double, R, C> input,
        Matrix<double, R, C>* output) {
        *output = input.array().cosh();
    }
};

template<int R, int C>
struct Tanh {
    void calculate(Matrix<double, R, C> input,
        Matrix<double, R, C>* output) {
        *output = input.array().tanh();
    }
};

template<int R, int C>
struct Asin {
    void calculate(Matrix<double, R, C> input,
        Matrix<double, R, C>* output) {
        *output = input.array().asin();
    }
};

template<int R, int C>
struct Acos {
    void calculate(Matrix<double, R, C> input,
        Matrix<double, R, C>* output) {
        *output = input.array().acos();
    }
};

template<int R, int C>
struct Atan {
    void calculate(Matrix<double, R, C> input,
        Matrix<double, R, C>* output) {
        *output = input.array().atan();
    }
};

template<int R, int C>
struct Atan2 {
    void calculate(Matrix<double, R, C> y,
        Matrix<double, R, C> x,
        Matrix<double, R, C>* output) {
        *output = y.binaryExpr(x, std::ptr_fun(::atan2));
    }
    void calculate(Matrix<double, R, C> y, 
        Matrix<double, 1, 1> x, 
        Matrix<double, R, C>* output) {
        auto x_val = Matrix<double, R, C>::Ones()*x(0, 0);
        *output = y.binaryExpr(x_val, std::ptr_fun(::atan2));
    }
    void calculate(Matrix<double, 1, 1> y, 
        Matrix<double, R, C> x, 
        Matrix<double, R, C>* output) {
        auto y_val = Matrix<double, R, C>::Ones()*y(0, 0);
        *output = y_val.binaryExpr(x, std::ptr_fun(::atan2));
    }
    void calculate(Matrix<double, R, C> y, 
        Matrix<double, R, 1> x, 
        Matrix<double, R, C>* output) {
        auto ones = Matrix<double, R, C>::Ones();
        for (int r=0; r<R; r++) {
            output->row(r) = y.row(r).binaryExpr(ones.row(r)*y(r, 0), std::ptr_fun(::atan2));
        }
    }
    void calculate(Matrix<double, R, 1> y, 
        Matrix<double, R, C> x, 
        Matrix<double, R, C>* output) {
        auto ones = Matrix<double, R, C>::Ones();
        for (int r=0; r<R; r++) {
            output->row(r) = (ones.row(r)*x(r, 0)).binaryExpr(y.row(r), std::ptr_fun(::atan2));
        }
    }
    void calculate(Matrix<double, R, C> y, 
        Matrix<double, 1, C> x, 
        Matrix<double, R, C>* output) {
        auto ones = Matrix<double, R, C>::Ones();
        for (int c=0; c<C; c++) {
            output->col(c) = y.col(c).binaryExpr(ones.col(c)*x(0, c), std::ptr_fun(::atan2));
        }
    }
    void calculate(Matrix<double, 1, C> y, 
        Matrix<double, R, C> x, 
        Matrix<double, R, C>* output) {
        auto ones = Matrix<double, R, C>::Ones();
        for (int c=0; c<C; c++) {
            output->col(c) = (ones.col(c)*y(0, c)).binaryExpr(x.col(c), std::ptr_fun(::atan2));
        }
    }
};

template<int R>
struct Atan2<R, 1> {
    void calculate(Matrix<double, R, 1> y,
        Matrix<double, R, 1> x,
        Matrix<double, R, 1>* output) {
        *output = y.binaryExpr(x, std::ptr_fun(::atan2));
    }
    void calculate(Matrix<double, R, 1> y, 
        Matrix<double, 1, 1> x, 
        Matrix<double, R, 1>* output) {
        auto x_val = Matrix<double, R, 1>::Ones()*x(0, 0);
        *output = y.binaryExpr(x_val, std::ptr_fun(::atan2));
    }
    void calculate(Matrix<double, 1, 1> y, 
        Matrix<double, R, 1> x, 
        Matrix<double, R, 1>* output) {
        auto y_val = Matrix<double, R, 1>::Ones()*y(0, 0);
        *output = y_val.binaryExpr(x, std::ptr_fun(::atan2));
    }
};

template<int C>
struct Atan2<1, C> {
    void calculate(Matrix<double, 1, C> y,
        Matrix<double, 1, C> x,
        Matrix<double, 1, C>* output) {
        *output = y.binaryExpr(x, std::ptr_fun(::atan2));
    }
    void calculate(Matrix<double, 1, C> y, 
        Matrix<double, 1, 1> x, 
        Matrix<double, 1, C>* output) {
        auto x_val = Matrix<double, 1, C>::Ones()*x(0, 0);
        *output = y.binaryExpr(x_val, std::ptr_fun(::atan2));
    }
    void calculate(Matrix<double, 1, 1> y, 
        Matrix<double, 1, C> x, 
        Matrix<double, 1, C>* output) {
        auto y_val = Matrix<double, 1, C>::Ones()*y(0, 0);
        *output = y_val.binaryExpr(x, std::ptr_fun(::atan2));
    }
};

template<>
struct Atan2<1, 1> {
    void calculate(Matrix<double, 1, 1> y,
        Matrix<double, 1, 1> x,
        Matrix<double, 1, 1>* output) {
        *output = y.binaryExpr(x, std::ptr_fun(::atan2));
    }
};

template<int R, int C>
struct Asinh {
    void calculate(Matrix<double, R, C> input,
        Matrix<double, R, C>* output) {
        *output = input.unaryExpr(std::ptr_fun(::asinh));
    }
};

template<int R, int C>
struct Acosh {
    void calculate(Matrix<double, R, C> input,
        Matrix<double, R, C>* output) {
        *output = input.unaryExpr(std::ptr_fun(::acosh));
    }
};

template<int R, int C>
struct Atanh {
    void calculate(Matrix<double, R, C> input,
        Matrix<double, R, C>* output) {
        *output = input.unaryExpr(std::ptr_fun(::atanh));
    }
};

template<int R, int C>
struct Exp {
    void calculate(Matrix<double, R, C> input,
        Matrix<double, R, C>* output) {
        *output = input.array().exp();
    }
};

// template<int R, int C>
// struct Pow {};

// template<int R, int C>
// struct Pow2 {};

template<int R, int C>
struct Sqrt {
    void calculate(Matrix<double, R, C> input,
        Matrix<double, R, C>* output) {
        *output = input.array().sqrt();
    }
};

template<int R, int C>
struct Sign {
    void calculate(Matrix<double, R, C> input,
        Matrix<double, R, C>* output) {
        for (int r=0; r<R; ++r) {
            for (int c=0; c<C; ++c) {
                (*output)(r, c) = input(r, c)>0?1:input(r, c)<0?-1:0;
            }
        }
    }
};

template<int R, int C>
struct Log {
    void calculate(Matrix<double, R, C> input,
        Matrix<double, R, C>* output) {
        *output = input.array().log();
    }
};

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

template<int R, int C>
struct Terminal {
    void calculate(Matrix<double, R, C> input) { }
};

template<int R, int C, int ...iR>
struct VStack {
    void calculate(Matrix<double, iR, C>...input, 
        Matrix<double, R, C>* output) {
        int r=0;
        setRows(output, &r, input...);
    }
private:
    template<typename T0, typename ...T>
    void setRows(Matrix<double, R, C>* output, int* r, T0 row0, T...rows) {
        output->block(*r, 0, row0.rows(), C) = row0;
        *r = *r+row0.rows();
        setRows(output, r, rows...);
    }
    void setRows(Matrix<double, R, C>* output, int* r) {}
};

template<int R, int C, int ...iC>
struct HStack {
    void calculate(Matrix<double, R, iC>...input, 
        Matrix<double, R, C>* output) {
        int c=0;
        setCols(output, &c, input...);
    }
private:
    template<typename T0, typename ...T>
    void setCols(Matrix<double, R, C>* output, int* c, T0 col0, T...cols) {
        output->block(0, *c, R, col0.cols()) = col0;
        *c = *c+col0.cols();
        setCols(output, c, cols...);
    }
    void setCols(Matrix<double, R, C>* output, int* c) {}
};

// template<int R, int C>
// struct Split {};

template<int R, int C>
struct VSplit {
    template<int ...iR>
    void calculate(Matrix<double, R, C> input,
        Matrix<double, iR, C>*...output) {
        int r=0;
        getRows(input, &r, output...);
    }
    template<typename T0, typename ...T>
    void getRows(Matrix<double, R, C> input, int* r, T0* row0, T*...rows) {
        *row0 = input.block(*r, 0, row0->rows(), C);
        *r = *r+row0->rows();
        getRows(input, r, rows...);
    }
    void getRows(Matrix<double, R, C> input, int* r) {}
};

template<int R, int C>
struct HSplit {
    template<int ...iC>
    void calculate(Matrix<double, R, C> input,
        Matrix<double, R, iC>*...output) {
        int c=0;
        getCols(input, &c, output...);
    }
    template<typename T0, typename ...T>
    void getCols(Matrix<double, R, C> input, int* c, T0* col0, T*...cols) {
        *col0 = input.block(0, *c, R, col0->cols());
        *c = *c+col0->cols();
        getCols(input, c, cols...);
    }
    void getCols(Matrix<double, R, C> input, int* c) {}
};

template<int R, int C>
struct InputMarker {
    int index;
    context* ctx;
    void calculate(Matrix<double, R, C> _,
        Matrix<double, R, C>* u) {
        for (int r=0; r<R; ++r) {
            for (int c=0; c<C; ++c) {
                (*u)(r, c) = ctx->u(index+r*C+c);
            }
        }
    }
};

#define OutputSink Scope

struct Sweep {
    double k;
    double A;
    context* ctx;
    void calculate(Matrix<double, 1, 1>* output) {
        *output = (((ctx->t*k+1)*Matrix<double, 1, 1>::Ones()).array().log()*A).sin();
    }
};

template<int S>
struct Inverse {
    void calculate(Matrix<double, S, S> input, 
        Matrix<double, S, S>* output) {
        *output = input.inverse();
    }
};

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

struct Cross {
    void calculate(Matrix<double, 1, 3> left, 
        Matrix<double, 1, 3> right, 
        Matrix<double, 1, 3>* output) {
        *output = left.cross(right);
    }
};

template<int R, int C>
struct Transpose {
    void calculate(Matrix<double, R, C> input,
        Matrix<double, C, R>* output) {
        *output = input.transpose();
    }
};

template<int R, int C>
struct Sec {
    void calculate(Matrix<double, R, C> input,
        Matrix<double, R, C>* output) {
        *output = input.array().cos().inverse();
    }
};

template<int R, int C>
struct Csc {
    void calculate(Matrix<double, R, C> input,
        Matrix<double, R, C>* output) {
        *output = input.array().sin().inverse();
    }
};

template<int R, int C>
struct Cot {
    void calculate(Matrix<double, R, C> input,
        Matrix<double, R, C>* output) {
        *output = input.array().tan().inverse();
    }
};

// template<int R, int C>
// struct Interpolation2D {};

// template<int R, int C>
// struct InterpolationHD {};

template<int R, int C>
struct TransferFunction {
    Matrix<double, R, R> a;
    Matrix<double, R, 1> b;
    Matrix<double, 1, R> c;
    Matrix<double, 1, 1> d;
    Matrix<double, R, C> x;
    int index;
    context* ctx;
    void calculate(Matrix<double, 1, C> u,
        Matrix<double, 1, C>* y) {
        for (int r=0; r<R; ++r) {
            for (int c=0; c<C; ++c) {
                x(r, c) = ctx->x(index+r*C+c);
            }
        }
        *y = c*x+d*u;
    }
    void input_update(Matrix<double, 1, C> u) {
        auto xdot = a*x+b*u;
        for (int r=0; r<R; ++r) {
            for (int c=0; c<C; ++c) {
                ctx->xdot(index+r*C+c) = xdot(r, c);
            }
        }
    }
    void config() {
        for (int r=0; r<R; ++r) {
            for (int c=0; c<C; ++c) {
                ctx->x(index+r*C+c) = x(r, c);
            }
        }
    }
};


#endif
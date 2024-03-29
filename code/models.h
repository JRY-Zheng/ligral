#ifndef MODELS_H
#define MODELS_H

#include <Eigen/Dense>
#include <Eigen/MatrixFunctions>
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
            *output = table.row(i).reshaped(R, C);
            return;
        }
        while (time(++i,0) <= ctx->t) {
            if (time(i,0) == ctx->t) {
                *output = table.row(i).reshaped(R, C);
                return;
            }
            if (i+1 >= L) {
                *output = table.row(i).reshaped(R, C);
                return;
            }
        }
        double ratio = ((ctx->t)-time(i-1,0))/(time(i,0)-time(i-1,0));
        *output = (table.row(i-1)*(1-ratio)+table.row(i)*ratio).reshaped(R, C);
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

template<int R, int C>
struct DotPow {
    void calculate(Matrix<double, R, C> left, 
        Matrix<double, R, C> right, 
        Matrix<double, R, C>* output) {
        *output = left.array().pow(right.array());
    }
    void calculate(Matrix<double, R, C> left, 
        Matrix<double, 1, 1> right, 
        Matrix<double, R, C>* output) {
        *output = left.array().pow(right(0,0));
    }
    void calculate(Matrix<double, 1, 1> left, 
        Matrix<double, R, C> right, 
        Matrix<double, R, C>* output) {
        *output = Eigen::pow(left(0,0), right.array());
    }
    void calculate(Matrix<double, R, C> left, 
        Matrix<double, R, 1> right, 
        Matrix<double, R, C>* output) {
        for (int r=0; r<R; r++) {
            output->row(r) = left.row(r).array().pow(right(r, 0));
        }
    }
    void calculate(Matrix<double, R, C> left, 
        Matrix<double, 1, C> right, 
        Matrix<double, R, C>* output) {
        for (int c=0; c<C; c++) {
            output->col(c) = left.col(c).array().pow(right(0, c));
        }
    }
    void calculate(Matrix<double, R, 1> left, 
        Matrix<double, R, R> right, 
        Matrix<double, R, C>* output) {
        for (int r=0; r<R; r++) {
            output->row(r) = Eigen::pow(left(r, 0),right.row(r).array());
        }
    }
    void calculate(Matrix<double, 1, C> left, 
        Matrix<double, R, C> right, 
        Matrix<double, R, C>* output) {
        for (int c=0; c<C; c++) {
            output->col(c) = Eigen::pow(left(0, c),right.col(c).array());
        }
    }
};

template<int R>
struct DotPow<R, 1> {
    void calculate(Matrix<double, R, 1> left, 
        Matrix<double, R, 1> right, 
        Matrix<double, R, 1>* output) {
        *output = left.array().pow(right.array());
    }
    void calculate(Matrix<double, R, 1> left, 
        Matrix<double, 1, 1> right, 
        Matrix<double, R, 1>* output) {
        *output = left.array().pow(right(0,0));
    }
    void calculate(Matrix<double, 1, 1> left, 
        Matrix<double, R, 1> right, 
        Matrix<double, R, 1>* output) {
        *output = Eigen::pow(left(0,0), right.array());
    }
};

template<int C>
struct DotPow<1, C> {
    void calculate(Matrix<double, 1, C> left, 
        Matrix<double, 1, C> right, 
        Matrix<double, 1, C>* output) {
        *output = left.array().pow(right.array());
    }
    void calculate(Matrix<double, 1, C> left, 
        Matrix<double, 1, 1> right, 
        Matrix<double, 1, C>* output) {
        *output = left.array().pow(right(0,0));
    }
    void calculate(Matrix<double, 1, 1> left, 
        Matrix<double, 1, C> right, 
        Matrix<double, 1, C>* output) {
        *output = Eigen::pow(left(0,0), right.array());
    }
};

template<>
struct DotPow<1, 1> {
    void calculate(Matrix<double, 1, 1> left, 
        Matrix<double, 1, 1> right, 
        Matrix<double, 1, 1>* output) {
        *output = left.array().pow(right.array());
    }
};


template<int R, int C>
struct Abs {
    void calculate(Matrix<double, R, C> input,
        Matrix<double, R, C>* output) {
        *output = input.array().abs();
    }
};

#define Input Node

#define Output Node

template<int R, int C>
struct Memory {
    Matrix<double, R, C> stack;
    void calculate(Matrix<double, R, C> input,
        Matrix<double, R, C>* output) {
        *output = stack;
    }
    void refresh() {
        stack = current;
    }
    void input_update(Matrix<double, R, C> input) {
        current = input;
    }
private:
    Matrix<double, R, C> current;
};

struct Clock {
    context* ctx;
    void calculate(Matrix<double, 1, 1>* output) {
        *output = Matrix<double, 1, 1>::Ones()*(ctx->t);
    }
};

template<int R, int C>
struct Deadzone {
    double left;
    double right;
    void calculate(Matrix<double, R, C> input,
        Matrix<double, R, C>* output) {
        *output = input.unaryExpr([this](double item) -> double {
            if (item < this->left) return item - this->left;
            else if (item > this->right) return item - this->right;
            else return 0;
        });
    }
};

template<int R, int C>
struct Saturation {
    double upper;
    double lower;
    void calculate(Matrix<double, R, C> input,
        Matrix<double, R, C>* output) {
        *output = input.unaryExpr([this](double item) -> double {
            if (item < this->lower) return this->lower;
            else if (item > this->upper) return this->upper;
            else return item;
        });
    }
};

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
            output->row(r) = y.row(r).binaryExpr(ones.row(r)*x(r, 0), std::ptr_fun(::atan2));
        }
    }
    void calculate(Matrix<double, R, 1> y, 
        Matrix<double, R, C> x, 
        Matrix<double, R, C>* output) {
        auto ones = Matrix<double, R, C>::Ones();
        for (int r=0; r<R; r++) {
            output->row(r) = (ones.row(r)*y(r, 0)).binaryExpr(x.row(r), std::ptr_fun(::atan2));
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

template<int S>
struct Pow {
    int power;
    void calculate(Matrix<double, S, S> input,
        Matrix<double, S, S>* output) {
        *output = input.pow(power);
    }
};

template<int S>
struct Pow2 {
    void calculate(Matrix<double, S, S> input,
        Matrix<double, 1, 1> power,
        Matrix<double, S, S>* output) {
        *output = input.pow(power(0, 0));
    }
};

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

template<int R, int C>
struct Log2 {
    void calculate(Matrix<double, R, C> x,
        Matrix<double, R, C> base,
        Matrix<double, R, C>* output) {
        *output = x.binaryExpr(base, [](double xi, double bi) {
            return std::log(xi)/std::log(bi);
        });
    }
    void calculate(Matrix<double, R, C> x, 
        Matrix<double, 1, 1> base, 
        Matrix<double, R, C>* output) {
        auto b_val = Matrix<double, R, C>::Ones()*base(0, 0);
        *output = x.binaryExpr(b_val, [](double xi, double bi) {
            return std::log(xi)/std::log(bi);
        });
    }
    void calculate(Matrix<double, 1, 1> x, 
        Matrix<double, R, C> base, 
        Matrix<double, R, C>* output) {
        auto x_val = Matrix<double, R, C>::Ones()*x(0, 0);
        *output = x_val.binaryExpr(base, [](double xi, double bi) {
            return std::log(xi)/std::log(bi);
        });
    }
    void calculate(Matrix<double, R, C> x, 
        Matrix<double, R, 1> base, 
        Matrix<double, R, C>* output) {
        auto ones = Matrix<double, R, C>::Ones();
        for (int r=0; r<R; r++) {
            output->row(r) = x.row(r).binaryExpr(ones.row(r)*base(r, 0), [](double xi, double bi) {
                return std::log(xi)/std::log(bi);
            });
        }
    }
    void calculate(Matrix<double, R, 1> x, 
        Matrix<double, R, C> base, 
        Matrix<double, R, C>* output) {
        auto ones = Matrix<double, R, C>::Ones();
        for (int r=0; r<R; r++) {
            output->row(r) = (ones.row(r)*x(r, 0)).binaryExpr(base.row(r), [](double xi, double bi) {
                return std::log(xi)/std::log(bi);
            });
        }
    }
    void calculate(Matrix<double, R, C> x, 
        Matrix<double, 1, C> base, 
        Matrix<double, R, C>* output) {
        auto ones = Matrix<double, R, C>::Ones();
        for (int c=0; c<C; c++) {
            output->col(c) = x.col(c).binaryExpr(ones.col(c)*base(0, c), [](double xi, double bi) {
                return std::log(xi)/std::log(bi);
            });
        }
    }
    void calculate(Matrix<double, 1, C> x, 
        Matrix<double, R, C> base, 
        Matrix<double, R, C>* output) {
        auto ones = Matrix<double, R, C>::Ones();
        for (int c=0; c<C; c++) {
            output->col(c) = (ones.col(c)*x(0, c)).binaryExpr(base.col(c), [](double xi, double bi) {
                return std::log(xi)/std::log(bi);
            });
        }
    }
};


template<int R>
struct Log2<R, 1> {
    void calculate(Matrix<double, R, 1> x,
        Matrix<double, R, 1> base,
        Matrix<double, R, 1>* output) {
        *output = x.binaryExpr(base, [](double xi, double bi) {
            return std::log(xi)/std::log(bi);
        });
    }
    void calculate(Matrix<double, R, 1> x, 
        Matrix<double, 1, 1> base, 
        Matrix<double, R, 1>* output) {
        auto b_val = Matrix<double, R, 1>::Ones()*base(0, 0);
        *output = x.binaryExpr(b_val, [](double xi, double bi) {
            return std::log(xi)/std::log(bi);
        });
    }
    void calculate(Matrix<double, 1, 1> x, 
        Matrix<double, R, 1> base, 
        Matrix<double, R, 1>* output) {
        auto x_val = Matrix<double, R, 1>::Ones()*x(0, 0);
        *output = x_val.binaryExpr(base, [](double xi, double bi) {
            return std::log(xi)/std::log(bi);
        });
    }
};


template<int C>
struct Log2<1, C> {
    void calculate(Matrix<double, 1, C> x,
        Matrix<double, 1, C> base,
        Matrix<double, 1, C>* output) {
        *output = x.binaryExpr(base, [](double xi, double bi) {
            return std::log(xi)/std::log(bi);
        });
    }
    void calculate(Matrix<double, 1, C> x, 
        Matrix<double, 1, 1> base, 
        Matrix<double, 1, C>* output) {
        auto b_val = Matrix<double, 1, C>::Ones()*base(0, 0);
        *output = x.binaryExpr(b_val, [](double xi, double bi) {
            return std::log(xi)/std::log(bi);
        });
    }
    void calculate(Matrix<double, 1, 1> x, 
        Matrix<double, 1, C> base, 
        Matrix<double, 1, C>* output) {
        auto x_val = Matrix<double, 1, C>::Ones()*x(0, 0);
        *output = x_val.binaryExpr(base, [](double xi, double bi) {
            return std::log(xi)/std::log(bi);
        });
    }
};

template<>
struct Log2<1, 1> {
    void calculate(Matrix<double, 1, 1> x,
        Matrix<double, 1, 1> base,
        Matrix<double, 1, 1>* output) {
        *output = x.binaryExpr(base, [](double xi, double bi) {
            return std::log(xi)/std::log(bi);
        });
    }
};

template<int R, int C>
struct Switch {
    double threshold;
    void calculate(Matrix<double, R, C> condition,
        Matrix<double, R, C> input1,
        Matrix<double, R, C> input2,
        Matrix<double, R, C>* output) {
        Matrix<double, R, C> distribution = condition.unaryExpr([this](double c) -> double {
            return c >= this->threshold ? 1 : 0;
        });
        *output = input1.array()*distribution.array()+input2.array()*(1-distribution.array());
    }
    void calculate(Matrix<double, R, C> condition, 
        Matrix<double, 1, 1> input1,  
        Matrix<double, 1, 1> input2, 
        Matrix<double, R, C>* output) {
        Matrix<double, R, C> distribution = condition.unaryExpr([this](double c) -> double {
            return c >= this->threshold ? 1 : 0;
        });
        *output = input1(0,0)*distribution.array()+input2(0,0)*(1-distribution.array());
    }
    void calculate(Matrix<double, 1, 1> condition, 
        Matrix<double, R, C> input1, 
        Matrix<double, R, C> input2, 
        Matrix<double, R, C>* output) {
        double distribution = condition(0,0)>=threshold ? 1 : 0;
        *output = input1.array()*distribution+input2.array()*(1-distribution);
    }
    void calculate(Matrix<double, R, C> condition, 
        Matrix<double, R, 1> input1, 
        Matrix<double, R, 1> input2, 
        Matrix<double, R, C>* output) {
        Matrix<double, R, C> distribution = condition.unaryExpr([this](double c) -> double {
            return c >= this->threshold ? 1 : 0;
        });
        for (int r=0; r<R; r++) {
            output->row(r) = input1(r,0)*distribution.row(r)+input2(r,0)*(Matrix<double, 1, C>::Ones()-distribution.row(r));
        }
    }
    void calculate(Matrix<double, R, 1> condition, 
        Matrix<double, R, C> input1, 
        Matrix<double, R, C> input2, 
        Matrix<double, R, C>* output) {
        Matrix<double, R, 1> distribution = condition.unaryExpr([this](double c) -> double {
            return c >= this->threshold ? 1 : 0;
        });
        for (int r=0; r<R; r++) {
            output->row(r) = input1.row(r)*distribution(r,0)+input2.row(r)*(1-distribution(r,0));
        }
    }
    void calculate(Matrix<double, R, C> condition, 
        Matrix<double, 1, C> input1, 
        Matrix<double, 1, C> input2, 
        Matrix<double, R, C>* output) {
        Matrix<double, R, C> distribution = condition.unaryExpr([this](double c) -> double {
            return c >= this->threshold ? 1 : 0;
        });
        for (int c=0; c<C; c++) {
            output->col(c) = input1(0,c)*distribution.col(c)+input2(0,c)*(Matrix<double, R, 1>::Ones()-distribution.col(c));
        }
    }
    void calculate(Matrix<double, 1, C> condition, 
        Matrix<double, R, C> input1, 
        Matrix<double, R, C> input2, 
        Matrix<double, R, C>* output) {
        Matrix<double, 1, C> distribution = condition.unaryExpr([this](double c) -> double {
            return c >= this->threshold ? 1 : 0;
        });
        for (int c=0; c<C; c++) {
            output->col(c) = input1.col(c)*distribution(0,c)+input2.col(c)*(1-distribution(0,c));
        }
    }
};

template<int R>
struct Switch<R, 1> {
    double threshold;
    void calculate(Matrix<double, R, 1> condition,
        Matrix<double, R, 1> input1,
        Matrix<double, R, 1> input2,
        Matrix<double, R, 1>* output) {
        Matrix<double, R, 1> distribution = condition.unaryExpr([this](double c) -> double {
            return c >= this->threshold ? 1 : 0;
        });
        *output = input1.array()*distribution.array()+input2.array()*(1-distribution.array());
    }
    void calculate(Matrix<double, R, 1> condition, 
        Matrix<double, 1, 1> input1,  
        Matrix<double, 1, 1> input2, 
        Matrix<double, R, 1>* output) {
        Matrix<double, R, 1> distribution = condition.unaryExpr([this](double c) -> double {
            return c >= this->threshold ? 1 : 0;
        });
        *output = input1(0,0)*distribution.array()+input2(0,0)*(1-distribution.array());
    }
    void calculate(Matrix<double, 1, 1> condition, 
        Matrix<double, R, 1> input1, 
        Matrix<double, R, 1> input2, 
        Matrix<double, R, 1>* output) {
        double distribution = condition(0,0)>=threshold ? 1 : 0;
        *output = input1.array()*distribution+input2.array()*(1-distribution);
    }
};

template<int C>
struct Switch<1, C> {
    double threshold;
    void calculate(Matrix<double, 1, C> condition,
        Matrix<double, 1, C> input1,
        Matrix<double, 1, C> input2,
        Matrix<double, 1, C>* output) {
        Matrix<double, 1, C> distribution = condition.unaryExpr([this](double c) -> double {
            return c >= this->threshold ? 1 : 0;
        });
        *output = input1.array()*distribution.array()+input2.array()*(1-distribution.array());
    }
    void calculate(Matrix<double, 1, C> condition, 
        Matrix<double, 1, 1> input1,  
        Matrix<double, 1, 1> input2, 
        Matrix<double, 1, C>* output) {
        Matrix<double, 1, C> distribution = condition.unaryExpr([this](double c) -> double {
            return c >= this->threshold ? 1 : 0;
        });
        *output = input1(0,0)*distribution.array()+input2(0,0)*(1-distribution.array());
    }
    void calculate(Matrix<double, 1, 1> condition, 
        Matrix<double, 1, C> input1, 
        Matrix<double, 1, C> input2, 
        Matrix<double, 1, C>* output) {
        double distribution = condition(0,0)>=threshold ? 1 : 0;
        *output = input1.array()*distribution+input2.array()*(1-distribution);
    }
};

template<>
struct Switch<1, 1> {
    double threshold;
    void calculate(Matrix<double, 1, 1> condition, 
        Matrix<double, 1, 1> input1, 
        Matrix<double, 1, 1> input2, 
        Matrix<double, 1, 1>* output) {
        double distribution = condition(0,0)>=threshold ? 1 : 0;
        *output << input1(0,0)*distribution+input2(0,0)*(1-distribution);
    }
};



template<int R, int C>
struct Max {
    void calculate(Matrix<double, R, C> x,
        Matrix<double, R, C> y,
        Matrix<double, R, C>* output) {
        *output = x.binaryExpr(y, [](double xi, double yi) {
            return xi>yi?xi:yi;
        });
    }
    void calculate(Matrix<double, R, C> x, 
        Matrix<double, 1, 1> y, 
        Matrix<double, R, C>* output) {
        *output = x.unaryExpr([y](double xi) {
            return xi>y(0,0)?xi:y(0,0);
        });
    }
    void calculate(Matrix<double, 1, 1> x, 
        Matrix<double, R, C> y, 
        Matrix<double, R, C>* output) {
        *output = y.unaryExpr([x](double yi) {
            return x(0,0)>yi?x(0,0):yi;
        });
    }
    void calculate(Matrix<double, R, C> x, 
        Matrix<double, R, 1> y, 
        Matrix<double, R, C>* output) {
        for (int r=0; r<R; r++) {
            output->row(r) = x.row(r).unaryExpr([y, r](double xi) {
                return xi>y(r,0)?xi:y(r,0);
            });
        }
    }
    void calculate(Matrix<double, R, 1> x, 
        Matrix<double, R, C> y, 
        Matrix<double, R, C>* output) {
        for (int r=0; r<R; r++) {
            output->row(r) = y.row(r).unaryExpr([x, r](double yi) {
                return x(r,0)>yi?x(r,0):yi;
            });
        }
    }
    void calculate(Matrix<double, R, C> x, 
        Matrix<double, 1, C> y, 
        Matrix<double, R, C>* output) {
        for (int c=0; c<C; c++) {
            output->col(c) = x.col(c).unaryExpr([y, c](double xi) {
                return xi>y(0,c)?xi:y(0,c);
            });
        }
    }
    void calculate(Matrix<double, 1, C> x, 
        Matrix<double, R, C> y, 
        Matrix<double, R, C>* output) {
        for (int c=0; c<C; c++) {
            output->col(c) = y.col(c).unaryExpr([x, c](double yi) {
                return x(0,c)>yi?x(0,c):yi;
            });
        }
    }
};

template<int R>
struct Max<R, 1> {
    void calculate(Matrix<double, R, 1> x,
        Matrix<double, R, 1> y,
        Matrix<double, R, 1>* output) {
        *output = x.binaryExpr(y, [](double xi, double yi) {
            return xi>yi?xi:yi;
        });
    }
    void calculate(Matrix<double, R, 1> x, 
        Matrix<double, 1, 1> y, 
        Matrix<double, R, 1>* output) {
        *output = x.unaryExpr([y](double xi) {
            return xi>y(0,0)?xi:y(0,0);
        });
    }
    void calculate(Matrix<double, 1, 1> x, 
        Matrix<double, R, 1> y, 
        Matrix<double, R, 1>* output) {
        *output = y.unaryExpr([x](double yi) {
            return x(0,0)>yi?x(0,0):yi;
        });
    }
};

template<int C>
struct Max<1, C> {
    void calculate(Matrix<double, 1, C> x,
        Matrix<double, 1, C> y,
        Matrix<double, 1, C>* output) {
        *output = x.binaryExpr(y, [](double xi, double yi) {
            return xi>yi?xi:yi;
        });
    }
    void calculate(Matrix<double, 1, C> x, 
        Matrix<double, 1, 1> y, 
        Matrix<double, 1, C>* output) {
        *output = x.unaryExpr([y](double xi) {
            return xi>y(0,0)?xi:y(0,0);
        });
    }
    void calculate(Matrix<double, 1, 1> x, 
        Matrix<double, 1, C> y, 
        Matrix<double, 1, C>* output) {
        *output = y.unaryExpr([x](double yi) {
            return x(0,0)>yi?x(0,0):yi;
        });
    }
};

template<>
struct Max<1, 1> {
    void calculate(Matrix<double, 1, 1> x,
        Matrix<double, 1, 1> y,
        Matrix<double, 1, 1>* output) {
        *output << (x(0,0)>y(0,0)?x(0,0):y(0,0));
    }
};

template<int R, int C>
struct Min {
    void calculate(Matrix<double, R, C> x,
        Matrix<double, R, C> y,
        Matrix<double, R, C>* output) {
        *output = x.binaryExpr(y, [](double xi, double yi) {
            return xi<yi?xi:yi;
        });
    }
    void calculate(Matrix<double, R, C> x, 
        Matrix<double, 1, 1> y, 
        Matrix<double, R, C>* output) {
        *output = x.unaryExpr([y](double xi) {
            return xi<y(0,0)?xi:y(0,0);
        });
    }
    void calculate(Matrix<double, 1, 1> x, 
        Matrix<double, R, C> y, 
        Matrix<double, R, C>* output) {
        *output = y.unaryExpr([x](double yi) {
            return x(0,0)<yi?x(0,0):yi;
        });
    }
    void calculate(Matrix<double, R, C> x, 
        Matrix<double, R, 1> y, 
        Matrix<double, R, C>* output) {
        for (int r=0; r<R; r++) {
            output->row(r) = x.row(r).unaryExpr([y, r](double xi) {
                return xi<y(r,0)?xi:y(r,0);
            });
        }
    }
    void calculate(Matrix<double, R, 1> x, 
        Matrix<double, R, C> y, 
        Matrix<double, R, C>* output) {
        for (int r=0; r<R; r++) {
            output->row(r) = y.row(r).unaryExpr([x, r](double yi) {
                return x(r,0)<yi?x(r,0):yi;
            });
        }
    }
    void calculate(Matrix<double, R, C> x, 
        Matrix<double, 1, C> y, 
        Matrix<double, R, C>* output) {
        for (int c=0; c<C; c++) {
            output->col(c) = x.col(c).unaryExpr([y, c](double xi) {
                return xi<y(0,c)?xi:y(0,c);
            });
        }
    }
    void calculate(Matrix<double, 1, C> x, 
        Matrix<double, R, C> y, 
        Matrix<double, R, C>* output) {
        for (int c=0; c<C; c++) {
            output->col(c) = y.col(c).unaryExpr([x, c](double yi) {
                return x(0,c)<yi?x(0,c):yi;
            });
        }
    }
};

template<int R>
struct Min<R, 1> {
    void calculate(Matrix<double, R, 1> x,
        Matrix<double, R, 1> y,
        Matrix<double, R, 1>* output) {
        *output = x.binaryExpr(y, [](double xi, double yi) {
            return xi>yi?xi:yi;
        });
    }
    void calculate(Matrix<double, R, 1> x, 
        Matrix<double, 1, 1> y, 
        Matrix<double, R, 1>* output) {
        *output = x.unaryExpr([y](double xi) {
            return xi<y(0,0)?xi:y(0,0);
        });
    }
    void calculate(Matrix<double, 1, 1> x, 
        Matrix<double, R, 1> y, 
        Matrix<double, R, 1>* output) {
        *output = y.unaryExpr([x](double yi) {
            return x(0,0)<yi?x(0,0):yi;
        });
    }
};

template<int C>
struct Min<1, C> {
    void calculate(Matrix<double, 1, C> x,
        Matrix<double, 1, C> y,
        Matrix<double, 1, C>* output) {
        *output = x.binaryExpr(y, [](double xi, double yi) {
            return xi<yi?xi:yi;
        });
    }
    void calculate(Matrix<double, 1, C> x, 
        Matrix<double, 1, 1> y, 
        Matrix<double, 1, C>* output) {
        *output = x.unaryExpr([y](double xi) {
            return xi<y(0,0)?xi:y(0,0);
        });
    }
    void calculate(Matrix<double, 1, 1> x, 
        Matrix<double, 1, C> y, 
        Matrix<double, 1, C>* output) {
        *output = y.unaryExpr([x](double yi) {
            return x(0,0)<yi?x(0,0):yi;
        });
    }
};

template<>
struct Min<1, 1> {
    void calculate(Matrix<double, 1, 1> x,
        Matrix<double, 1, 1> y,
        Matrix<double, 1, 1>* output) {
        *output << (x(0,0)<y(0,0)?x(0,0):y(0,0));
    }
};

template<int R, int C>
struct Rand {
    void calculate(Matrix<double, R, C>* output) {
        *output = value;
    }
    void refresh() {
        value = Matrix<double, R, C>::Random().array().abs();
    }
private:
    Matrix<double, R, C> value;
};

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

template<int R, int C>
struct Split {
    template<typename ...T>
    void calculate(Matrix<double, R, C> input,
        Matrix<T, 1, 1>*... output) {
        int i=0;
        getItem(input, &i, output...);
    }
private:
    template<typename T0, typename ...T>
    void getItem(Matrix<double, R, C> input, int*i, T0* item0, T*...items) {
        *item0 << input((*i)/input.rows(), (*i)%input.rows());
        *i = *i+1;
        getItem(input, i, items...);
    }
    void getItem(Matrix<double, R, C>input, int*i) {}
};

template<int R, int C>
struct VSplit {
    template<int ...iR>
    void calculate(Matrix<double, R, C> input,
        Matrix<double, iR, C>*...output) {
        int r=0;
        getRows(input, &r, output...);
    }
private:
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
private:
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

/* not implemented
template<int R, int C>
struct Equal {};

template<int R, int C>
struct EqualToZero {};

template<int R, int C>
struct Variable {};
*/

template<int R, int C, int L>
struct Interpolation {
    Matrix<double, L, 1> points;
    Matrix<double, L, R*C> table;
    void calculate(Matrix<double, 1, 1> input,
        Matrix<double, R, C>* output) {
        int i=0;
        if (points(i,0) >= input(0, 0)) {
            *output = table.row(i).reshaped(R, C);
            return;
        }
        while (points(++i,0) <= input(0, 0)) {
            if (points(i,0) == input(0, 0) || i+1 >= L) {
                *output = table.row(i).reshaped(R, C);
                return;
            }
        }
        double ratio = (input(0, 0)-points(i-1,0))/(points(i,0)-points(i-1,0));
        *output = (table.row(i-1)*(1-ratio)+table.row(i)*ratio).reshaped(R, C);
    }
};

struct UDPListener {
    int index;
    context* ctx;
    template<typename ...T>
    void calculate(T*...output) {
        i = index;
        setInput(output...);
    }
private:
    int i;
    template<int R, int C, typename...T>
    void setInput(Matrix<double, R, C>* input0, T*...inputs) {
        for (int r=0; r<R; ++r) {
            for (int c=0; c<C; ++c) {
                (*input0)(r, c) = ctx->u(index+r*C+c);
            }
        }
        i += R*C;
        setInput(inputs...);
    }
    void setInput(){}
};

struct UDPSender {
    int index;
    context* ctx;
    template<typename ...T>
    void calculate(T... input) {
        i = index;
        setOutput(input...);
    }
private:
    int i;
    template<int R, int C, typename...T>
    void setOutput(Matrix<double, R, C> output0, T...outputs) {
        for (int r=0; r<R; ++r) {
            for (int c=0; c<C; ++c) {
                ctx->y(i+r*C+c) = output0(r, c);
            }
        }
        i += R*C;
        setOutput(outputs...);
    }
    void setOutput(){}
};

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

template<int R, int C>
struct Interpolation2D {
    Matrix<double, R, 1> rows;
    Matrix<double, 1, C> cols;
    Matrix<double, R, C> table;
    void calculate(Matrix<double, 1, 1> row,
        Matrix<double, 1, 1> col,
        Matrix<double, 1, 1>* output) {
        double rowRatio = 0;
        int r = 0;
        if (rows(r,0) >= row(0, 0)) {
            rowRatio = 1;
        }
        else while (rows(++r,0) <= row(0, 0)) {
            if (rows(r,0) == row(0, 0) || r+1 >= R) {
                rowRatio = 1;
                break;
            }
        }
        if (rowRatio == 0) {
            rowRatio = (row(0, 0)-rows(r-1,0))/(rows(r,0)-rows(r-1,0));
        }
        double colRatio = 0;
        int c = 0;
        if (cols(0,c) >= col(0, 0)) {
            colRatio = 1;
        }
        else while (cols(0,++c) <= col(0, 0)) {
            if (cols(0,c) == col(0, 0) || c+1 >= C) {
                colRatio = 1;
                break;
            }
        }
        if (colRatio == 0) {
            colRatio = (col(0, 0)-cols(0,c-1))/(cols(0,c)-cols(0,c-1));
        }
        double left = table(r-1, c-1)*(1-rowRatio)+table(r, c-1)*rowRatio;
        double right = table(r-1, c)*(1-rowRatio)+table(r, c)*rowRatio;
        *output << left*(1-colRatio)+right*colRatio;
    }
};

template<int D, int S, int R, int C>
struct InterpolationHD {
    Matrix<double, R, C> table;
    Matrix<double, D, S> axes;
    Matrix<int, 1, D> dim;
    Matrix<int, 1, D-1> indices;
    void calculate(Matrix<double, 1, D> value,
        Matrix<double, 1, 1>* output) {
        Matrix<int, 1, D> leftIndex;
        Matrix<double, 1, D> ratio;
        for (int i=0; i<D; i++) {
            int j=0;
            if (axes(i, 0) >= value(0, i));
            else while (true) {
                if (j>=dim(0, i)-2) break;
                else if (axes(i, j+1) > value(0, i)) break;
                else j++;
            }
            leftIndex(0, i) = j;
            ratio(0, i) = (value(0, i) - axes(i, j))/(axes(i, j+1) - axes(i, j));
        }
        double val = 0;
        for (int k=0; k<(1<<D); k++) {
            double c = 1;
            Matrix<int, 1, D> ind;
            for (int x=0; x<D; x++) {
                if (((k>>x)&1) == 0) {
                    c *= ratio(0, x);
                    ind(0, x) = leftIndex(0, x)+1;
                } else {
                    c *= 1-ratio(0, x);
                    ind(0, x) = leftIndex(0, x);
                }
            }
            val += c*_table(ind);
        }
        *output << val;
    }
private:
    double _table(Matrix<int, 1, D> ind) {
        int c = 0;
        for (int i=0; i<D-1; i++) {
            c += ind(0, i)*indices(0, i);
        }
        return table(c, ind(0, D-1));
    }
};

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
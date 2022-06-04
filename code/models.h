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
        *output = Matrix<double, 1, 1>::Ones()*level*((ctx->t)>start);
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

// template<int R, int C>
// struct BoundedIntegrator {};

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

// template<int R, int C>
// struct PhaseDiagram {};

// template<int R, int C>
// struct Print {};
#define Print Scope

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


template<int R, int C, int type>
struct Sub {
    void calculate(Matrix<double, R, C> left, 
        Matrix<double, R, C> right, 
        Matrix<double, R, C>* output) {
        *output = left - right;
    }
};

template<int R, int C>
struct Sub<R, C, BROADCAST_RC11> {
    void calculate(Matrix<double, R, C> left, 
        Matrix<double, 1, 1> right, 
        Matrix<double, R, C>* output) {
        auto right_val = Matrix<double, R, C>::Ones()*right(0, 0);
        *output = left - right_val;
    }
};

template<int R, int C>
struct Sub<R, C, BROADCAST_11RC> {
    void calculate(Matrix<double, 1, 1> left, 
        Matrix<double, R, C> right, 
        Matrix<double, R, C>* output) {
        auto left_val = Matrix<double, R, C>::Ones()*left(0, 0);
        *output = left_val - right;
    }
};

template<int R, int C>
struct Sub<R, C, BROADCAST_RCR1> {
    void calculate(Matrix<double, R, C> left, 
        Matrix<double, R, 1> right, 
        Matrix<double, R, C>* output) {
        auto ones = Matrix<double, R, C>::Ones();
        for (int r=0; r<R; r++) {
            output->row(r) = left.row(r) - ones.row(r)*right(r, 0);
        }
    }
};

template<int R, int C>
struct Sub<R, C, BROADCAST_R1RC> {
    void calculate(Matrix<double, R, 1> left, 
        Matrix<double, R, C> right, 
        Matrix<double, R, C>* output) {
        auto ones = Matrix<double, R, C>::Ones();
        for (int r=0; r<R; r++) {
            output->row(r) = ones.row(r)*left(r, 0) - right.row(r);
        }
    }
};

template<int R, int C>
struct Sub<R, C, BROADCAST_RC1C> {
    void calculate(Matrix<double, R, C> left, 
        Matrix<double, 1, C> right, 
        Matrix<double, R, C>* output) {
        auto ones = Matrix<double, R, C>::Ones();
        for (int c=0; c<C; c++) {
            output->col(c) = left.col(c) - ones.col(c)*right(0, c);
        }
    }
};

template<int R, int C>
struct Sub<R, C, BROADCAST_1CRC> {
    void calculate(Matrix<double, 1, C> left, 
        Matrix<double, R, C> right, 
        Matrix<double, R, C>* output) {
        auto ones = Matrix<double, R, C>::Ones();
        for (int c=0; c<C; c++) {
            output->col(c) = ones.col(c)*left(0, c) - right.col(c);
        }
    }
};

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

template<int R, int C>
struct Abs {
    void calculate(Matrix<double, R, C> input,
        Matrix<double, R, C>* output) {
        *output = input.array().abs();
    }
};

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

// template<int R, int C>
// struct Atan2 {};

// template<int R, int C>
// struct Asinh {};

// template<int R, int C>
// struct Acosh {};

// template<int R, int C>
// struct Atanh {};

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

// template<int R, int C>
// struct Sign {};

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

// template<int R, int C>
// struct Terminal {};

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

// template<int R, int C>
// struct InputMarker {};

// template<int R, int C>
// struct OutputSink {};
#define OutputSink Scope

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

template<int R, int C>
struct Transpose {
    void calculate(Matrix<double, R, C> input,
        Matrix<double, R, C>* output) {
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


#endif
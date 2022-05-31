#ifndef PROJECT_H
#define PROJECT_H

#include "models.h"
struct inputs {
    
};

struct states {
    double x1;
    double x2;
    double x3;
    double x4;
    double x5;
    double x6;
};

struct outputs {
    
};


class project {
public:
    context ctx;
    inputs* up;
    states* xdotp;
    states* xp;
    outputs* yp;
// models initialization
    Constant<2,2> constant1;
    Constant<1,2> constant2;
    VStack<3,2,2,1> vstack1;
    Integrator<3,2> integrator1;
    void init();
    void step();
};

// configuration
void project::init() {
    up = (inputs*) &(ctx.u);
    xdotp = (states*) &(ctx.xdot);
    xp = (states*) &(ctx.x);
    yp = (outputs*) &(ctx.y);
    
    constant1.value << 1, 2, 3, 4;
    constant2.value << -1, -2;

    integrator1.ctx = &ctx;
    integrator1.initial << 0, 0, 0, 0, 0, 0;
    integrator1.index = 0;
    integrator1.config();
}

void project::step() {
    // temp variables definition
    Matrix<double, 3, 2> integrator_output;
    Matrix<double, 2, 2> constant1_value;
    Matrix<double, 1, 2> constant2_value;
    Matrix<double, 3, 2> vstack1_output;
    
    // main calculation
    constant1.calculate(&constant1_value);
    constant2.calculate(&constant2_value);
    vstack1.calculate(constant1_value, constant2_value, &vstack1_output);
    integrator1.calculate(vstack1_output, &integrator_output);
    
    // loop
    integrator1.input_update(vstack1_output);
}

#endif
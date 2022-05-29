#ifndef PROJECT_H
#define PROJECT_H

#include "models.h"

class project {
public:
    context ctx;
// models initialization
    Constant<2,2> constant1;
    Constant<1,2> constant2;
    Add<2,2,6> add1;
    Integrator<2,2,4> integrator1;
    void init();
    void step();
};

// configuration
void project::init() {
    constant1.value << 1, 2, 3, 4;
    constant2.value << -1, -2;

    integrator1.ctx = &ctx;
    integrator1.initial << 0, 0, 0, 0;
    integrator1.index = 0;
    integrator1.config();
}

void project::step() {
    // temp variables definition
    Matrix<double, 2, 2> integrator_output;
    Matrix<double, 2, 2> constant1_value;
    Matrix<double, 1, 2> constant2_value;
    Matrix<double, 2, 2> add1_output;
    
    // main calculation
    constant1.calculate(&constant1_value);
    constant2.calculate(&constant2_value);
    add1.calculate(constant2_value, constant1_value, &add1_output);
    integrator1.calculate(add1_output, &integrator_output);
    
    // loop
    integrator1.input_update(add1_output);
}

#endif
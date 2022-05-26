#ifndef PROJECT_H
#define PROJECT_H

#include "models.h"

class project {
public:
    context ctx;
// models initialization
    constant<2,1> constant1;
    integrator<2,1,2> integrator1;
    void init();
    void step();
};

// configuration
void project::init() {
    constant1.value << 1, -2;

    integrator1.ctx = &ctx;
    integrator1.initial << 0, 0;
    integrator1.index = 0;
    integrator1.config();
}

void project::step() {
    // temp variables definition
    Matrix<double, 2, 1> integrator_output;
    Matrix<double, 2, 1> constant1_value;
    
    // main calculation
    constant1.calculate(&constant1_value);
    integrator1.calculate(constant1_value, &integrator_output);
    
    // loop
    integrator1.input_update(constant1_value);
}

#endif
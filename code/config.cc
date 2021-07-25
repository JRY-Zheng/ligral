#include "config.h"

Vector f(Vector x) {
    Vector xdot;

    // models initialization
    constant_struct<2,1> constant1;
    constant1.value << 1, -2;

    integrator_struct<2,1,2> integrator1;
    integrator1.initial << 0, 0;
    integrator1.states = &x;
    integrator1.derivatives = &xdot;
    integrator1.index = 0;

    // temp variables definition
    Matrix<double, 2, 1> integrator_output;
    Matrix<double, 2, 1> constant1_value;
    
    // main calculation
    constant1.calculate(&constant1_value);
    integrator1.calculate(constant1_value, &integrator_output);
    
    // loop
    integrator1.input_update(constant1_value);

    return xdot;
}
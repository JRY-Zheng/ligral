#include "config.h"

Vector f(Vector x) {
    // Eigen::MatrixXd A(2, 2);
    // A << 0, 1, -0.2, -0.5;
    // return A*x;
    Vector xdot;

    constant_struct<2,1> constant1;
    constant1.value << 1, -2;

    integrator_struct<2,1,2> integrator1;
    integrator1.initial << 0, 0;
    integrator1.states = &x;
    integrator1.derivatives = &xdot;
    integrator1.index = 0;

    Matrix<double, 2, 1> constant1_value;
    constant1.calculate(&constant1_value);
    
    Matrix<double, 2, 1> integrator_output;
    integrator1.calculate(constant1_value, &integrator_output);

    return xdot;
}
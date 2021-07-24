#include "solvers.h"

VectorXd euler_integral(VectorXd f(VectorXd), VectorXd x, double h) {
    return x+f(x)*h;
}
#include "config.h"

Vector f(Vector x) {
    Eigen::MatrixXd A(2, 2);
    A << 0, 1, -0.2, -0.5;
    return A*x;
}
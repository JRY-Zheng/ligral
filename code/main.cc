/* Copyright (C) 2019-2021 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

    Distributed under MIT license.
    See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

#include <iostream>
#include <Eigen/Dense>
using Eigen::MatrixXd;
using Eigen::VectorXd;

#define h 0.1;

VectorXd euler_integral(VectorXd x, VectorXd xdot) {
    return x+xdot*h;
}

int main() {
    MatrixXd A(2, 2);
    A << 0, 1, -0.2, -0.5;
    VectorXd x(2);
    x << 0, 1;
    for (int i=0; i<10; i++) {
        double t = ((double)i)*h;
        x = euler_integral(x, A*x);
        std::cout << "t = " << t << std::endl;
        std::cout << x << std::endl;
    }
    return 0;
}
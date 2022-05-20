/* Copyright (C) 2019-2021 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

    Distributed under MIT license.
    See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

#include <iostream>
#include <Eigen/Dense>
using Eigen::Matrix;

#include "step.h"

int main() {
    Matrix<double, n, 1> x;
    x << 0, 1;
    for (int i=0; i<10; i++) {
        double t = ((double)i)*h;
        x = integral(f, x, h);
        std::cout << "t = " << t << std::endl;
        std::cout << x << std::endl;
    }
    return 0;
}
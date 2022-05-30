/* Copyright (C) 2019-2021 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

    Distributed under MIT license.
    See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

#include <iostream>
#include <Eigen/Dense>
using Eigen::Matrix;

#include "project.h"

project p;

Vector(N) f(Vector(N) x) {
    p.ctx.x = x;
    p.step();
    return p.ctx.xdot;
}

int main() {
    p.init();
    for (int i=0; i<STEPS; i++) {
        p.ctx.t = ((double)i)*H;
        p.ctx.x = integral(f, p.ctx.x, H);
        std::cout << "t = " << p.ctx.t << std::endl;
        std::cout << "x = " << p.ctx.x.transpose() << std::endl;
        std::cout << "y = " << p.ctx.y.transpose() << std::endl;
    }
    return 0;
}
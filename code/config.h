#ifndef CONFIG_H
#define CONFIG_H

#include "solvers.h"
#include "context.h"

#define H 0.1
#define N 2
#define M 1
#define P 0
#define STEPS 10
#define integral euler_integral<N>
#define context struct context_struct<M, N, P>

#endif
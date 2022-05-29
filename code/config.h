#ifndef CONFIG_H
#define CONFIG_H

#include "solvers.h"
#include "context.h"

#define H 1
#define N 6
#define M 0
#define P 0
#define STEPS 1
#define integral euler_integral<N>
#define context struct context_struct<M, N, P>

#endif
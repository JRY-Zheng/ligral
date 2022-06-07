import numpy as np

def check_list(var):
    if len(var) == 0:
        raise Exception('Row number cannot be zero')
    if all(isinstance(row, list) for row in var):
        for row in var:
            for item in row:
                if not isinstance(item, float) and not isinstance(item, int):
                    raise Exception('Item should be number')
    elif all(isinstance(row, int) or isinstance(row, float) for row in var):
        return [var, ]
    colno = [len(row) for row in var]
    colmatch = [col==colno[0] for col in colno]
    if not all(colmatch):
        raise Exception('Column numbers are not matched')
    if colno[0] == 0:
        raise Exception('Column number cannot be zero')
    return var

def check_ndarray(var):
    if not np.issubdtype(var.dtype, np.integer) and \
        not np.issubdtype(var.dtype, np.floating):
        raise Exception('Item should be number')
    if len(var) == 0:
        raise Exception('Empty matrix is not supported')
    if len(var.shape) > 2:
        raise Exception('High dimension is not supported')
    elif len(var.shape) == 2:
        return [[*row] for row in var]
    elif len(var.shape) == 1:
        return [[*var], ]
    else:
        raise Exception('Invalid shape')

def get(name):
    pool = {**globals()}
    if name not in pool:
        print(name, 'not found')
        return
    val = pool[name]
    try:
        if isinstance(val, list):
            val = check_list(val)
        elif isinstance(val, np.matrix):
            val = check_ndarray(val.A)
        elif isinstance(val, np.ndarray):
            val = check_ndarray(val)
        else:
            print('type of', name, type(val), 'wrong type')
            return
    except Exception as e:
        print('error in', name, e)
    print(name, '=', val)

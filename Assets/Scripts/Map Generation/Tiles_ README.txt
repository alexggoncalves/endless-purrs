TILE IDENTIFIER:
    -1 => empty
    0 => grass
    1 => grass_L1
    2 => grass_L2
    3 => cliff
    4 => cliff_L1
    5 => grass->sand intersection sides
    6 => sand
    7 => sand_slope
    8 => water
    

SUFFIXES:
    s => symmetrical
    _symmetrical sockets fit when both have an "s"
    _ex. "0s", "4s"

    f => asymmetrical
    _asymmetrical sockets fit when one has an "f" and the other doesn't
    _ex. "3f" == "3", "5" == "5f"

PREFIXES:
    v => vertical
    _vertical sockets fit when both have the same rotation and tile identifiers [v_tile_rotation]
    _ex. "v_3_0", "v_3_1", "v_3_2", "v_3_4" (all rotations for tile 3)
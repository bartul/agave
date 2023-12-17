# Agave 

Agave seed, once planted, takes _x_ time to germinate, with success rate of _i_.

Agave once germinated, takes _y_ time to blossom, where it will produce _n_ seeds.

Seeds are then planted, and the cycle continues.

Soon after blossoming, agave dies.

Each new seed has a chance of being less successful than the previous generation by degeneration rate _d_, degrading success rate _i_ more by each generation.

## Example

> _n_ = 3
>
> _d_ = 0.1
>
> _x_ = 10s
>
> _y_ = 10s
>
> _i_ = 1

### Generation 1

2 seeds planted, 2 seed germinates after 10s, 6 seeds produced after 10s

### Generation 2

> _i_ = _i_ - _d_ = 0.9

4 seeds planted, 3 seed germinates after 10s, 9 seeds produced after 10s

### Generation 3

> _i_ = _i_ - _d_ = 0.8

9 seeds planted, 7 seed germinates after 10s, 21 seeds produced after 10s

### Generation 4

> _i_ = _i_ - _d_ = 0.7

21 seeds planted, 14 seed germinates after 10s, 42 seeds produced after 10s

### Generation 5

> _i_ = _i_ - _d_ = 0.6

42 seeds planted, 25 seed germinates after 10s, 75 seeds produced after 10s

### Generation 6

> _i_ = _i_ - _d_ = 0.5

75 seeds planted, 37 seed germinates after 10s, 111 seeds produced after 10s

### Generation 7

> _i_ = _i_ - _d_ = 0.4

111 seeds planted, 44 seed germinates after 10s, 132 seeds produced after 10s 

### Generation 8

> _i_ = _i_ - _d_ = 0.3

132 seeds planted, 39 seed germinates after 10s, 117 seeds produced after 10s

### Generation 9

> _i_ = _i_ - _d_ = 0.2

117 seeds planted, 23 seed germinates after 10s, 69 seeds produced after 10s

### Generation 10

> _i_ = _i_ - _d_ = 0.1

69 seeds planted, 6 seed germinates after 10s, 18 seeds produced after 10s

### Generation 11

> _i_ = _i_ - _d_ = 0.0

18 seeds planted, 0 seed germinates after 10s, 0 seeds produced after 10s
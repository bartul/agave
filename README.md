# Agave

Agave seed, once planted, takes _tg_ time to germinate, with success rate of _rs_. Otherwise, it dies.

Agave once germinated, takes _tb_ time to blossom, where it will produce _n_ seeds.

Seeds are then planted, and the cycle continues.

After blossoming, agave dies.

Each new seed generation has a lower success rate to germinate than the previous generation by degeneration rate of _rd_, degrading success rate of _rs_ more by each generation.

## Example

> _tg_ = 10s // time to germinate
>
> _tb_ = 10s // time to blossom
>
> _n_ = 3 // number of seeds produced after blossoming
>
> _rs_ = 1 // success rate (initial) - 100%
>
> _rd_ = 0.1 // degeneration rate - 10%

### Generation 1

2 seeds planted, 2 seed germinates after 10s, 6 seeds produced after 10s

### Generation 2

> _rs_ = _rs_ - _rd_ = 0.9

4 seeds planted, 3 seed germinates after 10s, 9 seeds produced after 10s

### Generation 3

> _rs_ = _rs_ - _rd_ = 0.8

9 seeds planted, 7 seed germinates after 10s, 21 seeds produced after 10s

### Generation 4

> _rs_ = _rs_ - _rd_ = 0.7

21 seeds planted, 14 seed germinates after 10s, 42 seeds produced after 10s

### Generation 5

> _rs_ = _rs_ - _rd_ = 0.6

42 seeds planted, 25 seed germinates after 10s, 75 seeds produced after 10s

### Generation 6

> _rs_ = _rs_ - _rd_ = 0.5

75 seeds planted, 37 seed germinates after 10s, 111 seeds produced after 10s

### Generation 7

> _rs_ = _rs_ - _rd_ = 0.4

111 seeds planted, 44 seed germinates after 10s, 132 seeds produced after 10s 

### Generation 8

> _rs_ = _rs_ - _rd_ = 0.3

132 seeds planted, 39 seed germinates after 10s, 117 seeds produced after 10s

### Generation 9

> _rs_ = _rs_ - _rd_ = 0.2

117 seeds planted, 23 seed germinates after 10s, 69 seeds produced after 10s

### Generation 10

> _rs_ = _rs_ - _rd_ = 0.1

69 seeds planted, 6 seed germinates after 10s, 18 seeds produced after 10s

### Generation 11

> _rs_ = _rs_ - _rd_ = 0.0

18 seeds planted, 0 seed germinates after 10s, 0 seeds produced after 10s
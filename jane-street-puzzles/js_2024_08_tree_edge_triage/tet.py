from sympy import symbols, init_printing, reduce_inequalities, solve_poly_inequality, Poly, QQ
from fractions import Fraction


def main() -> None:
    init_printing(use_unicode=True)
    p = symbols("p")
    expr = (2 * p**3 - p**4) ** 16 - Fraction(1, 1000000)
    print(f"expr={expr.as_poly()}")
    poly = expr.as_poly(domain="QQ")
    print(f"poly={poly}")
    # red = reduce_inequalities(QQ(0)<prob, [])
    # print(f"p={red}")
    sol = solve_poly_inequality(poly, ">")
    print(f"sol={sol}")
    vals = [0.0, 0.1, 0.25, 0.5, 0.75, 0.9, 0.95, 0.99, 1.0]
    for v in vals:
        val = expr.evalf(subs={p: v})
        print(f" prob({v}) = {val}")

    expr2 = 4 - 4 * p + p * p
    poly2 = expr2.as_poly(domain="QQ")
    sol2 = solve_poly_inequality(poly2, ">")
    print(f"sol2={sol2}")
    pass


if __name__ == "__main__":
    main()

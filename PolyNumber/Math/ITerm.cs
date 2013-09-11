namespace Math {
    /// <summary>
    /// A component of a polynomial equation, such as X or X^2 or X*Y or X*Y^2*Z.
    /// 
    /// (i.e. represents the basis vectors that make up a polynomial vector space)
    /// 
    /// Implementors must have a default value that corresponds to X^0.
    /// </summary>
    public interface ITerm<T> {
        T Times(T other);
    }
}

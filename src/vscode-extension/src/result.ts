/**
 * The result of some operation.
 */
export type Result<T, E extends Error = Error> = {
    /**
     * Specifies whether the result is ok or an error.
     */
    kind: "ok",
    /**
     * The ok value of the result.
     */
    value: T
} | {
    /**
     * Specifies whether the result is ok or an error.
     */
    kind: "err",
    /**
     * The error of the result.
     */
    error: E
}

/**
 * Creates a result containing an ok value.
 * @param value The value of the result.
 */
export function ok<T, E extends Error>(value: T): Result<T, E> {
    return {
        kind: "ok",
        value
    };
}

/**
 * Creates a result containing an error.
 * @param error The error of the result.
 */
export function err<T, E extends Error>(error: E): Result<T, E> {
    return {
        kind: "err",
        error
    };
}

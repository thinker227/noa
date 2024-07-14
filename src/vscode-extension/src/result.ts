export type Result<T, E extends Error = Error> = {
    kind: "ok",
    value: T
} | {
    kind: "err",
    error: E
}

export function ok<T, E extends Error>(value: T): Result<T, E> {
    return {
        kind: "ok",
        value
    };
}

export function err<T, E extends Error>(error: E): Result<T, E> {
    return {
        kind: "err",
        error
    };
}

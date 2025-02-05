import { GlobalEnvironmentVariableCollection } from "vscode";

let variables: GlobalEnvironmentVariableCollection;

/**
 * Initializes the environment variable support for the extension.
 * @param vars The environment variable collection.
 */
export function initializeVariables(vars: GlobalEnvironmentVariableCollection) {
    variables = vars;
}

/**
 * Gets the `NOA_RUNTIME` environment variable.
 */
export function getNoaRuntimeVariable(): string | undefined {
    return variables.get("NOA_RUNTIME")?.value;
}

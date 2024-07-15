import { WorkspaceConfiguration, workspace } from "vscode";

/**
 * Gets the extension configuration.
 */
export function getConfig(): WorkspaceConfiguration {
    return workspace.getConfiguration("noa-lang");
}

/**
 * Gets the CLI command used by the extension.
 */
export function getCliCommand(): string {
    return getConfig().get("cliExecutable") || "noa";
}

/**
 * Gets the logging path used by the language server.
 */
export function getServerLogPath(): string | undefined {
    return getConfig().get("serverLogPath") || undefined;
}

/**
 * Gets the log level used by the language server.
 */
export function getLogLevel(): string {
    return getConfig().get("logLevel") || "info";
}

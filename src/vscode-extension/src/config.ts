import { ConfigurationTarget, WorkspaceConfiguration, window, workspace } from "vscode";

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
 * Gets the runtime executable path used by the extension.
 */
export function getRuntimePath(): string | undefined {
    return getConfig().get("runtimeExecutable");
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

/**
 * Updates the runtime executable path configuration interactively.
 * @returns The new value of the runtime executable, or undefined if canceled.
 */
export async function updateRuntimePathInteractively(): Promise<string | undefined> {
    return await updateInteractively(
        "runtimeExecutable",
        "Select Noa runtime executable",
        "Select",
        async (title, prompt) => {
            let res = await window.showOpenDialog({
                title,
                openLabel: prompt,
                canSelectFiles: true,
                canSelectFolders: false,
                canSelectMany: false
            });

            if (!res) {
                return undefined;
            }

            return res[0].fsPath;
        }
    );
}

/**
 * Interactively updates a configuration section.
 * @param section The the config section to update.
 * @param title The title input box.
 * @param prompt The input box prompt.
 * @returns The new value of the configuration, or undefined if canceled.
 */
export async function updateInteractively(
    section: string,
    title?: string,
    prompt?: string,
    inputFunc?: (title?: string, prompt?: string) => Thenable<string | undefined>
): Promise<string | undefined> {
    inputFunc ??= async (title, prompt) => await window.showInputBox({ title, prompt });

    let value = await inputFunc(title, prompt);

    if (!value) {
        return undefined;
    }

    let target = await window.showQuickPick([
        {
            label: "Global",
            description: "Set the configuration globally",
            target: ConfigurationTarget.Global
        },
        {
            label: "Workspace",
            description: "Set the configuration in the workspace",
            target: ConfigurationTarget.Workspace
        },
        {
            label: "Workspace folder",
            description: "Set the configuration in the workspace folder",
            target: ConfigurationTarget.WorkspaceFolder
        }
    ], {
        title: "Where should the configuration be set?",
        canPickMany: false,
    });

    if (!target) {
        return undefined;
    }

    await getConfig().update(section, value, target.target);

    return value;
}

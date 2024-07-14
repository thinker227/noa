import { WorkspaceConfiguration, workspace } from "vscode";

export function getConfig(): WorkspaceConfiguration {
    return workspace.getConfiguration("noa-lang");
}

export function getCliCommand(): string {
    return getConfig().get("cliExecutable") || "noa";
}

export function getServerLogPath(): string | undefined {
    return getConfig().get("serverLogPath");
}

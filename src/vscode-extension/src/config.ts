import { WorkspaceConfiguration, workspace } from "vscode";

export function getConfig(): WorkspaceConfiguration {
    return workspace.getConfiguration("noa-lang");
}

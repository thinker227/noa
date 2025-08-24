import { Terminal, TextDocument, ThemeIcon, window,  } from "vscode";
import { checkForCli } from "./command_line";
import { getCliCommand } from "./config";
import { getRuntime } from "./runtime";
import path = require("path");

let terminal: Terminal | undefined = undefined;

const terminalIcon: ThemeIcon = new ThemeIcon("notebook-execute");

function getOrCreateTerminal(): Terminal {
    terminal ??= window.createTerminal({
        name: "Noa run",
        iconPath: terminalIcon,
        
    });

    return terminal;
}

function constructRunCommand(document: TextDocument, runtimePath: string): string {
    let args = [
        document.uri.fsPath,
        "--print-ret",
        "--runtime",
        runtimePath
    ];

    let command = getCliCommand();

    return `${command} ${args.join(" ")}`;
}

/**
 * Runs a Noa document.
 * @param document The document to run.
 * @returns Whether the document was successfully run.
 */
export async function runDocument(document: TextDocument): Promise<boolean> {
    if (!await checkForCli()) {
        let command = getCliCommand();
        await window.showErrorMessage(`Cannot find the Noa CLI (${command})`);

        return false;
    }

    let cwd = path.resolve(document.uri.fsPath, "..");
    let runtimePath = await getRuntime(cwd);

    if (!runtimePath) {
        await window.showErrorMessage("Cannot find the Noa runtime.");

        return false;
    }

    let runCommand = constructRunCommand(document, runtimePath);

    await document.save();

    let terminal = getOrCreateTerminal();
    terminal.show();

    terminal.sendText(runCommand, true);
}

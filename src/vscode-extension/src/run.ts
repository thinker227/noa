import { Terminal, TextDocument, ThemeIcon, Uri, window,  } from "vscode";
import { checkForCli } from "./command_line";
import { getCliCommand, getRuntimePath, updateRuntimePathInteractively } from "./config";
import { getNoaRuntimeVariable } from "./variables";

let terminal: Terminal | undefined = undefined;

const terminalIcon: ThemeIcon = new ThemeIcon("notebook-execute");

function getOrCreateTerminal(): Terminal {
    terminal ??= window.createTerminal({
        name: "Noa run",
        iconPath: terminalIcon,
        
    });

    return terminal;
}

async function userUpdateRuntimePath(): Promise<string | undefined> {
    async function showError(): Promise<void> {
        await window.showErrorMessage("No runtime executable has been configured.");
    }

    let response = await window.showWarningMessage(
        "The environment `NOA_RUNTIME` is not set. Do you want to manually set the path to the Noa runtime executable?",
        {
            title: "Yes",
            value: true
        },
        {
            title: "No",
            value: false
        }
    );

    if (response === undefined || !response.value) {
        await showError();
        return undefined;
    }

    let configured = await updateRuntimePathInteractively();

    if (!configured) {
        await showError();
        return undefined;
    }

    return configured;
}

function constructRunCommand(document: TextDocument, runtimePath: string | undefined): string {
    let args = [
        "run",
        document.uri.fsPath,
        "--print-ret"
    ];
    
    if (runtimePath !== undefined) {
        args.push(`--runtime ${runtimePath}`);
    }

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
        await window.showErrorMessage(
            `Cannot find the Noa CLI (${command})`
        );

        return false;
    }

    let runtimePath: string | undefined;

    if (getNoaRuntimeVariable() !== undefined) {
        runtimePath = undefined;
    } else {
        runtimePath = getRuntimePath();

        if (!runtimePath) {
            runtimePath = await userUpdateRuntimePath();

            if (!runtimePath) {
                return false;
            }
        }
    }

    let runCommand = constructRunCommand(document, runtimePath);

    await document.save();

    let terminal = getOrCreateTerminal();
    terminal.show();

    terminal.sendText(runCommand, true);
}

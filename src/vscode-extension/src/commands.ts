import { window } from "vscode";
import { restartLanguageServer } from "./lang_server";
import { runDocument } from "./run";

/**
 * Restarts the language server.
 */
export async function restartLangServer() {
    let success = await restartLanguageServer();

    if (!success) {
        window.showErrorMessage("Failed to restart the language server.");
    }
}

/**
 * Runs the currently opened document.
 */
export async function run() {
    let editor = window.activeTextEditor;
    
    if (editor) {
        await runDocument(editor.document);
    } else {
        window.showErrorMessage(
            "Cannot run document because no editor is active."
        );
    }
}

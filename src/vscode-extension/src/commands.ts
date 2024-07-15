import { window } from "vscode";
import { restartLanguageServer } from "./lang_server";

/**
 * Restarts the language server.
 */
export async function restartLangServer() {
    let success = await restartLanguageServer();

    if (!success) {
        window.showErrorMessage("Failed to restart the language server.");
    }
}

"use strict";

import { ExtensionContext, commands } from "vscode";
import { restartLangServer } from "./commands";
import { startLanguageServer, stopLanguageServer } from "./lang_server";
import { initializeVariables } from "./variables";

export async function activate(context: ExtensionContext) {
    initializeVariables(context.environmentVariableCollection);

    context.subscriptions.push(commands.registerCommand("noa-lang.restartLangServer", restartLangServer));

    await startLanguageServer();
}

export async function deactivate() {
    await stopLanguageServer();
}

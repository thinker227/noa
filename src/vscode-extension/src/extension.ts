"use strict";

import { ExtensionContext, commands } from "vscode";
import { restartLangServer, run } from "./commands";
import { startLanguageServer, stopLanguageServer } from "./lang_server";
import { initializeVariables } from "./variables";

export async function activate(context: ExtensionContext) {
    initializeVariables(context.environmentVariableCollection);

    context.subscriptions.push(commands.registerCommand("noa-lang.restartLangServer", restartLangServer));
    context.subscriptions.push(commands.registerCommand("noa-lang.run", run));

    await startLanguageServer();
}

export async function deactivate() {
    await stopLanguageServer();
}

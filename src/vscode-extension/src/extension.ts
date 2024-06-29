"use strict";

import { workspace, ExtensionContext } from "vscode";
import {
    LanguageClient,
    LanguageClientOptions,
    ServerOptions,
    TransportKind,
    Trace
} from "vscode-languageclient/node";

let client: LanguageClient;

export async function activate(context: ExtensionContext) {
    let serverExe = "dotnet";
    let path = "this/is/very/obviously/a/path/lmao";
    let args = [path, "lang-server"];

    // If the extension is launched in debug mode then the debug server options are used,
    // otherwise the run options are used.
    let serverOptions: ServerOptions = {
        run: {
            command: serverExe,
            args: args,
            transport: TransportKind.stdio,
        },
        debug: {
            command: serverExe,
            args: args,
            transport: TransportKind.stdio,
            runtime: "",
        },
    };

    // Options to control the language client
    let clientOptions: LanguageClientOptions = {
        // Register the server for text documents
        documentSelector: [{ pattern: "**/*.noa" }],
        progressOnInitialization: true,
        synchronize: {
            // Synchronize the setting section "noa-lang-server" to the server
            configurationSection: "noa-lang-server",
            fileEvents: workspace.createFileSystemWatcher("**/*.noa"),
        },
    };

    // Create the language client and start the client
    client = new LanguageClient("noa-lang-server", "Noa Language Server", serverOptions, clientOptions);
    client.registerProposedFeatures();
    client.setTrace(Trace.Verbose);
    await client.start();
}

export function deactivate() {
    return client.stop();
}

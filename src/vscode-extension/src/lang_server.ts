import { window, workspace } from "vscode";
import { LanguageClient, LanguageClientOptions, ServerOptions, Trace, TransportKind } from "vscode-languageclient/node";
import { getConfig } from "./config";

let client: LanguageClient | undefined = undefined;

export function getClient(): LanguageClient | undefined {
    return client;
}

function getExecutableInfo(): [string, string[]] {
    let config = getConfig();

    let cliExecutable: string = config.get("cliExecutable");
    let serverLogPath: string | null = config.get("serverLogPath");

    let command = cliExecutable
        ? cliExecutable
        : "noa";

    let args = ["lang-server"];
    if (serverLogPath) args.push("--log", serverLogPath);

    return [command, args];
}

export async function startLanguageServer() {
    await stopLanguageServer();

    let [command, args] = getExecutableInfo();

    // If the extension is launched in debug mode then the debug server options are used,
    // otherwise the run options are used.
    let serverOptions: ServerOptions = {
        run: {
            command,
            args,
            transport: TransportKind.stdio,
        },
        debug: {
            command,
            args,
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
            // Synchronize the setting section "noa-lang" to the server
            configurationSection: "noa-lang",
            fileEvents: workspace.createFileSystemWatcher("**/*.noa"),
        },
    };

    // Create the language client and start the client
    client = new LanguageClient("noa-lang-server", "Noa Language Server", serverOptions, clientOptions);
    client.registerProposedFeatures();
    client.setTrace(Trace.Verbose);

    window.showInformationMessage(`Starting Noa Language Server (${command} ${args.join(" ")} --stdio)`);

    await client.start();

    return client;
}

export async function stopLanguageServer() {
    if (!client) return false;

    await client.stop();
    return true;
}

export async function restartLanguageServer() {
    if (!client) return false;

    await client.restart();
    return true;
}

import { window, workspace } from "vscode";
import { LanguageClient, LanguageClientOptions, ServerOptions, Trace, TransportKind } from "vscode-languageclient/node";
import { getCliCommand, getLogLevel, getServerLogPath } from "./config";
import { checkForCli } from "./command_line";

let client: LanguageClient | undefined = undefined;

/**
 * Returns the language client, or `undefined` if none has been started.
 */
export function getClient(): LanguageClient | undefined {
    return client;
}

function getExecutableInfo(): [string, string[]] {
    let command = getCliCommand();
    let serverLogPath = getServerLogPath();
    let logLevel = getLogLevel();

    let args = ["lang-server"];
    if (serverLogPath) args.push("--log", serverLogPath);
    
    args.push("--log-level", logLevel);

    return [command, args];
}

/**
 * Attempts to start a new language client.
 * @returns The started language client, or `undefined` if none could be started.
 */
export async function startLanguageServer(): Promise<LanguageClient | undefined> {
    await stopLanguageServer();

    if (!await checkForCli()) {
        let command = getCliCommand();
        await window.showErrorMessage(
            `Cannot find the Noa CLI (${command})`
        );

        return undefined;
    }

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

/**
 * Stops the currently running language client.
 * @returns Whether the language client existed and therefore could be stopped.
 */
export async function stopLanguageServer() {
    if (!client) return false;

    await client.stop();
    return true;
}

/**
 * Restarts the currently running language client.
 * @returns Whether the language client existed and therefore could be restarted.
 */
export async function restartLanguageServer() {
    if (!client) return false;

    await client.restart();
    return true;
}

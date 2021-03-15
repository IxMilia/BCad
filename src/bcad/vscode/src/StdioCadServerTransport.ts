import * as cp from 'child_process';
import * as vscode from 'vscode';
import * as os from 'os';
import * as path from 'path';
import * as rpc from 'vscode-jsonrpc';
import { ClientUpdate } from './client/contracts.generated';

export class StdioCadServerTransport {
    private childProcess: cp.ChildProcessWithoutNullStreams;
    private clientUpdateSubscriptions: Array<{ (clientUpdate: ClientUpdate): void }> = [];
    private connection: rpc.MessageConnection;
    private nextId: number = 1;
    private postMessage: { (message: any): void } = _ => { };
    private onReadyCallbacks: Array<{ (): void }> = [];
    private replyHandlers: Map<number, { (payload: any): void }> = new Map<number, { (payload: any): void }>();

    constructor(readonly uri: vscode.Uri, dotnetPath: string, serverPath: string) {
        const serverAssembly = path.join(serverPath, 'IxMilia.BCad.Server.dll');
        this.childProcess = cp.spawn(dotnetPath, [serverAssembly]);
        this.childProcess.on('exit', (code: number, signal: string) => {
        });
        console.log('server pid ' + this.childProcess.pid);
        let logger: rpc.Logger = {
            error: console.log,
            warn: console.log,
            info: console.log,
            log: console.log,
        };
        this.connection = rpc.createMessageConnection(
            new rpc.StreamMessageReader(this.childProcess.stdout),
            new rpc.StreamMessageWriter(this.childProcess.stdin),
            logger);
        this.connection.onRequest((method, params) => {
            // request from the server, forward
            return new Promise<any>((resolve, reject) => {
                const id = this.nextId++;
                this.replyHandlers.set(id, resolve);
                this.postMessage({
                    method,
                    params,
                    id
                });
            });
        });
        this.connection.onNotification((method, params) => {
            if (method === 'ClientUpdate') {
                const clientUpdate = <ClientUpdate>params;
                for (let sub of this.clientUpdateSubscriptions) {
                    sub(clientUpdate);
                }
            }
            // all client notifications are forwarded with no changes
            this.postMessage({
                method,
                params
            });
        });
        this.connection.listen();
    }

    prepareHandlers(webviewPanel: vscode.WebviewPanel): vscode.Disposable {
        const disposable = webviewPanel.webview.onDidReceiveMessage(async message => {
            // message from the client, forward to the server
            this.handleMessageFromClient(message);
            if (message?.method === 'Ready') {
                for (let onReady of this.onReadyCallbacks) {
                    onReady();
                }

                this.onReadyCallbacks = [];
            }
        });
        this.postMessage = message => webviewPanel.webview.postMessage(message);
        return disposable;
    }

    registerReadyCallback(onReady: { (): void }) {
        this.onReadyCallbacks.push(onReady);
    }

    subscribeToClientUpdates(subscription: { (clientUpdate: ClientUpdate): void }) {
        this.clientUpdateSubscriptions.push(subscription);
    }

    undo() {
        this.connection.sendNotification('Undo', {});
    }

    redo() {
        this.connection.sendNotification('Redo', {});
    }

    private async handleMessageFromClient(message: any) {
        if (message?.method !== 'MouseMove') {
            console.log(JSON.stringify(message));
        }
        if (message.result !== undefined && message.id) {
            // response to a server-side request, find the appropriate reply channel and send it back
            const handler = this.replyHandlers.get(message.id);
            if (handler) {
                this.replyHandlers.delete(message.id);
                handler(message.result);
            }
        } else if (message.id) {
            // request from client, reply expected
            const serverResult = await this.connection.sendRequest(message.method, message.params);
            this.postMessage({
                result: serverResult,
                id: message.id
            });
        } else {
            // notification from client, just forward to the server
            if (message.method === 'ExecuteCommand') {
                // currently the UI doesn't send these commands; they're all handled server-side
                switch (message.params.command) {
                    case 'File.Open':
                        // swallow because we don't allow this?
                        return;
                    case 'File.Save':
                        // TODO: intercept this and reinterpret
                        return;
                    case 'File.SaveAs':
                        // TODO: how to properly switch files?
                        return;
                    default:
                        break;
                }
            }

            this.connection.sendNotification(message.method, message.params);
        }
    }

    parseFile(fileContents: string) {
        this.connection.sendNotification('ParseFile', { filePath: this.uri.fsPath, data: fileContents });
    }

    async getDrawingContents(uri: vscode.Uri, preserveSettings: boolean): Promise<string | null> {
        const filePath = uri.fsPath;
        const fileContents = await this.connection.sendRequest<string | null>('GetDrawingContents', { filePath, preserveSettings });
        return fileContents;
    }

    dispose() {
        this.childProcess.kill();
    }
}

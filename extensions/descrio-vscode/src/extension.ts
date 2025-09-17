import * as path from 'path';
import { workspace, ExtensionContext } from 'vscode';

import {
  LanguageClient,
  LanguageClientOptions,
  ServerOptions,
  TransportKind
} from 'vscode-languageclient/node';

let client: LanguageClient;

export function activate(context: ExtensionContext) {
  const serverExecutable = context.asAbsolutePath(
    path.join('..', '..', 'src', 'Descrio.LspServer', 'bin', 'Debug', 'net8.0', 'Descrio.LspServer.exe')
  );

  const serverOptions: ServerOptions = {
    run: { command: serverExecutable, transport: TransportKind.stdio },
    debug: { command: serverExecutable, transport: TransportKind.stdio }
  };

  const clientOptions: LanguageClientOptions = {
    documentSelector: [{ scheme: 'file', language: 'descrio' }],
    synchronize: {
      fileEvents: workspace.createFileSystemWatcher('**/*.yaml')
    }
  };

  client = new LanguageClient(
    'descrioLanguageServer',
    'Descrio Language Server',
    serverOptions,
    clientOptions
  );

  client.start();
}

export function deactivate(): Thenable<void> | undefined {
  if (!client) {
    return undefined;
  }
  return client.stop();
}
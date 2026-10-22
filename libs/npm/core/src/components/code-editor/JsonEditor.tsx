import Editor, { BeforeMount, loader } from '@monaco-editor/react';
import type * as MonacoEditor from 'monaco-editor/esm/vs/index';
import React from 'react';

import { Spinner } from '../spinners';
import * as styled from './styled';

/** Where the app serves monaco's files from; the apps copy them into public/monaco/vs. */
let monacoPath = '/monaco/vs';
let isLoaderConfigured = false;

/**
 * Changes where monaco's files are loaded from. Call it before the first editor renders.
 * @param path The URL of monaco's "vs" folder (default: /monaco/vs).
 */
export const configureJsonEditor = (path: string) => {
  monacoPath = path;
  isLoaderConfigured = false;
};

const configureLoader = () => {
  if (isLoaderConfigured) return;
  loader.config({ paths: { vs: monacoPath } });
  isLoaderConfigured = true;
};

export interface IJsonEditorProps {
  /** A name unique to this editor; it identifies the document the schema applies to. */
  name: string;
  /** The JSON text. */
  value: string;
  /** The text changed. */
  onChange?: (value: string) => void;
  /** A JSON schema that drives completion, hover documentation and validation. */
  schema?: object;
  /** Whether the text can be edited (default: editable). */
  readOnly?: boolean;
  /** The editor height (default: 100% of its container). */
  height?: string | number;
  className?: string;
}

/**
 * A JSON code editor (monaco) with colouring, and completion and validation from a schema.
 */
export const JsonEditor: React.FC<IJsonEditorProps> = ({
  name,
  value,
  onChange,
  schema,
  readOnly = false,
  height = '100%',
  className,
}) => {
  configureLoader();
  const path = `inmemory://tno/${name}.json`;

  const beforeMount: BeforeMount = React.useCallback(
    (monaco) => {
      if (!schema) return;
      // The wrapper types monaco as the editor API alone; the loaded module also has the languages.
      const defaults = (monaco as unknown as typeof MonacoEditor).json.jsonDefaults;
      const uri = `${path}.schema`;
      const others = (defaults.diagnosticsOptions.schemas ?? []).filter((s) => s.uri !== uri);
      defaults.setDiagnosticsOptions({
        ...defaults.diagnosticsOptions,
        validate: true,
        enableSchemaRequest: false,
        schemas: [...others, { uri, fileMatch: [path], schema }],
      });
    },
    [path, schema],
  );

  return (
    <styled.JsonEditor className={`json-editor${className ? ` ${className}` : ''}`}>
      <Editor
        path={path}
        language="json"
        value={value}
        height={height}
        onChange={(text) => onChange?.(text ?? '')}
        beforeMount={beforeMount}
        loading={<Spinner size="1.5rem" />}
        options={{
          readOnly,
          minimap: { enabled: false },
          automaticLayout: true,
          scrollBeyondLastLine: false,
          tabSize: 2,
          wordWrap: 'on',
          formatOnPaste: true,
          fontSize: 13,
          lineNumbersMinChars: 3,
          folding: true,
          quickSuggestions: { strings: true, other: true, comments: false },
          suggest: { showWords: false },
        }}
      />
    </styled.JsonEditor>
  );
};

import React from 'react';
import { createPortal } from 'react-dom';
import { FaCircleInfo, FaPaste } from 'react-icons/fa6';
import styled from 'styled-components';

import { aiInputFields, defaultAIInputFields } from './aiInputFields';

const Help = styled.dialog`
  width: min(48rem, 90vw);
  max-height: 85vh;
  overflow-y: auto;
  padding: 1.5rem;
  border: 1px solid #ccc;
  border-radius: 0.5rem;
  &::backdrop {
    background: #0006;
  }
  table {
    width: 100%;
    border-collapse: collapse;
  }
  th,
  td {
    padding: 0.4rem;
    text-align: left;
    vertical-align: top;
    border-bottom: 1px solid #ddd;
  }
  pre {
    white-space: pre-wrap;
    overflow-wrap: anywhere;
    padding: 0.75rem;
    background: #f5f5f5;
  }
`;

/** Actions shared by the inline and expanded prompt editor toolbars. */
export const ReportPromptTools: React.FC<{
  defaultPrompt?: string;
  inputFields?: string[];
  onUseDefault: () => void;
}> = ({ defaultPrompt, inputFields, onUseDefault }) => {
  const dialog = React.useRef<HTMLDialogElement>(null);
  // Rich-text defaults containing only empty paragraphs are not usable defaults.
  const hasDefault = !!defaultPrompt
    ?.replace(/<[^>]*>/g, '')
    .replace(/&nbsp;/g, ' ')
    .trim();
  const selected = inputFields ?? defaultAIInputFields;

  return (
    <>
      {hasDefault && (
        <button
          type="button"
          title="Use default user prompt"
          aria-label="Use default user prompt"
          onClick={onUseDefault}
        >
          <FaPaste className="custom-icon" />
        </button>
      )}
      <button
        type="button"
        title="Prompt data and story links"
        aria-label="Prompt data and story links"
        onClick={() => dialog.current?.showModal()}
      >
        <FaCircleInfo className="custom-icon" />
      </button>
      {createPortal(
        <Help ref={dialog} aria-label="Prompt data and story links">
          <h2>Prompt data and story links</h2>
          <p>
            The AI reads every story in the chosen sections using only the data fields you select
            below the editor, with article text as a fallback when no summary is available. The same
            selection applies to prior reports. Missing values are omitted.
          </p>
          <h3>Story data</h3>
          <p>
            Stories are sent as text blocks, not JSON objects. Each starts with a source reference
            such as <code>[S1]</code>, followed by the selected headline, metadata, and text. An
            analyzed story uses its summary, key facts, people and organizations, and quotes.
            Selecting Article text uses it instead of the summary. If article text is empty, the
            summary is used when selected. When Article text is unselected, it is still used if
            neither an analysis summary nor a story summary is available. Summary and article text
            are never sent together.
          </p>
          <table>
            <thead>
              <tr>
                <th>Field</th>
                <th>Meaning</th>
                <th>Selected</th>
              </tr>
            </thead>
            <tbody>
              {aiInputFields.map((field) => (
                <tr key={field.value}>
                  <td>
                    <code>{field.value}</code>
                  </td>
                  <td>{field.description}</td>
                  <td>{selected.includes(field.value) ? 'Yes' : 'No'}</td>
                </tr>
              ))}
            </tbody>
          </table>
          <h3>Example input with default fields</h3>
          <pre>{`[S1] New transit service announced\nsource: Example News | mediaType: Online | publishedOn: 2026-10-06 12:00 UTC\nThe city announced a new bus route.\nKey facts:\n- Service begins in November.\nEntities: City council\nQuotes:\n- "Service starts soon" (Spokesperson)`}</pre>
          <p>
            Large inputs are summarized in batches. The final writing step receives findings with a
            statement and the supporting source references, rather than the original story objects.
          </p>
          <pre>{`- Service begins in November. [S1]`}</pre>
          <h3>Links that open the story</h3>
          <p>
            Ask the AI to cite the supplied references, such as <code>[S1]</code>. The report
            replaces them with links to the subscriber’s <code>/view/:id</code> page, opening in a
            new tab. IDs and URLs are retained by the report service, so links still work when you
            deselect the headline or other data fields.
          </p>
          <p>
            There is no <code>url</code> field in the model input. Do not ask the AI to construct
            URLs or put source references inside HTML links. Prior-report context is provided
            separately; its references are not links to current stories.
          </p>
          <h3>Example prompt</h3>
          <pre>
            Write five bullet points about the main developments. End each point with the supplied
            source references in square brackets, such as [S1] or [S1][S2]. Do not construct URLs.
          </pre>
          <button type="button" onClick={() => dialog.current?.close()}>
            Close
          </button>
        </Help>,
        document.body,
      )}
    </>
  );
};

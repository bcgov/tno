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

const HelpButton = styled.button`
  &&& {
    width: auto;
    display: inline-flex;
    align-items: center;
    gap: 0.35rem;
    white-space: nowrap;
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
      <HelpButton
        type="button"
        title="Prompt data and story links"
        aria-label="Prompt data and story links"
        onClick={() => dialog.current?.showModal()}
      >
        <FaCircleInfo className="custom-icon" />
        <span>Prompt data</span>
      </HelpButton>
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
          <h3>Link data sent to the final writing step</h3>
          <p>
            Alongside the findings, the AI receives a <strong>Story link data</strong> JSON array.
            Match a finding’s reference, such as <code>[S1]</code>, to the entry whose{' '}
            <code>reference</code> is <code>S1</code>. Copy its <code>anchor</code> or{' '}
            <code>url</code> exactly into <code>href</code>, depending on the destination you want.
            Link data is provided regardless of the selected story fields.
          </p>
          <table>
            <thead>
              <tr>
                <th>Property</th>
                <th>Meaning</th>
              </tr>
            </thead>
            <tbody>
              <tr>
                <td>
                  <code>reference</code>
                </td>
                <td>The story reference used by a finding, without brackets.</td>
              </tr>
              <tr>
                <td>
                  <code>url</code>
                </td>
                <td>
                  The subscriber story page URL, for example https://your-subscriber-site/view/123.
                  May be null when no story page URL is configured.
                </td>
              </tr>
              <tr>
                <td>
                  <code>anchor</code>
                </td>
                <td>
                  A link within the current report, such as #item-123. Only available when the story
                  is rendered in a visible report section. Null for hidden sections and
                  headlines-only stories.
                </td>
              </tr>
            </tbody>
          </table>
          <pre>{`## Findings for this report\n- Service begins in November. [S1]\n- Council approved a second route. [S2]\n\n## Story link data\n[\n  { "reference": "S1", "url": "https://your-subscriber-site/view/123", "anchor": "#item-123" },\n  { "reference": "S2", "url": "https://your-subscriber-site/view/456", "anchor": null }\n]`}</pre>
          <p>
            An <code>anchor</code> starting with <code>#</code> jumps to the story in the current
            report. Omit <code>target="_blank"</code> for these links so they stay in the same tab.
            A subscriber <code>/view/:id</code> URL can open in a new tab. Links to unknown URLs or
            anchors are removed. Previous reports provide context; their stories are not link
            targets in this report.
          </p>
          <h3>Example prompt: read within the report</h3>
          <pre>{`Write a concise summary as HTML bullet points. Match each supporting story reference to the Story link data. End each story bullet with a <a href="{anchor}">read</a>, where {anchor} is that story's anchor field copied exactly. Never construct or invent a URL; omit the link if a story has no anchor field or its value is null or empty. Do not substitute url for anchor. Do not also add bracketed citations to a bullet that already has a read link.`}</pre>
          <h3>Example prompt: view the story in a new tab</h3>
          <pre>{`Write a concise summary as HTML bullet points. Match each supporting story reference to the Story link data. End each story bullet with a <a target="_blank" href="{url}">view</a>, where {url} is that story's url field copied exactly. Never construct or invent a URL; omit the link if a story has no url field or its value is null or empty. Do not substitute anchor for url. Do not also add bracketed citations to a bullet that already has a view link.`}</pre>
          <h3>Example prompt: automatic story links</h3>
          <pre>{`Write five bullet points about the main developments. End each point with supporting story references, such as [S1] or [S1][S2].`}</pre>
          <p>The report replaces these references with story links automatically.</p>
          <button type="button" onClick={() => dialog.current?.close()}>
            Close
          </button>
        </Help>,
        document.body,
      )}
    </>
  );
};

export const sectionDocs = {
  profile: {
    title: 'Automation Profile',
    content: (
      <>
        <p>
          Profiles define one automation flow. Each profile can select a filter to load
          Elasticsearch content and then run steps in order.
        </p>
        <ul>
          <li>Profile filter is optional.</li>
          <li>
            If a profile filter exists, each content result is iterated for content-targeted steps.
          </li>
          <li>Use schedule cron for recurring runs, or leave it blank for manual-only runs.</li>
        </ul>
      </>
    ),
  },
  steps: {
    title: 'Step Configuration',
    content: (
      <>
        <p>
          Steps run in configured order. Each step has its own prompt, optional filter behavior, and
          one or more actions.
        </p>
        <ul>
          <li>
            Step target <strong>start</strong> runs once before iterating profile filter results.
          </li>
          <li>
            Step target <strong>content</strong> runs for each content item.
          </li>
          <li>
            Step target <strong>end</strong> runs once after all content items are processed.
          </li>
        </ul>
      </>
    ),
  },
  schedule: {
    title: 'Schedule Help',
    content: (
      <>
        <p>
          The Scheduler service queues a run for this profile once per day at (or after) the Run At
          time, on the selected week days. This works safely when services are scaled to multiple
          instances.
        </p>
        <ul>
          <li>Times are interpreted in the Scheduler service's configured time zone.</li>
          <li>Select no week days to run every day.</li>
          <li>
            <strong>Start After</strong> is the date/time the schedule becomes valid; nothing runs
            before it. Leave it empty and a schedule saved after its Run At time has already passed
            becomes eligible the same day - set it to a future date/time to hold the first run until
            then.
          </li>
          <li>Disable the schedule to run this profile manually only.</li>
        </ul>
      </>
    ),
  },
  stepPrompt: {
    title: 'Step Prompt Keywords',
    content: (
      <>
        <p>
          Step prompts support token substitution using braces. Tokens are replaced at runtime with
          values from the current content item and/or step filter results.
        </p>
        <ul>
          <li>
            <code>{`{content}`}</code> - Inject the current content item payload.
          </li>
          <li>
            <code>{`{content.<field>}`}</code> - Use a field from the current iterated content item.
          </li>
          <li>
            <code>{`{results}`}</code> - Inject the full step filter results payload.
          </li>
          <li>
            <code>{`{results[<index>].<field>}`}</code> - Use a specific field from a specific
            result item.
          </li>
          <li>
            <code>{`{actions}`}</code> - Inject the composed prompts of this step&apos;s actions.
            The step and its actions produce a single prompt per content item; the response is
            parsed for each action&apos;s confirmation statement to determine which actions to
            perform.
          </li>
        </ul>
        <p>
          Examples: {`{content}`}, {`{content.headline}`}, {`{results[0].headline}`}
        </p>
        <p>
          If a token path does not exist at runtime, the rendered value is blank. Keep prompts
          resilient by including clear fallback instructions.
        </p>
      </>
    ),
  },
  actionPrompt: {
    title: 'Action Prompt',
    content: (
      <>
        <p>
          Each action contributes a prompt fragment. The step composes a single prompt per content
          item - the step prompt plus every action prompt (injected at the{' '}
          <code>{`{actions}`}</code> token) - and sends it to the LLM once. The response is then
          parsed for each action&apos;s <strong>Confirmation Statement</strong> to decide which
          actions to perform.
        </p>
        <p>Write an action prompt as a condition plus the exact response you expect:</p>
        <ul>
          <li>
            Describe the condition using the content properties, for example{' '}
            <code>{`content.body`}</code>, <code>{`content.headline`}</code>,{' '}
            <code>{`content.summary`}</code>, <code>{`content.source.name`}</code>, or{' '}
            <code>{`content.tonePools`}</code>.
          </li>
          <li>
            Instruct the model to respond with a unique phrase when the condition is met, and set
            the <strong>Confirmation Statement</strong> to that exact phrase.
          </li>
          <li>
            Pick distinctive phrases (for example <code>[IGNORE CONTENT]</code>) so they cannot
            appear in the response by accident.
          </li>
          <li>
            To extract data from the response, include the <code>{`{value}`}</code> token in both
            the prompt and the Confirmation Statement (for example{' '}
            <code>{`[SENTIMENT:{value}]`}</code>). The statement is matched as a pattern and{' '}
            <code>{`{value}`}</code> is captured - a number, a name, a list, or even rewritten text.
          </li>
          <li>
            <code>{`{field}`}</code> is replaced with the action&apos;s selected Content Field
            (Update Content Field actions).
          </li>
          <li>
            <code>{`{objective}`}</code> is replaced with the action&apos;s Objective (Score Content
            and Select Top Content actions). Score actions record a per-story score for the
            objective; the matching Select Top Content action (usually on an <em>end</em> step)
            receives the top scored stories at <code>{`{candidates:<objective>}`}</code> (contentId,
            score, headline, source, summary) and applies its Content Action to the ids it selects,
            capped by Max Calls.
          </li>
          <li>
            For large multiline values (rewritten text, HTML, XML) wrap <code>{`{value}`}</code>{' '}
            between start and end marker lines so the capture is clearly bounded:
            <br />
            <code>{`[UPDATE FIELD START:{field}]`}</code>
            <br />
            <code>{`{value}`}</code>
            <br />
            <code>{`[UPDATE FIELD END:{field}]`}</code>
          </li>
        </ul>
        <p>
          <strong>Example - Add Sentiment (with value extraction):</strong>
        </p>
        <p>
          Prompt:{' '}
          <code>
            {`Review the content.headline, content.summary, and content.body and generate a sentiment value from -5 to 5. Respond with "[SENTIMENT:{value}]"`}
          </code>
        </p>
        <p>
          Confirmation Statement: <code>{`[SENTIMENT:{value}]`}</code> - when the model responds
          with <code>[SENTIMENT:3]</code>, the action executes with the extracted value{' '}
          <code>3</code>.
        </p>
        <p>
          <strong>Example - Stop Remaining Actions:</strong>
        </p>
        <p>
          Prompt:{' '}
          <code>{`If the content.body is less than 100 words respond with "[IGNORE CONTENT]"`}</code>
        </p>
        <p>
          Confirmation Statement: <code>[IGNORE CONTENT]</code>
        </p>
        <p>
          When the model responds with <code>[IGNORE CONTENT]</code> the Stop Remaining Actions
          action executes. It is position sensitive: actions listed <em>above</em> it that were
          confirmed are still applied, but none of the actions listed <em>below</em> it are
          performed for this content item.
        </p>
        <p>
          <strong>Example - Publish Content Item:</strong>
        </p>
        <p>
          Prompt:{' '}
          <code>{`If the story is about the provincial government respond with "[PUBLISH STORY]"`}</code>
        </p>
        <p>
          Confirmation Statement: <code>[PUBLISH STORY]</code>
        </p>
      </>
    ),
  },
  stepFilters: {
    title: 'Target and Filter Behavior',
    content: (
      <>
        <p>
          <strong>Target</strong> controls when this step runs: <em>start</em> runs once before
          iterating content, <em>content</em> runs for each iterated item, and <em>end</em> runs
          once after iteration. When the profile does not include a filter there is no content
          iteration, so the target must be <em>none</em>.
        </p>
        <p>
          <strong>Step Filter</strong> is optional; when selected, it can fetch related search
          results for prompt context.
        </p>
        <p>
          Enable <strong>Apply filter to profile content</strong> to use the step filter as a gate
          on the current profile content item and skip this step when it does not match.
        </p>
      </>
    ),
  },
} as const;

/** The report status message prefix that announces a finished AI section test run. */
export const AI_SECTION_TEST_MESSAGE_PREFIX = 'ai-test:';

/**
 * Whether a report status message announces a finished AI section test run. Such a message is for
 * the prompt workbench only; it does not mean the report or its instance changed.
 * @param message The hub message.
 * @returns True when the message is a test run notice.
 */
export const isAISectionTestMessage = (message?: { message?: string | null }) =>
  !!message?.message?.startsWith(AI_SECTION_TEST_MESSAGE_PREFIX);

/**
 * The test run a report status message announces.
 * @param message The hub message.
 * @returns The test id, or undefined when the message is not a test run notice.
 */
export const getAISectionTestId = (message?: { message?: string | null }) =>
  isAISectionTestMessage(message)
    ? message!.message!.substring(AI_SECTION_TEST_MESSAGE_PREFIX.length)
    : undefined;

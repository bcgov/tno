/** Return the description of the selected LLM, if any. */
export const getLLMDescription = (
  llms: { id: number; description?: string }[],
  llmId?: number,
): string => (llmId ? llms.find((llm) => llm.id === llmId)?.description : undefined) ?? '';

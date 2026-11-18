export interface IFilterAIStepModel {
  /** The tool the model used [lookup, search]. */
  tool: string;
  /** The arguments it sent, as JSON. */
  arguments: string;
  /** What the tool returned to the model, as JSON. */
  result: string;
}

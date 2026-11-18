export interface ITransferObject {
  checked: boolean;
  originalId: number;
  originalName: string;
  newId?: number;
  newName?: string;
  subscribeOnly: boolean;
}

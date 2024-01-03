export interface ISubMediaGroupExpanded {
  all: boolean;
  cpNews: boolean;
  dailyPrint: boolean;
  online: boolean;
  weeklyPrint: boolean;
  talkRadio: boolean;
  television: boolean;
  newsRadio: boolean;
}

/** keeps track of thea advanced search sub menu state (open/ collapsed) */
export const defaultSubMediaGroupExpanded: ISubMediaGroupExpanded = {
  all: false,
  cpNews: false,
  dailyPrint: false,
  online: false,
  weeklyPrint: false,
  talkRadio: false,
  television: false,
  newsRadio: false,
};

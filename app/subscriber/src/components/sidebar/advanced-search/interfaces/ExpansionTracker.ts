export interface ISubMediaGroupExpanded {
  all: boolean;
  cpNews: boolean;
  dailyPapers: boolean;
  internet: boolean;
  regionalPapers: boolean;
  talkRadio: boolean;
  television: boolean;
}

/** keeps track of thea advanced search sub menu state (open/ collapsed) */
export const defaultSubMediaGroupExpanded: ISubMediaGroupExpanded = {
  all: false,
  cpNews: false,
  dailyPapers: false,
  internet: false,
  regionalPapers: false,
  talkRadio: false,
  television: false,
};

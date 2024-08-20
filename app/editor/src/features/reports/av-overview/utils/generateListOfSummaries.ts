import { getDistinct, IAVOverviewSectionItemModel, IAVOverviewSectionModel } from 'tno-core';

export interface ISectionSummary {
  key: string;
  text: string;
}

/**
 * Generate a list of summaries to pick from.
 * @param sections An array of sections.
 * @returns An array of section summaries.
 */
export const generateListOfSummaries = (sections: IAVOverviewSectionModel[]) => {
  let summaries: ISectionSummary[] = [];
  if (sections && sections.length) {
    sections.forEach((section, sectionIndex) => {
      summaries.push(
        ...section?.items?.reduce(function (
          acc: Array<ISectionSummary>,
          current: IAVOverviewSectionItemModel,
          itemIndex: number,
        ) {
          if (current.summary)
            acc.push({ key: `${sectionIndex}-${itemIndex}`, text: current.summary });
          return acc;
        },
        []),
      );
    });
  }
  return getDistinct(summaries, (item) => item.text);
};

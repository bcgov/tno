import { IContentModel, IReportInstanceContentModel } from 'tno-core';

import { generateContents } from './generateContent';
import { generateRandom } from './generateRandom';
import { generateReportSection } from './generateReportSection';

interface IGenerateReportInstanceContentOptions {
  _id?: number;
  quantity?: number;
  content?: IContentModel;
}

export const generateReportInstanceContent = (
  options: IGenerateReportInstanceContentOptions,
): IReportInstanceContentModel => {
  const ric: IReportInstanceContentModel = {
    instanceId: 0,
    sectionName: `Section ${options._id}`,
    contentId: options.content?.id ?? 0,
    content: options.content,
    sortOrder: 0,
  };
  return ric;
};

interface IGenerateReportInstanceContentsOptions {
  sections: {
    min: number;
    max: number;
  };
  content: {
    min: number;
    max: number;
  };
}

export const generateReportInstanceContents = (options: IGenerateReportInstanceContentsOptions) => {
  const quantity = generateRandom(options.sections.min, options.sections.max);
  const sections = Array.from(Array(quantity).keys()).map((key) => {
    return generateReportSection({ _id: key });
  });
  const sectionDict: Record<string, IReportInstanceContentModel[]> = {};

  // Flatten sections into array of content.
  sections.forEach((section) => {
    // Generate random content for this section.
    const content = generateContents(options.content.min, options.content.max).sort((a, b) =>
      a.sourceId ?? 0 < (b.sourceId ?? 0) ? -1 : a.sourceId ?? 0 > (b.sourceId ?? 0) ? 1 : 0,
    );
    const ric = content.map((c) => generateReportInstanceContent({ _id: section.id, content: c }));
    sectionDict[section.name] = ric;
  });

  return Object.entries(sectionDict)
    .map(([k, v]) => v)
    .flat()
    .sort((a, b) => (a.sectionName < b.sectionName ? -1 : a.sectionName > b.sectionName ? 1 : 0));
};

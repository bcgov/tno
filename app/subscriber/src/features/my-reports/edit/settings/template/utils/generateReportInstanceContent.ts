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
    sectionName: `Section ${generateRandom(1, 10)}`,
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
  const quantity = Math.ceil(
    Math.random() * (options.sections.max - options.sections.min) + options.sections.min,
  );
  const sections = Array.from(Array(quantity).keys()).map((key) => {
    return generateReportSection({ _id: key });
  });
  const sectionsFlat: Record<string, IReportInstanceContentModel[]> = {};

  // Flatten sections into array of content.
  sections.forEach((section) => {
    // Generate random content for this section.
    const content = generateContents(options.content.min, options.content.max);
    const ric = content.map((c) => generateReportInstanceContent({ _id: section.id, content: c }));
    sectionsFlat[section.name] = ric;
  });

  return Object.entries(sectionsFlat)
    .map(([k, v]) => v)
    .flat();
};

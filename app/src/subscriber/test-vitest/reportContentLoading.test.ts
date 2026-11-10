import { IReportInstanceContentModel, IReportModel, ReportStatusName } from 'tno-core';
import { describe, expect, it } from 'vitest';

import { defaultReport } from '../features/my-reports/constants';
import { IReportForm } from '../features/my-reports/interfaces';
import { sanitizeReport, shouldSaveContent, toForm } from '../features/my-reports/utils';

const story = (contentId: number, sectionName = 'section-1'): IReportInstanceContentModel =>
  ({
    instanceId: 7,
    contentId,
    sectionName,
    sortOrder: contentId,
    content: { id: contentId, headline: `Headline ${contentId}`, publishedOn: '2026-01-01' },
  }) as unknown as IReportInstanceContentModel;

const report = (content: IReportInstanceContentModel[]): IReportModel =>
  ({
    ...defaultReport(1, 1),
    id: 593,
    instances: [{ id: 7, reportId: 593, status: ReportStatusName.Pending, content }],
  }) as unknown as IReportModel;

const save = (form: IReportForm, baseline?: IReportInstanceContentModel[], editable = true) =>
  shouldSaveContent(form, sanitizeReport(form), baseline, editable);

describe('report stories are loaded only when needed', () => {
  it('treats an instance without stories as not loaded', () => {
    expect(toForm(report([])).contentLoaded).toBe(false);
    expect(toForm(report([story(1)])).contentLoaded).toBe(true);
    expect(toForm(report([story(1)]), false, false).contentLoaded).toBe(false);
  });

  it('never saves stories that were not loaded, even when the list looks empty or changed', () => {
    const form = toForm(report([]));
    expect(save(form, undefined)).toBe(false);
    const changed = { ...form, instances: [{ ...form.instances[0], content: [] }] };
    expect(save(changed, form.instances[0].content)).toBe(false);
  });

  it('does not save unchanged stories with a settings change', () => {
    const form = toForm(report([story(1), story(2)]));
    const baseline = form.instances[0].content;
    const settingsChanged = { ...form, name: 'Renamed', sections: [...form.sections] };
    expect(save(settingsChanged, baseline)).toBe(false);
  });

  it('saves loaded stories once they change, including removing them all', () => {
    const form = toForm(report([story(1), story(2)]));
    const baseline = form.instances[0].content;
    const removedOne = {
      ...form,
      instances: [{ ...form.instances[0], content: baseline.slice(1) }],
    };
    expect(save(removedOne, baseline)).toBe(true);
    const removedAll = { ...form, instances: [{ ...form.instances[0], content: [] }] };
    expect(save(removedAll, baseline)).toBe(true);
  });

  it('never saves the stories of an instance that can no longer change', () => {
    const form = toForm(report([story(1)]));
    const changed = { ...form, instances: [{ ...form.instances[0], content: [] }] };
    expect(save(changed, form.instances[0].content, false)).toBe(false);
  });
});

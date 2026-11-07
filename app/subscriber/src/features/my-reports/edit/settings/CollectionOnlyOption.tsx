import { FormikCheckbox } from 'tno-core';

/** Marks a report as collecting content for other reports only; shared by the Info and Preferences tabs. */
export const CollectionOnlyOption = () => {
  return (
    <div className="collection-only">
      <FormikCheckbox
        id="chk-collection-only"
        name="settings.collectionOnly"
        label="Collection only (this report gathers content for other reports and is never sent)"
      />
      <p className="info">
        Use this for a report that only collects stories by section for media analytics in other
        reports. It still runs and collects content on its schedule, but it is never emailed, and
        its preview lists headlines only.
      </p>
    </div>
  );
};

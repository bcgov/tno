import moment from 'moment';
import { IReportModel, IReportScheduleModel } from 'tno-core';

const WeekDays: Record<string, number> = {
  Sunday: 0,
  Monday: 1,
  Tuesday: 2,
  Wednesday: 3,
  Thursday: 4,
  Friday: 5,
  Saturday: 6,
};

interface ISchedule {
  weekDayNum: number;
  time: string;
}

function getNextReportSend(report: IReportModel): string {
  // Set constants with the last sent info. If it's null consider the actual date
  const sentOn: string | Date | undefined = report.instances.length
    ? report.instances[0].sentOn
    : undefined;

  const lastSent: Date = sentOn === undefined ? new Date() : new Date(sentOn);

  const lastWeekDay = lastSent.toLocaleDateString('en-US', { weekday: 'long' });

  const lastTimeSent = lastSent.toLocaleTimeString('en-US', {
    timeZone: 'UTC',
    hour12: false,
    hour: '2-digit',
    minute: '2-digit',
    second: '2-digit',
  });

  // Get all the schedules and order them by weekday and time
  const sends: ISchedule[] = [];

  report.events.forEach((s: IReportScheduleModel) => {
    const weekDays = s.runOnWeekDays.split(',');
    for (let i = 0; i < weekDays.length; i++) {
      const key: string = weekDays[i].trim();
      if (s.settings.autoSend) {
        sends.push({
          weekDayNum: WeekDays[key],
          time: s.startAt ? s.startAt : '',
        });
      }
    }
  });

  sends.sort((a, b) => a.weekDayNum - b.weekDayNum || a.time.localeCompare(b.time));

  // Run through the available schedule day, and add the proper to the variable
  // Calculate the day difference between the last send/future send
  let nextSend: ISchedule | null = null;
  let daysToAdd: number = 0;
  for (let i = 0; i < sends.length; i++) {
    if (sends[i].weekDayNum > WeekDays[lastWeekDay]) {
      daysToAdd = sends[i].weekDayNum - WeekDays[lastWeekDay];
      nextSend = sends[i];
      break;
    }

    if (sends[i].weekDayNum === WeekDays[lastWeekDay]) {
      if (sends[i].time > lastTimeSent) {
        daysToAdd = sends[i].weekDayNum - WeekDays[lastWeekDay];
        nextSend = sends[i];
        break;
      }
    }
    if (nextSend === null) {
      // Advance one week and set to the first available day
      daysToAdd = sends[0].weekDayNum + 7 - WeekDays[lastWeekDay];
      nextSend = sends[0];
    }
  }

  // Add the day difference to the last sent date and format correctly to display
  const nextSendDate = new Date(lastSent.setDate(lastSent.getDate() + daysToAdd));
  const year = nextSendDate.toLocaleDateString('en-US', { year: 'numeric' });
  const month = nextSendDate.toLocaleDateString('en-US', { month: '2-digit' });
  const day = nextSendDate.toLocaleDateString('en-US', { day: '2-digit' });
  const nextSendDateTime = new Date(`${year}-${month}-${day}T${nextSend?.time}.000Z`);
  return moment(nextSendDateTime).utc(false).format('yyyy-MM-DD hh:mm:ssA');
}

/**
 * Calculates the next time the report will be sent.
 * @param report Report model.
 * @returns The next send on date.
 */
export const calcNextSend = (report: IReportModel) => {
  const schedules = report.events.filter(
    (s: IReportScheduleModel) => s.isEnabled && s.settings.autoSend,
  );
  if (!schedules.length) return 'NA';
  return getNextReportSend(report);
};

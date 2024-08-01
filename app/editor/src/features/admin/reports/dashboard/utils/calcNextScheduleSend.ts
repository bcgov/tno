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

/**
 * Calculates the next time the schedule will be sent.
 * @param report Report.
 * @param schedule Report schedule.
 * @returns The next send on date.
 */
export const calcNextScheduleSend = (report: IReportModel, schedule: IReportScheduleModel) => {
  if (!schedule.isEnabled) return undefined;

  const currentTime: Date = new Date();
  const currentWeekDay = currentTime.toLocaleDateString('en-US', {
    timeZone: 'America/Vancouver',
    weekday: 'long',
  });
  const currentTimeOfDay = currentTime.toLocaleTimeString('en-US', {
    // TODO: Store User's preferred timezone in profile/settings to use for converting/storing
    // Server currently compares time in server local time, which is PST. Hence, use PST.
    timeZone: 'America/Vancouver',
    hour12: false,
    hour: '2-digit',
    minute: '2-digit',
    second: '2-digit',
  });

  // Get all the schedules and order them by weekday and time
  const sends: ISchedule[] = [];

  const weekDays = schedule.runOnWeekDays.split(',');
  for (let i = 0; i < weekDays.length; i++) {
    const key: string = weekDays[i].trim();
    sends.push({
      weekDayNum: WeekDays[key],
      time: schedule.startAt ? schedule.startAt : '',
    });
  }

  sends.sort((a, b) => a.weekDayNum - b.weekDayNum || a.time.localeCompare(b.time));

  // Run through the available schedule day, and add the proper to the variable
  // Calculate the day difference between now and next send
  let nextSend: ISchedule | null = null;
  let daysToAdd: number = 0;
  for (let i = 0; i < sends.length; i++) {
    if (sends[i].weekDayNum > WeekDays[currentWeekDay]) {
      daysToAdd = sends[i].weekDayNum - WeekDays[currentWeekDay];
      nextSend = sends[i];
      break;
    }

    if (sends[i].weekDayNum === WeekDays[currentWeekDay]) {
      if (sends[i].time > currentTimeOfDay) {
        daysToAdd = sends[i].weekDayNum - WeekDays[currentWeekDay];
        nextSend = sends[i];
        break;
      }
    }
    if (nextSend === null) {
      // Advance one week and set to the first available day
      daysToAdd = sends[0].weekDayNum + 7 - WeekDays[currentWeekDay];
      nextSend = sends[0];
    }
  }

  // Add the day difference to the last sent date and format correctly to display
  const nextSendDate = new Date(currentTime.setDate(currentTime.getDate() + daysToAdd));
  const year = nextSendDate.toLocaleDateString('en-US', { year: 'numeric' });
  const month = nextSendDate.toLocaleDateString('en-US', { month: '2-digit' });
  const day = nextSendDate.toLocaleDateString('en-US', { day: '2-digit' });
  const nextSendDateTime = new Date(`${year}-${month}-${day}T${nextSend?.time}.000Z`);
  return moment(nextSendDateTime).utc(false);
};

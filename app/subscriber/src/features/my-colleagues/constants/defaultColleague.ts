import { IColleagueForm } from '../interfaces';

export const defaultColleague = (): IColleagueForm => {
  var colleague: IColleagueForm = {
    colleagueEmail: '',
  };

  return colleague;
};

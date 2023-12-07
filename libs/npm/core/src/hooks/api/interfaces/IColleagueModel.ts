export interface IColleagueModel {
  colleague:
    | {
        id: number | undefined;
        username: string | undefined;
        email: string | undefined;
      }
    | undefined;
}
